using Ticketing.Application.Data;
using Ticketing.Application.Tickets;

namespace Ticketing.Application.Services;

public sealed class TicketService
{
    private readonly ITicketRepository _repository;

    public TicketService(ITicketRepository repository)
    {
        _repository = repository;
    }

    public Task RegisterAsync(Ticket ticket, CancellationToken ct = default)
    {
        return _repository.InsertAsync(ticket, ct);
    }

    public Task<Ticket?> FindAsync(string code, CancellationToken ct = default)
    {
        return _repository.FindAsync(code, ct);
    }

    public Task<IReadOnlyCollection<Ticket>> GetAllAsync(CancellationToken ct = default)
    {
        return _repository.GetAllAsync(ct);
    }

    public Task<bool> UpdateAsync(Ticket ticket, CancellationToken ct = default)
    {
        return _repository.UpdateAsync(ticket, ct);
    }

    public Task<bool> DeleteAsync(string code, CancellationToken ct = default)
    {
        return _repository.DeleteAsync(code, ct);
    }

    public Task<int> CountAsync(CancellationToken ct = default)
    {
        return _repository.CountAsync(ct);
    }
}