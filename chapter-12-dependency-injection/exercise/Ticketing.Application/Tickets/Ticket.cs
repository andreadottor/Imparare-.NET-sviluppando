namespace Ticketing.Application.Tickets;

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

        Code        = code.Trim();
        Title       = title.Trim();
        Description = description.Trim();
        Priority    = priority;
        CreatedAt   = createdAt;
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