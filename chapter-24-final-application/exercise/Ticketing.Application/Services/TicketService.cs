using Ticketing.Application.Data;
using Ticketing.Application.Tickets;

namespace Ticketing.Application.Services;

public sealed class TicketService
{
    private readonly ITicketRepository _repository;
    private readonly TimeProvider _timeProvider;

    public TicketService(ITicketRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<IncidentTicket> CreateIncidentAsync(
        string code,
        string title,
        string description,
        int priority,
        string affectedService,
        CancellationToken ct = default)
    {
        var ticket = new IncidentTicket(
            code,
            title,
            description,
            priority,
            _timeProvider.GetUtcNow(),
            affectedService);

        await _repository.InsertAsync(ticket, ct);

        return ticket;
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

    public async Task<bool> UpdateIncidentAsync(
        string code,
        string title,
        string description,
        int priority,
        string affectedService,
        CancellationToken ct = default)
    {
        if (await _repository.FindAsync(code, ct) is not IncidentTicket original)
        {
            return false;
        }

        var ticket = new IncidentTicket(
            original.Code,
            title,
            description,
            priority,
            original.CreatedAt,
            affectedService,
            original.ClosedAt);

        return await _repository.UpdateAsync(ticket, ct);
    }

    public async Task<bool> CloseAsync(string code, CancellationToken ct = default)
    {
        Ticket? ticket = await _repository.FindAsync(code, ct);
        if (ticket is null)
        {
            return false;
        }

        ticket.Close(_timeProvider.GetUtcNow());
        return await _repository.CloseAsync(ticket.Code, ticket.ClosedAt!.Value, ct);
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