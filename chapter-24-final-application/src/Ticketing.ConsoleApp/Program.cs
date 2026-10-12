using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Application.Data;
using Ticketing.Application.Services;
using Ticketing.Application.Tickets;

namespace Ticketing.ConsoleApp;

internal static class Program
{
    private static async Task Main()
    {
        string connectionString =
            Environment.GetEnvironmentVariable("TICKETING_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "La variabile TICKETING_CONNECTION_STRING non è configurata.");

        ServiceCollection services = new();

        services.AddDbContextFactory<TicketingDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddTransient<ITicketRepository, EfCoreTicketRepository>();
        services.AddTransient<TicketService>();
        services.AddSingleton(TimeProvider.System);

        using ServiceProvider provider = services.BuildServiceProvider();

        TicketService service = provider.GetRequiredService<TicketService>();

        try
        {
            Ticket ticket = new IncidentTicket(
                "INC-001",
                "Portale non disponibile",
                "Gli utenti non riescono ad accedere.",
                priority: 1,
                new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero),
                "Customer Portal");

            await service.RegisterAsync(ticket, CancellationToken.None);

            Console.WriteLine($"Ticket {ticket.Code} registrato.");

            Ticket? foundTicket = await service.FindAsync(
                "INC-001",
                CancellationToken.None);

            if (foundTicket is not null)
            {
                Console.WriteLine(foundTicket.GetSummary());
            }

            Ticket updatedTicket = new IncidentTicket(
                ticket.Code,
                "Portale disponibile a intermittenza",
                "Alcuni utenti ricevono ancora un errore.",
                priority: 2,
                ticket.CreatedAt,
                "Customer Portal");

            bool updated = await service.UpdateAsync(
                updatedTicket,
                CancellationToken.None);

            Console.WriteLine($"Ticket aggiornato: {updated}");

            IReadOnlyCollection<Ticket> tickets =
                await service.GetAllAsync(CancellationToken.None);

            Console.WriteLine($"Ticket presenti: {tickets.Count}");

            bool deleted = await service.DeleteAsync(
                ticket.Code,
                CancellationToken.None);

            Console.WriteLine($"Ticket cancellato: {deleted}");
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine($"Dati non validi: {exception.Message}");
        }
        catch (DuplicateTicketException exception)
        {
            Console.WriteLine($"Registrazione non riuscita: {exception.Message}");
        }
        catch (SqlException exception)
        {
            Console.WriteLine($"Errore SQL Server: {exception.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operazione annullata.");
        }
        finally
        {
            Console.WriteLine("Operazione terminata.");
        }
    }
}