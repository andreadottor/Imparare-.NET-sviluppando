class Program
{
    static void Main()
    {
        Console.WriteLine("=== Nuovo ticket ===");

        string title = ReadRequiredText("Inserisci il titolo:");

        Console.WriteLine("Inserisci la descrizione:");
        string description = Console.ReadLine() ?? "";

        int priority = ReadIntInRange(
            "Inserisci la priorità da 1 a 3:",
            minimumValue: 1,
            maximumValue: 3);

        int estimatedHours = ReadPositiveInt(
            "Inserisci le ore stimate:");

        var ticket = new Ticket(
            title,
            description,
            priority,
            DateTimeOffset.UtcNow);

        var estimate = new WorkEstimate(estimatedHours);

        if (ReadConfirmation("Vuoi chiudere subito il ticket? S/N"))
        {
            ticket.Close();
        }

        TicketSnapshot snapshot = ticket.CreateSnapshot(estimate);
        PrintTicketSummary(snapshot, ticket.RequiresEvaluation(estimate));
    }

    static string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string value = Console.ReadLine() ?? "";

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            Console.WriteLine("Il valore non può essere vuoto.");
        }
    }

    static int ReadIntInRange(string prompt, int minimumValue, int maximumValue)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string input = Console.ReadLine() ?? "";

            if (!int.TryParse(input, out int value))
            {
                Console.WriteLine("Il valore deve essere un numero intero.");
                continue;
            }

            if (value < minimumValue || value > maximumValue)
            {
                Console.WriteLine(
                    $"Il valore deve essere compreso tra {minimumValue} " +
                    $"e {maximumValue}.");
                continue;
            }

            return value;
        }
    }

    static int ReadPositiveInt(string prompt)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int value) && value > 0)
            {
                return value;
            }

            Console.WriteLine(
                "Il valore deve essere un numero intero maggiore di zero.");
        }
    }

    static bool ReadConfirmation(string prompt)
    {
        Console.WriteLine();
        Console.WriteLine(prompt);

        string answer = Console.ReadLine() ?? "";

        return string.Equals(
            answer.Trim(),
            "S",
            StringComparison.OrdinalIgnoreCase);
    }

    static void PrintTicketSummary(
        TicketSnapshot snapshot,
        bool requiresEvaluation)
    {
        Console.WriteLine();
        Console.WriteLine("Ticket inserito:");
        Console.WriteLine($"Titolo: {snapshot.Title}");
        Console.WriteLine($"Descrizione: {snapshot.Description}");
        Console.WriteLine($"Priorità: {snapshot.PriorityLabel}");
        Console.WriteLine(
            $"Creato il: {snapshot.CreatedAt:dd/MM/yyyy HH:mm:ss zzz}");
        Console.WriteLine($"Ore stimate: {snapshot.EstimatedHours}");
        Console.WriteLine($"Chiuso: {(snapshot.IsClosed ? "Sì" : "No")}");

        if (requiresEvaluation)
        {
            Console.WriteLine(
                "Il ticket richiede la valutazione di un operatore.");
        }
    }
}

public class Ticket
{
    public string Title { get; }
    public string Description { get; private set; }
    public int Priority { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public bool IsClosed { get; private set; }

    public bool IsUrgent => Priority == 1;

    public string PriorityLabel => Priority switch
    {
        1 => "Alta",
        2 => "Media",
        3 => "Bassa",
        _ => "Sconosciuta"
    };

    public Ticket(
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

    public bool RequiresEvaluation(WorkEstimate estimate)
    {
        return IsUrgent || estimate.IsLong;
    }

    public TicketSnapshot CreateSnapshot(WorkEstimate estimate)
    {
        return new TicketSnapshot(
            Title,
            Description,
            PriorityLabel,
            CreatedAt,
            estimate.Hours,
            IsClosed);
    }
}

public readonly struct WorkEstimate
{
    public int Hours { get; }

    public bool IsLong => Hours > 8;

    public WorkEstimate(int hours)
    {
        if (hours <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(hours),
                "Le ore devono essere maggiori di zero.");
        }

        Hours = hours;
    }

    public override string ToString()
    {
        return $"{Hours} ore";
    }
}

public record TicketSnapshot(
    string Title,
    string Description,
    string PriorityLabel,
    DateTimeOffset CreatedAt,
    int EstimatedHours,
    bool IsClosed);