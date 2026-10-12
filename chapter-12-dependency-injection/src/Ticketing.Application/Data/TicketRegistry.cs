using Ticketing.Application.Tickets;

namespace Ticketing.Application.Data;

public sealed class TicketRegistry<TTicket>
    where TTicket : Ticket
{
    private readonly Dictionary<string, TTicket> _tickets = new(
        StringComparer.OrdinalIgnoreCase);

    public bool Add(TTicket ticket) => _tickets.TryAdd(ticket.Code, ticket);

    public bool Remove(string code) => _tickets.Remove(code);

    public bool TryGet(string code, out TTicket? ticket) =>
        _tickets.TryGetValue(code, out ticket);

    public IReadOnlyCollection<TTicket> GetAll() => _tickets.Values.ToArray();
}