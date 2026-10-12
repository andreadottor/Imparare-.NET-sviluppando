using Microsoft.Data.SqlClient;
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

        services.AddSingleton(_ => new TicketRepository(connectionString));
        services.AddTransient<TicketService>();

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

            await service.RegisterAsync(ticket);

            Console.WriteLine($"Ticket {ticket.Code} registrato.");

            Ticket? foundTicket = await service.FindAsync("INC-001");

            if (foundTicket is not null)
            {
                Console.WriteLine(foundTicket.GetSummary());
            }

            const string mixedCaseCode = "Inc-001";
            Ticket? mixedCaseTicket = await service.FindAsync(mixedCaseCode);

            Console.WriteLine(
                $"Ricerca di {mixedCaseCode} per il ticket {ticket.Code}: " +
                (mixedCaseTicket is not null ? "trovato" : "non trovato"));

            Ticket updatedTicket = new IncidentTicket(
                ticket.Code,
                "Portale disponibile a intermittenza",
                "Alcuni utenti ricevono ancora un errore.",
                priority: 2,
                ticket.CreatedAt,
                "Customer Portal");

            bool updated = await service.UpdateAsync(updatedTicket);

            Console.WriteLine($"Ticket aggiornato: {updated}");

            IReadOnlyCollection<Ticket> tickets = await service.GetAllAsync();

            Console.WriteLine($"Ticket letti: {tickets.Count}");

            int count = await service.CountAsync();

            Console.WriteLine($"Ticket presenti: {count}");

            for (int priority = 1; priority <= 3; priority++)
            {
                int priorityCount = await service.CountByPriorityAsync(priority);

                Console.WriteLine($"Ticket con priorità {priority}: {priorityCount}");
            }

            bool deleted = await service.DeleteAsync(ticket.Code);

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