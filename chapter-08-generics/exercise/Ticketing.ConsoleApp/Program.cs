class Program
{
    static void Main()
    {
        Ticket firstTicket = new IncidentTicket(
            "INC-001",
            "Portale non disponibile",
            "Gli utenti ricevono un errore durante l'accesso.",
            priority: 1,
            DateTimeOffset.UtcNow,
            "Customer Portal");

        Ticket secondTicket = new ServiceRequestTicket(
            "REQ-001",
            "Attivazione ambiente di test",
            "Serve un nuovo ambiente per il team di sviluppo.",
            priority: 2,
            DateTimeOffset.UtcNow,
            "Test Environment");

        Ticket thirdTicket = new MaintenanceRequestTicket(
            "MNT-001",
            "Manutenzione pianificata del database",
            "Aggiornamento del server di database in finestra notturna.",
            priority: 3,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(7));

        List<Ticket> initialTickets = [firstTicket, secondTicket, thirdTicket];

        var registry = new TicketRegistry<Ticket>();

        foreach (Ticket ticket in initialTickets)
        {
            bool added = registry.Add(ticket);

            if (!added)
            {
                Console.WriteLine($"Il codice {ticket.Code} è già presente.");
            }
        }

        Console.WriteLine("=== Ticket registrati ===");
        PrintItems(registry.GetAll());
        PrintCount(initialTickets);

        Console.WriteLine();
        Console.WriteLine("=== Ricerca per codice ===");

        if (registry.TryGet("inc-001", out Ticket? foundTicket) &&
            foundTicket is not null)
        {
            Console.WriteLine(foundTicket.GetSummary());
        }
        else
        {
            Console.WriteLine("Ticket non trovato.");
        }

        var services = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Customer Portal",
            "Test Environment"
        };

        bool duplicateAdded = services.Add("customer portal");

        Console.WriteLine();
        Console.WriteLine($"Servizio duplicato aggiunto: {duplicateAdded}");
        PrintCount(services);

        Console.WriteLine();
        Console.WriteLine("=== Codici registrati ===");
        IReadOnlyCollection<string> codes = registry.GetCodes();
        PrintCount(codes);

        var closedTickets = new Stack<Ticket>();
        var processedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        Queue<Ticket> workQueue = registry.CreateQueue();

        Console.WriteLine();
        Console.WriteLine("=== Coda di lavorazione ===");

        while (workQueue.TryDequeue(out Ticket? nextTicket))
        {
            if (!processedCodes.Add(nextTicket.Code))
            {
                Console.WriteLine(
                    $"Il ticket {nextTicket.Code} è già stato elaborato.");
                continue;
            }

            Console.WriteLine($"Elaborazione di {nextTicket.Code}");
            nextTicket.Close();
            closedTickets.Push(nextTicket);
        }

        Console.WriteLine();
        Console.WriteLine("=== Cronologia chiusure (ordine inverso) ===");

        while (closedTickets.TryPop(out Ticket? closedTicket))
        {
            Console.WriteLine(closedTicket.Code);
        }

        Console.WriteLine();
        Console.WriteLine($"Ticket nell'archivio: {registry.Count}");
        Console.WriteLine($"Ticket ancora nella coda: {workQueue.Count}");
    }

    static void PrintItems<T>(IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            Console.WriteLine(item);
        }
    }

    static void PrintCount<T>(IReadOnlyCollection<T> items)
    {
        Console.WriteLine($"Numero di elementi: {items.Count}");
    }
}

public sealed class TicketRegistry<TTicket>
    where TTicket : Ticket
{
    private readonly Dictionary<string, TTicket> _tickets =
        new(StringComparer.OrdinalIgnoreCase);

    public int Count => _tickets.Count;

    public bool Add(TTicket ticket)
    {
        return _tickets.TryAdd(ticket.Code, ticket);
    }

    public bool TryGet(string code, out TTicket? ticket)
    {
        return _tickets.TryGetValue(code, out ticket);
    }

    public bool Remove(string code)
    {
        return _tickets.Remove(code);
    }

    public IEnumerable<TTicket> GetAll()
    {
        return _tickets.Values;
    }

    public IReadOnlyCollection<string> GetCodes()
    {
        // Restituisco una copia: la vista resta stabile e non deve riflettere
        // le modifiche successive all'archivio. Restituire _tickets.Keys darebbe
        // invece una vista "viva", che cambierebbe insieme al dizionario.
        return new List<string>(_tickets.Keys);
    }

    public Queue<TTicket> CreateQueue()
    {
        return new Queue<TTicket>(_tickets.Values);
    }
}

