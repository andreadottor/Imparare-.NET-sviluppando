using Ticketing.Application.Tickets;

namespace Ticketing.Application.Tests.Builders;

public sealed class TicketBuilder
{
    private string _code = "INC-001";
    private string _title = "Portale non disponibile";
    private string _description = "Gli utenti non riescono ad accedere.";
    private int _priority = 1;
    private DateTimeOffset _createdAt = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);
    private string _affectedService = "Customer Portal";

    public TicketBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public TicketBuilder WithPriority(int priority)
    {
        _priority = priority;
        return this;
    }

    public TicketBuilder CreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public IncidentTicket Build()
    {
        return new IncidentTicket(
            _code,
            _title,
            _description,
            _priority,
            _createdAt,
            _affectedService);
    }
}