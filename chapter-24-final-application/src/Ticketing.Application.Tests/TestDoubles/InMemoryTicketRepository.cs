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

        if (!_tickets.TryAdd(ticket.Code, ticket))
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

        return Task.FromResult(ticket);
    }

    public Task<IReadOnlyCollection<Ticket>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        IReadOnlyCollection<Ticket> tickets = _tickets.Values.ToArray();

        return Task.FromResult(tickets);
    }

    public Task<bool> UpdateAsync(Ticket ticket, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_tickets.ContainsKey(ticket.Code))
        {
            return Task.FromResult(false);
        }

        _tickets[ticket.Code] = ticket;
        return Task.FromResult(true);
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
}