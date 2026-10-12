using System.Collections.Concurrent;
using Ticketing.Application.Data;
using Ticketing.Application.Tickets;

namespace Ticketing.Application.Tests.TestDoubles;

public sealed class InMemoryTicketRepository : ITicketRepository
{
    private readonly ConcurrentDictionary<string, Ticket> _tickets = new(StringComparer.OrdinalIgnoreCase);

    public Task InsertAsync(Ticket ticket, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_tickets.TryAdd(ticket.Code, Copy(ticket)))
        {
            throw new DuplicateTicketException(
                $"Il ticket {ticket.Code} è già presente.");
        }

        return Task.CompletedTask;
    }

    public Task<Ticket?> FindAsync(string code, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _tickets.TryGetValue(code, out Ticket? ticket);

        return Task.FromResult(ticket is null ? null : Copy(ticket));
    }

    public Task<IReadOnlyCollection<Ticket>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        IReadOnlyCollection<Ticket> tickets = _tickets.Values.Select(Copy).ToArray();

        return Task.FromResult(tickets);
    }

    public Task<bool> UpdateAsync(Ticket ticket, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        while (_tickets.TryGetValue(ticket.Code, out Ticket? original))
        {
            var incident = (IncidentTicket)ticket;
            var updated = new IncidentTicket(original.Code, incident.Title, incident.Description,
                incident.Priority, original.CreatedAt, incident.AffectedService, original.ClosedAt);
            if (_tickets.TryUpdate(ticket.Code, updated, original))
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task<bool> CloseAsync(string code, DateTimeOffset closedAt, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        while (_tickets.TryGetValue(code, out Ticket? ticket))
        {
            Ticket closed = Copy(ticket);
            closed.Close(closedAt);
            if (_tickets.TryUpdate(code, closed, ticket))
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(string code, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        bool deleted = _tickets.TryRemove(code, out _);

        return Task.FromResult(deleted);
    }

    public Task<int> CountAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_tickets.Count);
    }

    private static Ticket Copy(Ticket ticket)
    {
        var incident = (IncidentTicket)ticket;
        return new IncidentTicket(incident.Code, incident.Title, incident.Description,
            incident.Priority, incident.CreatedAt, incident.AffectedService, incident.ClosedAt);
    }
}