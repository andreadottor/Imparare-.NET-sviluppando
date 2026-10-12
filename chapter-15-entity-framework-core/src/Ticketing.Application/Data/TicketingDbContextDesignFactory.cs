using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ticketing.Application.Data;

public sealed class TicketingDbContextDesignFactory
    : IDesignTimeDbContextFactory<TicketingDbContext>
{
    public TicketingDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable(
                "TICKETING_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "La variabile TICKETING_CONNECTION_STRING non è configurata.");

        DbContextOptions<TicketingDbContext> options =
            new DbContextOptionsBuilder<TicketingDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        return new TicketingDbContext(options);
    }
}
