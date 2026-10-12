using Ticketing.Application.Services;
using Ticketing.Application.Tests.TestDoubles;
using Ticketing.Application.Tickets;
using Xunit;

namespace Ticketing.Application.Tests.Services;

public sealed class TicketServiceTests
{
    [Fact]
    public async Task RegisterAsync_NewTicket_MakesTicketAvailable()
    {
        var repository = new InMemoryTicketRepository();
        var service = new TicketService(repository, TimeProvider.System);
        var ticket = new IncidentTicket(
            "INC-001",
            "Portale non disponibile",
            "Gli utenti non riescono ad accedere.",
            1,
            new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero),
            "Customer Portal");

        await service.RegisterAsync(ticket, CancellationToken.None);

        Ticket? saved = await service.FindAsync(
            "INC-001",
            CancellationToken.None);

        Assert.Same(ticket, saved);
    }
}