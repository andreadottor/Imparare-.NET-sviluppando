using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Data;

namespace Ticketing.Web.Configuration;

public static class TicketingPersistenceExtensions
{
    public static IServiceCollection AddTicketingPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Ticketing");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("La connection string 'Ticketing' non è configurata.");
        }

        string implementation = configuration["Persistence:Implementation"] ?? "EfCore";

        switch (implementation.ToUpperInvariant())
        {
            case "ADONET":
                services.AddScoped<ITicketRepository>(_ => new AdoNetTicketRepository(connectionString));
                break;

            case "DAPPER":
                services.AddScoped<ITicketRepository>(_ => new DapperTicketRepository(connectionString));
                break;

            case "EFCORE":
                services.AddDbContextFactory<TicketingDbContext>(options => options.UseSqlServer(connectionString));
                services.AddScoped<ITicketRepository, EfCoreTicketRepository>();
                break;

            default:
                throw new InvalidOperationException($"L'implementazione '{implementation}' non è supportata.");
        }

        return services;
    }
}