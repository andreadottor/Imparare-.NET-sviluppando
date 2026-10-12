using Ticketing.Application.Tickets;

namespace Ticketing.Application.Data;

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

    public IReadOnlyCollection<TTicket> GetAll()
    {
        return _tickets.Values.ToArray();
    }

    public bool Remove(string code)
    {
        return _tickets.Remove(code);
    }
}