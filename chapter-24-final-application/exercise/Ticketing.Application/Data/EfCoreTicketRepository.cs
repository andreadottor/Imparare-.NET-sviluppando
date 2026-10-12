using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Tickets;

namespace Ticketing.Application.Data;

public sealed class EfCoreTicketRepository : ITicketRepository
{
    private readonly IDbContextFactory<TicketingDbContext> _contextFactory;

    public EfCoreTicketRepository(IDbContextFactory<TicketingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task InsertAsync(Ticket ticket, CancellationToken ct)
    {
        await using TicketingDbContext context = await _contextFactory.CreateDbContextAsync(ct);

        context.Tickets.Add(ticket);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException
            {
                Number: 2601 or 2627
            })
        {
            throw new DuplicateTicketException(
                $"Il ticket {ticket.Code} è già presente.",
                exception);
        }
    }

    public async Task<Ticket?> FindAsync(string code, CancellationToken ct)
    {
        await using TicketingDbContext context =
            await _contextFactory.CreateDbContextAsync(ct);

        return await context.Tickets
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Code == code, ct);
    }

    public async Task<IReadOnlyCollection<Ticket>> GetAllAsync(CancellationToken ct)
    {
        await using TicketingDbContext context =
            await _contextFactory.CreateDbContextAsync(ct);

        return await context.Tickets
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateAsync(Ticket ticket, CancellationToken ct)
    {
        await using TicketingDbContext context =
            await _contextFactory.CreateDbContextAsync(ct);

        if (ticket is not IncidentTicket incident)
        {
            throw new NotSupportedException($"Il tipo {ticket.GetType().Name} non è supportato.");
        }

        int affectedRows = await context.Tickets.OfType<IncidentTicket>()
            .Where(item => item.Code == ticket.Code)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Title, incident.Title)
                .SetProperty(item => item.Description, incident.Description)
                .SetProperty(item => item.Priority, incident.Priority)
                .SetProperty(item => item.AffectedService, incident.AffectedService), ct);

        return affectedRows > 0;
    }

    public async Task<bool> CloseAsync(string code, DateTimeOffset closedAt, CancellationToken ct)
    {
        await using TicketingDbContext context = await _contextFactory.CreateDbContextAsync(ct);

        int affectedRows = await context.Tickets
            .Where(item => item.Code == code && item.Status == TicketStatus.Open)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Status, TicketStatus.Closed)
                .SetProperty(item => item.ClosedAt, (DateTimeOffset?)closedAt), ct);

        return affectedRows > 0 || await context.Tickets.AnyAsync(item => item.Code == code, ct);
    }

    public async Task<bool> DeleteAsync(string code, CancellationToken ct)
    {
        await using TicketingDbContext context =
            await _contextFactory.CreateDbContextAsync(ct);

        Ticket? ticket = await context.Tickets.SingleOrDefaultAsync(
            item => item.Code == code,
            ct);

        if (ticket is null)
        {
            return false;
        }

        context.Tickets.Remove(ticket);
        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        await using TicketingDbContext context = await _contextFactory.CreateDbContextAsync(ct);

        return await context.Tickets.CountAsync(ct);
    }
}