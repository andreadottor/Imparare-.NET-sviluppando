using Ticketing.Application.Data;
using Ticketing.Application.Services;
using Ticketing.Application.Tickets;

namespace Ticketing.ConsoleApp;

internal static class Program
{
    private static async Task Main()
    {
        var registry = new TicketRegistry<Ticket>();
        var gateway = new TicketGateway();
        var service = new TicketService(registry, gateway);

        try
        {
            Ticket ticket = new IncidentTicket(
                "INC-001",
                "Portale non disponibile",
                "Gli utenti non riescono ad accedere.",
                priority: 1,
                new DateTimeOffset(
                    2026,
                    9,
                    14,
                    9,
                    0,
                    0,
                    TimeSpan.Zero),
                "Customer Portal");

            await service.RegisterAsync(ticket, CancellationToken.None);

            Console.WriteLine($"Ticket {ticket.Code} registrato.");

            Ticket? foundTicket = await service.FindAsync(
                "inc-001",
                CancellationToken.None);

            if (foundTicket is not null)
            {
                Console.WriteLine(foundTicket.GetSummary());
            }
            else
            {
                Console.WriteLine("Ticket non trovato.");
            }

            Console.WriteLine("Ticket registrati:");

            foreach (Ticket registered in registry.GetAll())
            {
                Console.WriteLine($"- {registered.Code}: {registered.Title}");
            }
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine($"Dati non validi: {exception.Message}");
        }
        catch (DuplicateTicketException exception)
        {
            Console.WriteLine($"Registrazione non riuscita: {exception.Message}");
        }
        catch (IOException exception)
        {
            Console.WriteLine($"Salvataggio non riuscito: {exception.Message}");
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