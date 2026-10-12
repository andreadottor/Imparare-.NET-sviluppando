class Program
{
    static async Task Main()
    {
        // Ogni scenario usa istanze separate per restare indipendente
        // dagli effetti degli altri.
        await RunIoFailureDemoAsync();
        await RunDuplicateDemoAsync();
        RunInvalidPriorityDemo();
        await RunStatusDemoAsync();
        await RunCancellationDemoAsync();
        await RunParallelSearchDemoAsync();
        await RunBatchSaveDemoAsync();
        RunFireAndForgetDemo();
    }

    // Scenario 1: il gateway genera IOException perché il codice è INC-FAIL.
    // RegisterAsync rimuove il ticket dall'archivio e rilancia con throw;
    // Main esegue il catch (IOException), salta il messaggio di successo
    // ed esegue comunque il blocco finally.
    static async Task RunIoFailureDemoAsync()
    {
        Console.WriteLine("== Scenario 1: errore di I/O ==");

        var registry = new TicketRegistry<Ticket>();
        var gateway = new TicketGateway();
        var service = new TicketService(registry, gateway);

        try
        {
            Ticket ticket = CreateSampleTicket("INC-FAIL");

            await service.RegisterAsync(ticket);

            // Non viene raggiunto: SaveAsync genera IOException prima.
            Console.WriteLine($"Ticket {ticket.Code} registrato.");
        }
        catch (IOException exception)
        {
            Console.WriteLine($"Salvataggio non riuscito: {exception.Message}");
        }
        finally
        {
            Console.WriteLine("Operazione terminata.");
        }
    }

    // Scenario 2: due ticket con lo stesso codice INC-001. Il secondo Add
    // genera DuplicateTicketException. Main esegue il catch dedicato,
    // senza entrare in quello destinato agli errori di I/O.
    static async Task RunDuplicateDemoAsync()
    {
        Console.WriteLine("== Scenario 2: codice duplicato ==");

        var registry = new TicketRegistry<Ticket>();
        var gateway = new TicketGateway();
        var service = new TicketService(registry, gateway);

        try
        {
            await service.RegisterAsync(CreateSampleTicket("INC-001"));
            await service.RegisterAsync(CreateSampleTicket("INC-001"));
        }
        catch (DuplicateTicketException exception)
        {
            Console.WriteLine(
                $"Registrazione non riuscita: {exception.Message}");
        }
        catch (IOException exception)
        {
            // Non eseguito: l'eccezione è DuplicateTicketException.
            Console.WriteLine($"Salvataggio non riuscito: {exception.Message}");
        }
        finally
        {
            Console.WriteLine("Operazione terminata.");
        }
    }

    // Scenario 3: priorità 5. ArgumentOutOfRangeException nasce nel
    // costruttore di Ticket, dove viene validata la priorità, ma viene
    // gestita qui: nel debugger si leggono Message, ParamName,
    // ActualValue e StackTrace.
    static void RunInvalidPriorityDemo()
    {
        Console.WriteLine("== Scenario 3: priorità non valida ==");

        try
        {
            _ = new IncidentTicket(
                "INC-002",
                "Priorità errata",
                "Valore fuori intervallo.",
                priority: 5,
                new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero),
                "Customer Portal");
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine($"Dati non validi: {exception.Message}");
        }
        finally
        {
            Console.WriteLine("Operazione terminata.");
        }
    }

    // Scenario 4: prima di await esiste un Task<string> (la promessa di un
    // risultato); dopo await esiste una string (il risultato prodotto).
    static async Task RunStatusDemoAsync()
    {
        Console.WriteLine("== Scenario 4: stato del ticket ==");

        var registry = new TicketRegistry<Ticket>();
        var gateway = new TicketGateway();
        var service = new TicketService(registry, gateway);

        // Prima di await: Task<string>.
        Task<string> statusTask = service.GetStatusAsync(
            "INC-001",
            CancellationToken.None);

        // Dopo await: string.
        string status = await statusTask;

        Console.WriteLine(status);
    }

    // Scenario 5: il gateway attende più a lungo del limite, quindi la
    // richiesta di annullamento viene osservata. RegisterAsync ripristina
    // l'archivio e Main distingue l'annullamento dall'errore di I/O.
    static async Task RunCancellationDemoAsync()
    {
        Console.WriteLine("== Scenario 5: annullamento ==");

        var registry = new TicketRegistry<Ticket>();
        var gateway = new TicketGateway();
        var service = new TicketService(registry, gateway);

        // Chi crea il source ne è proprietario ed è responsabile di
        // rilasciarlo: using garantisce Dispose anche in caso di
        // eccezione. I metodi che ricevono solo il token non lo
        // possiedono e devono limitarsi a propagarlo.
        using var cancellationTokenSource = new CancellationTokenSource();

        cancellationTokenSource.CancelAfter(100);

        try
        {
            Ticket ticket = CreateSampleTicket("INC-003");

            // Il gateway attende 300 ms, oltre il limite di 100 ms:
            // l'annullamento viene osservato durante Task.Delay.
            await service.RegisterAsync(
                ticket,
                cancellationTokenSource.Token);

            Console.WriteLine($"Ticket {ticket.Code} registrato.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operazione annullata.");
        }
        catch (IOException exception)
        {
            Console.WriteLine($"Salvataggio non riuscito: {exception.Message}");
        }
        finally
        {
            Console.WriteLine("Operazione terminata.");
        }
    }

    // Scenario 6: due ricerche indipendenti avviate insieme. Rispetto a
    // due chiamate attese una dopo l'altra, qui i task procedono in
    // parallelo. Dopo WhenAll si attendono i singoli task, senza .Result.
    static async Task RunParallelSearchDemoAsync()
    {
        Console.WriteLine("== Scenario 6: ricerche parallele ==");

        var registry = new TicketRegistry<Ticket>();
        var gateway = new TicketGateway();
        var service = new TicketService(registry, gateway);

        await service.RegisterAsync(CreateSampleTicket("INC-001"));

        Task<Ticket?> firstTask =
            service.FindAsync("INC-001", CancellationToken.None);

        Task<Ticket?> secondTask =
            service.FindAsync("INC-002", CancellationToken.None);

        await Task.WhenAll(firstTask, secondTask);

        // I task sono già completati: attenderli restituisce subito il
        // valore, senza bloccare il thread come farebbe .Result.
        Ticket? first = await firstTask;
        Ticket? second = await secondTask;

        Console.WriteLine(first?.GetSummary() ?? "INC-001 non trovato.");
        Console.WriteLine(second?.GetSummary() ?? "INC-002 non trovato.");
    }

    // Scenario 7: salvataggio di più ticket.
    static async Task RunBatchSaveDemoAsync()
    {
        Console.WriteLine("== Scenario 7: salvataggio multiplo ==");

        var gateway = new TicketGateway();

        List<Ticket> tickets = new()
        {
            CreateSampleTicket("INC-010"),
            CreateSampleTicket("INC-011")
        };

        // ForEach richiede un Action<Ticket>, un delegato che restituisce
        // void. Una lambda async assegnata a questo tipo diventa async
        // void: ForEach non può attenderla e le eccezioni del task non
        // sono osservabili. Per questo evitiamo la forma seguente:
        //
        // tickets.ForEach(async ticket =>
        // {
        //     await gateway.SaveAsync(ticket, CancellationToken.None);
        // });

        // Prima alternativa: foreach che attende un salvataggio alla volta.
        foreach (Ticket ticket in tickets)
        {
            await gateway.SaveAsync(ticket, CancellationToken.None);
        }

        // Seconda alternativa: valida solo se i salvataggi sono
        // indipendenti. Select crea i task, ToArray li materializza e
        // WhenAll ne attende il completamento.
        Task[] saveTasks = tickets
            .Select(ticket => gateway.SaveAsync(ticket, CancellationToken.None))
            .ToArray();

        await Task.WhenAll(saveTasks);

        Console.WriteLine($"Salvati {tickets.Count} ticket.");
    }

    // Scenario 8: chiamata senza await.
    static void RunFireAndForgetDemo()
    {
        Console.WriteLine("== Scenario 8: chiamata senza await ==");

        var gateway = new TicketGateway();
        Ticket ticket = CreateSampleTicket("INC-020");

        // Senza await il compilatore segnala il warning CS4014:
        // gateway.SaveAsync(ticket, CancellationToken.None);

        // Il discard _ silenzia il warning, ma non attende il
        // completamento e non gestisce le eccezioni del task:
        // l'operazione può restare incompleta o fallire in silenzio.
        _ = gateway.SaveAsync(ticket, CancellationToken.None);

        Console.WriteLine("Chiamata avviata senza attendere.");
    }

    static Ticket CreateSampleTicket(string code)
    {
        return new IncidentTicket(
            code,
            "Portale non disponibile",
            "Gli utenti non riescono ad accedere.",
            priority: 1,
            new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero),
            "Customer Portal");
    }
}

public sealed class TicketService
{
    private readonly TicketRegistry<Ticket> _registry;
    private readonly TicketGateway _gateway;

    public TicketService(TicketRegistry<Ticket> registry, TicketGateway gateway)
    {
        _registry = registry;
        _gateway = gateway;
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

    /// <summary>
    /// Restituisce lo stato corrente di un ticket.
    /// </summary>
    /// <param name="code">Il codice del ticket.</param>
    /// <param name="ct">
    /// Il token usato per richiedere l'annullamento.
    /// </param>
    /// <returns>Il task che produce lo stato del ticket.</returns>
    public Task<string> GetStatusAsync(string code, CancellationToken ct = default)
    {
        return _gateway.GetStatusAsync(code, ct);
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

    public async Task<string> GetStatusAsync(string code, CancellationToken ct)
    {
        await Task.Delay(250, ct);
        return $"{code}: Open";
    }
}

public sealed class TicketRegistry<TTicket>
    where TTicket : Ticket
{
    private readonly Dictionary<string, TTicket> _tickets =
        new(StringComparer.OrdinalIgnoreCase);

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
            throw new DuplicateTicketException(
                $"Il ticket {ticket.Code} è già presente.");
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
