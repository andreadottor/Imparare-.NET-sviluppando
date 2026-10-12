class Program
{
    static async Task Main()
    {
        var registry = new TicketRegistry<Ticket>();
        var gateway  = new TicketGateway();
        var service  = new TicketService(registry, gateway);

        try
        {
            Ticket ticket = new IncidentTicket(
                "INC-001",
                "Portale non disponibile",
                "Gli utenti non riescono ad accedere.",
                priority: 1,
                new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero),
                "Customer Portal");

            await service.RegisterAsync(ticket);

            Console.WriteLine($"Ticket {ticket.Code} registrato.");

            Ticket? foundTicket = await service.FindAsync("inc-001");

            if (foundTicket is not null)
            {
                Console.WriteLine(foundTicket.GetSummary());
            }
            else
            {
                Console.WriteLine("Ticket non trovato.");
            }
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine($"Dati non validi: {exception.Message}");
        }
        catch (DuplicateTicketException exception)
        {
            Console.WriteLine($"Registrazione non riuscita: {exception.Message}");
        }
        catch (IOException exception)
        {
            Console.WriteLine($"Salvataggio non riuscito: {exception.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operazione annullata.");
        }
        finally
        {
            Console.WriteLine("Operazione terminata.");
        }
    }
}

public sealed class TicketService
{
    private readonly TicketRegistry<Ticket> _registry;
    private readonly TicketGateway _gateway;

    public TicketService(TicketRegistry<Ticket> registry, TicketGateway gateway)
    {
        _registry = registry;
        _gateway  = gateway;
    }

    /// <summary>
    /// Registra e salva un nuovo ticket.
    /// </summary>
    /// <param name="ticket">Il ticket da registrare.</param>
    /// <param name="ct">
    /// Il token usato per richiedere l'annullamento.
    /// Se omesso, l'operazione non può essere annullata dal chiamante.
    /// </param>
    /// <returns>Il task che rappresenta l'operazione.</returns>
    /// <exception cref="DuplicateTicketException">
    /// Viene generata quando il codice è già presente.
    /// </exception>
    /// <exception cref="IOException">
    /// Viene generata quando il salvataggio non riesce.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Viene generata quando l'operazione viene annullata.
    /// </exception>
    public async Task RegisterAsync(Ticket ticket, CancellationToken ct = default)
    {
        _registry.Add(ticket);

        try
        {
            await _gateway.SaveAsync(ticket, ct);
        }
        catch
        {
            _registry.Remove(ticket.Code);
            throw;
        }
    }

    /// <summary>
    /// Cerca un ticket tramite il codice.
    /// </summary>
    /// <param name="code">Il codice da cercare.</param>
    /// <param name="ct">
    /// Il token usato per richiedere l'annullamento.
    /// Se omesso, l'operazione non può essere annullata dal chiamante.
    /// </param>
    /// <returns>
    /// Il ticket trovato oppure <see langword="null"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Viene generata quando l'operazione viene annullata.
    /// </exception>
    public async Task<Ticket?> FindAsync(string code, CancellationToken ct = default)
    {
        await _gateway.WaitForResponseAsync(ct);

        _registry.TryGet(code, out Ticket? ticket);
        return ticket;
    }
}

public sealed class TicketGateway
{
    public async Task SaveAsync(Ticket ticket, CancellationToken ct)
    {
        await Task.Delay(300, ct);

        if (ticket.Code.Equals("INC-FAIL", StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException("Il sistema esterno non è disponibile.");
        }
    }

    public async Task WaitForResponseAsync(CancellationToken ct)
    {
        await Task.Delay(200, ct);
    }
}

public sealed class TicketRegistry<TTicket>
    where TTicket : Ticket
{
    private readonly Dictionary<string, TTicket> _tickets = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Aggiunge un ticket all'archivio.
    /// </summary>
    /// <param name="ticket">Il ticket da aggiungere.</param>
    /// <exception cref="DuplicateTicketException">
    /// Viene generata quando il codice è già presente.
    /// </exception>
    public void Add(TTicket ticket)
    {
        if (!_tickets.TryAdd(ticket.Code, ticket))
        {
            throw new DuplicateTicketException($"Il ticket {ticket.Code} è già presente.");
        }
    }

    public bool TryGet(string code, out TTicket? ticket)
    {
        return _tickets.TryGetValue(code, out ticket);
    }

    public bool Remove(string code)
    {
        return _tickets.Remove(code);
    }
}

public sealed class DuplicateTicketException : Exception
{
    public DuplicateTicketException()
    {
    }

    public DuplicateTicketException(string message)
        : base(message)
    {
    }

    public DuplicateTicketException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public abstract class Ticket
{
    public string Code { get; }
    public string Title { get; }
    public string Description { get; }
    public int Priority { get; }
    public DateTimeOffset CreatedAt { get; }

    public string PriorityLabel => Priority switch
    {
        1 => "Alta",
        2 => "Media",
        3 => "Bassa",
        _ => "Sconosciuta"
    };

    public abstract string TypeName { get; }

    protected Ticket(
        string code,
        string title,
        string description,
        int priority,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Il codice non può essere vuoto.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Il titolo non può essere vuoto.", nameof(title));
        }

        if (priority < 1 || priority > 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                priority,
                "La priorità deve essere compresa tra 1 e 3.");
        }

        Code = code.Trim();
        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        CreatedAt = createdAt;
    }

    public virtual string GetSummary()
    {
        return
            $"Codice: {Code}\n" +
            $"Tipo: {TypeName}\n" +
            $"Titolo: {Title}\n" +
            $"Priorità: {PriorityLabel}";
    }
}

public sealed class IncidentTicket : Ticket
{
    public string AffectedService { get; }

    public override string TypeName => "Incident";

    public IncidentTicket(
        string code,
        string title,
        string description,
        int priority,
        DateTimeOffset createdAt,
        string affectedService)
        : base(
            code,
            title,
            description,
            priority,
            createdAt)
    {
        if (string.IsNullOrWhiteSpace(affectedService))
        {
            throw new ArgumentException("Il servizio coinvolto non può essere vuoto.", nameof(affectedService));
        }

        AffectedService = affectedService.Trim();
    }

    public override string GetSummary()
    {
        return $"{base.GetSummary()}\nServizio coinvolto: {AffectedService}";
    }
}
