using Ticketing.Application.Tickets;

namespace Ticketing.Application.Data;

public interface ITicketRepository
{
    Task InsertAsync(Ticket ticket, CancellationToken ct);

    Task<Ticket?> FindAsync(string code, CancellationToken ct);

    Task<IReadOnlyCollection<Ticket>> GetAllAsync(CancellationToken ct);

    Task<bool> UpdateAsync(Ticket ticket, CancellationToken ct);

    Task<bool> CloseAsync(string code, DateTimeOffset closedAt, CancellationToken ct);

    Task<bool> DeleteAsync(string code, CancellationToken ct);

    Task<int> CountAsync(CancellationToken ct);
}