class Program
{
    static void Main()
    {
        Ticket firstTicket = new IncidentTicket(
            "Portale non disponibile",
            "Gli utenti ricevono un errore durante l'accesso.",
            priority: 1,
            DateTimeOffset.UtcNow,
            "Customer Portal");

        Ticket secondTicket = new ServiceRequestTicket(
            "Attivazione ambiente di test",
            "Serve un nuovo ambiente per il team di sviluppo.",
            priority: 2,
            DateTimeOffset.UtcNow,
            "Test Environment");

        Ticket thirdTicket = new MaintenanceRequestTicket(
            "Aggiornamento server",
            "Applicazione delle patch di sicurezza sui server di produzione.",
            priority: 3,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(3));

        Ticket[] tickets = [firstTicket, secondTicket, thirdTicket];

        ITicketFormatter formatter = new DetailedTicketFormatter();

        foreach (Ticket ticket in tickets)
        {
            PrintTicket(ticket, formatter);
            Console.WriteLine();
        }

        Console.WriteLine("=== Formato compatto ===");
        formatter = new CompactTicketFormatter();

        foreach (Ticket ticket in tickets)
        {
            PrintTicket(ticket, formatter);
        }

        Console.WriteLine();
        Console.WriteLine("=== Formato scadenza ===");
        formatter = new DeadlineTicketFormatter();

        foreach (Ticket ticket in tickets)
        {
            PrintTicket(ticket, formatter);
        }

        firstTicket.Close();
        Console.WriteLine();
        Console.WriteLine(
            $"Il ticket '{firstTicket.Title}' è chiuso: " +
            $"{firstTicket.IsClosed}");

        Console.WriteLine();
        Console.WriteLine("=== Boxing e unboxing ===");

        object boxedPriority = 2;

        if (boxedPriority is int priority)
        {
            Console.WriteLine($"is int soddisfatto: {priority}");
        }

        // Il box contiene un long: il pattern is int non è soddisfatto.
        boxedPriority = 2L;

        if (boxedPriority is int matchedInt)
        {
            Console.WriteLine($"is int soddisfatto: {matchedInt}");
        }
        else
        {
            Console.WriteLine("is int non soddisfatto: il box contiene un long.");
        }

        // Il cast diretto a int fallisce con InvalidCastException.
        int unboxed = (int)boxedPriority;
        Console.WriteLine(unboxed);
    }

    static void PrintTicket(Ticket ticket, ITicketFormatter formatter)
    {
        Console.WriteLine(formatter.Format(ticket));
    }
}

public abstract class Ticket
{
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
        string title,
        string description,
        int priority,
        DateTimeOffset createdAt)
    {
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

        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        CreatedAt = createdAt;
    }

    public bool UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
        {
            return false;
        }

        Description = newDescription.Trim();
        return true;
    }

    public bool ChangePriority(int newPriority)
    {
        if (newPriority < 1 || newPriority > 3)
        {
            return false;
        }

        Priority = newPriority;
        return true;
    }

    public void Close()
    {
        IsClosed = true;
    }

    public virtual string GetSummary()
    {
        return
            $"Tipo: {TypeName}\n" +
            $"Titolo: {Title}\n" +
            $"Descrizione: {Description}\n" +
            $"Priorità: {PriorityLabel}";
    }

    public override string ToString()
    {
        return $"{TypeName}: {Title} ({PriorityLabel})";
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
        string title,
        string description,
        int priority,
        DateTimeOffset createdAt,
        string affectedService)
        : base(title, description, priority, createdAt)
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
        string title,
        string description,
        int priority,
        DateTimeOffset createdAt,
        string requestedService)
        : base(title, description, priority, createdAt)
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
        string title,
        string description,
        int priority,
        DateTimeOffset createdAt,
        DateTimeOffset scheduledFor)
        : base(title, description, priority, createdAt)
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

public interface ITicketFormatter
{
    string Format(Ticket ticket);
}

public sealed class DetailedTicketFormatter : ITicketFormatter
{
    public string Format(Ticket ticket)
    {
        return
            $"{ticket.GetSummary()}\n" +
            $"Creato: {ticket.CreatedAt:dd/MM/yyyy HH:mm}\n" +
            $"SLA: {ticket.TargetResolutionHours} ore\n" +
            $"Scadenza: {ticket.DueAt:dd/MM/yyyy HH:mm}\n" +
            $"Chiuso: {(ticket.IsClosed ? "Sì" : "No")}";
    }
}

public sealed class CompactTicketFormatter : ITicketFormatter
{
    public string Format(Ticket ticket)
    {
        return
            $"{ticket} - " +
            $"scadenza {ticket.DueAt:dd/MM/yyyy HH:mm}";
    }
}

public sealed class DeadlineTicketFormatter : ITicketFormatter
{
    public string Format(Ticket ticket)
    {
        return
            $"[{ticket.TypeName}] {ticket.Title} - " +
            $"{ticket.TargetResolutionHours} ore disponibili";
    }
}