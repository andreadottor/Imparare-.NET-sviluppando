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
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 30)
        {
            throw new ArgumentException(
                "Il codice è obbligatorio e non può superare 30 caratteri.",
                nameof(code));
        }

        code = code.Trim();

        if (code is "." or ".." || code.Any(character =>
            !char.IsAsciiLetterOrDigit(character) && character != '-' && character != '.'))
        {
            throw new ArgumentException(
                "Usare lettere ASCII, numeri, trattini e punti; '.' e '..' non sono codici validi.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200)
        {
            throw new ArgumentException(
                "Il titolo è obbligatorio e non può superare 200 caratteri.",
                nameof(title));
        }

        if (priority < 1 || priority > 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                priority,
                "La priorità deve essere compresa tra 1 e 3.");
        }

        ArgumentNullException.ThrowIfNull(description);

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