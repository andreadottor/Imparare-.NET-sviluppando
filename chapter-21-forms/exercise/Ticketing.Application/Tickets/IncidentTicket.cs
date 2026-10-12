namespace Ticketing.Application.Tickets;

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
        : base(code, title, description, priority, createdAt)
    {
        if (string.IsNullOrWhiteSpace(affectedService) || affectedService.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Il servizio coinvolto è obbligatorio e non può superare 100 caratteri.",
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