public abstract class Ticket
{
    public string Code { get; }
    public string Title { get; }
    public string Description { get; private set; }
    public int Priority { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public bool IsClosed { get; private set; }

    public string PriorityLabel => Priority switch
    {
        1 => "Alta",
        2 => "Media",
        3 => "Bassa",
        _ => "Sconosciuta"
    };

    public abstract string TypeName { get; }
    public abstract int TargetResolutionHours { get; }

    public DateTimeOffset DueAt => CreatedAt.AddHours(TargetResolutionHours);

    protected Ticket(
        string code,
        string title,
        string description,
        int priority,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "Il codice non può essere vuoto.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Il titolo non può essere vuoto.",
                nameof(title));
        }

        if (priority < 1 || priority > 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                "La priorità deve essere compresa tra 1 e 3.");
        }

        Code = code.Trim();
        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        CreatedAt = createdAt;
    }

    public void Close()
    {
        IsClosed = true;
    }

    public virtual string GetSummary()
    {
        return
            $"Codice: {Code}\n" +
            $"Tipo: {TypeName}\n" +
            $"Titolo: {Title}\n" +
            $"Priorità: {PriorityLabel}\n" +
            $"Scadenza: {DueAt:dd/MM/yyyy HH:mm}";
    }

    public override string ToString()
    {
        return $"[{Code}] {TypeName}: {Title}";
    }
}

public sealed class IncidentTicket : Ticket
{
    public string AffectedService { get; }

    public override string TypeName => "Incident";

    public override int TargetResolutionHours => Priority switch
    {
        1 => 2,
        2 => 8,
        _ => 24
    };

    public IncidentTicket(
        string code,
        string title,
        string description,
        int priority,
        DateTimeOffset createdAt,
        string affectedService)
        : base(code, title, description, priority, createdAt)
    {
        if (string.IsNullOrWhiteSpace(affectedService))
        {
            throw new ArgumentException(
                "Il servizio coinvolto non può essere vuoto.",
                nameof(affectedService));
        }

        AffectedService = affectedService.Trim();
    }

    public override string GetSummary()
    {
        return
            $"{base.GetSummary()}\n" +
            $"Servizio coinvolto: {AffectedService}";
    }
}

public sealed class ServiceRequestTicket : Ticket
{
    public string RequestedService { get; }

    public override string TypeName => "Service request";

    public override int TargetResolutionHours => Priority switch
    {
        1 => 8,
        2 => 24,
        _ => 72
    };

    public ServiceRequestTicket(
        string code,
        string title,
        string description,
        int priority,
        DateTimeOffset createdAt,
        string requestedService)
        : base(code, title, description, priority, createdAt)
    {
        if (string.IsNullOrWhiteSpace(requestedService))
        {
            throw new ArgumentException(
                "Il servizio richiesto non può essere vuoto.",
                nameof(requestedService));
        }

        RequestedService = requestedService.Trim();
    }

    public override string GetSummary()
    {
        return
            $"{base.GetSummary()}\n" +
            $"Servizio richiesto: {RequestedService}";
    }
}

public sealed class MaintenanceRequestTicket : Ticket
{
    public DateTimeOffset ScheduledFor { get; }

    public override string TypeName => "Maintenance request";

    public override int TargetResolutionHours => Priority switch
    {
        1 => 24,
        2 => 72,
        _ => 120
    };

    public MaintenanceRequestTicket(
        string code,
        string title,
        string description,
        int priority,
        DateTimeOffset createdAt,
        DateTimeOffset scheduledFor)
        : base(code, title, description, priority, createdAt)
    {
        if (scheduledFor < createdAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scheduledFor),
                "La data pianificata non può precedere la creazione.");
        }

        ScheduledFor = scheduledFor;
    }

    public override string GetSummary()
    {
        return
            $"{base.GetSummary()}\n" +
            $"Pianificato per: {ScheduledFor:dd/MM/yyyy HH:mm}";
    }
}