class Program
{
    static void Main()
    {
        DateTimeOffset referenceTime = new(2026, 9, 11, 9, 0, 0, TimeSpan.Zero);

        Ticket firstTicket = new IncidentTicket(
            "INC-001",
            "Portale non disponibile",
            "Gli utenti ricevono un errore durante l'accesso.",
            priority: 1,
            referenceTime.AddHours(-1),
            "Customer Portal");

        Ticket secondTicket = new ServiceRequestTicket(
            "REQ-001",
            "Attivazione ambiente di test",
            "Serve un nuovo ambiente per il team di sviluppo.",
            priority: 2,
            referenceTime.AddHours(-3),
            "Test Environment");

        Ticket thirdTicket = new IncidentTicket(
            "INC-002",
            "Esportazione lenta",
            "La generazione del report richiede troppo tempo.",
            priority: 2,
            referenceTime.AddHours(-6),
            "Reporting");

        Ticket fourthTicket = new ServiceRequestTicket(
            "REQ-002",
            "Creazione nuovo utente",
            "Creare l'utenza per una nuova collega.",
            priority: 3,
            referenceTime.AddDays(-1),
            "Identity");

        fourthTicket.Closed += HandleTicketClosed;
        fourthTicket.Close();
        fourthTicket.Closed -= HandleTicketClosed;

        var registry = new TicketRegistry<Ticket>();

        registry.Add(firstTicket);
        registry.Add(secondTicket);
        registry.Add(thirdTicket);
        registry.Add(fourthTicket);

        IEnumerable<Ticket> workQuery = registry
            .GetAll()
            .WhereOpen()
            .OrderBy(ticket => ticket.Priority)
            .ThenBy(ticket => ticket.DueAt)
            .ThenBy(ticket => ticket.Code);

        List<Ticket> workItems = workQuery.ToList();

        Console.WriteLine("=== Coda di lavorazione ===");

        foreach (Ticket ticket in workItems)
        {
            Console.WriteLine(
                $"{ticket.Code} | " +
                $"{ticket.PriorityLabel} | " +
                $"{ticket.DueAt:dd/MM/yyyy HH:mm}");
        }

        bool hasUrgentTickets = registry
            .GetAll()
            .Any(ticket => ticket.Priority == 1 && !ticket.IsClosed);

        int openTicketCount = registry
            .GetAll()
            .Count(ticket => !ticket.IsClosed);

        Console.WriteLine();
        Console.WriteLine($"Ticket aperti: {openTicketCount}");
        Console.WriteLine($"Sono presenti urgenze: {hasUrgentTickets}");

        Ticket? foundTicket = registry
            .GetAll()
            .SingleOrDefault(ticket =>
                string.Equals(
                    ticket.Code,
                    "inc-002",
                    StringComparison.OrdinalIgnoreCase));

        Console.WriteLine();
        Console.WriteLine("=== Ricerca ===");

        if (foundTicket is not null)
        {
            Console.WriteLine(foundTicket.GetSummary());
        }
        else
        {
            Console.WriteLine("Ticket non trovato.");
        }

        var totalsByType = registry
            .GetAll()
            .GroupBy(ticket => ticket.TypeName)
            .Select(group => new
            {
                TypeName = group.Key,
                Total = group.Count(),
                Open = group.Count(ticket => !ticket.IsClosed)
            })
            .OrderBy(item => item.TypeName)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("=== Riepilogo per tipologia ===");

        foreach (var item in totalsByType)
        {
            Console.WriteLine(
                $"{item.TypeName}: " +
                $"{item.Open} aperti su {item.Total}");
        }
    }

    static void HandleTicketClosed(object? sender, EventArgs eventArgs)
    {
        if (sender is Ticket ticket)
        {
            Console.WriteLine($"Evento: {ticket.Code} è stato chiuso.");
        }
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
}

public static class TicketEnumerableExtensions
{
    extension(IEnumerable<Ticket> tickets)
    {
        public IEnumerable<Ticket> WhereOpen()
        {
            return tickets.Where(ticket => !ticket.IsClosed);
        }
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

    public event EventHandler? Closed;

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
        if (IsClosed)
        {
            return;
        }

        IsClosed = true;
        Closed?.Invoke(this, EventArgs.Empty);
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