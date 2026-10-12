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

        context.Tickets.Update(ticket);

        try
        {
            await context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
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
        await using TicketingDbContext context =
            await _contextFactory.CreateDbContextAsync(ct);

        return await context.Tickets.CountAsync(ct);
    }
}