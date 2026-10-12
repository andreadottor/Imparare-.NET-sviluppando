using Ticketing.Application.Services;
using Ticketing.Application.Tests.TestDoubles;
using Ticketing.Application.Tickets;
using Xunit;

namespace Ticketing.Application.Tests.Services;

public sealed class TicketServiceTests
{
    [Fact]
    public async Task CloseAsync_UsesTimeProviderAndPreservesFirstDate()
    {
        var clock = new TestTimeProvider();
        var service = new TicketService(new InMemoryTicketRepository(), clock);
        IncidentTicket ticket = await service.CreateIncidentAsync("INC-CLOSE", "Title", "Description", 1, "Portal");
        Assert.Equal(TicketStatus.Open, ticket.Status);
        DateTimeOffset createdAt = ticket.CreatedAt;
        clock.UtcNow = clock.UtcNow.AddHours(1);
        DateTimeOffset firstClosure = clock.UtcNow;

        Assert.True(await service.CloseAsync(ticket.Code));
        clock.UtcNow = clock.UtcNow.AddHours(1);
        Assert.True(await service.CloseAsync(ticket.Code));

        Ticket? saved = await service.FindAsync(ticket.Code);
        Assert.NotNull(saved);
        Assert.Equal(TicketStatus.Closed, saved.Status);
        Assert.Equal(firstClosure, saved.ClosedAt);
        Assert.Equal(createdAt, saved.CreatedAt);
    }

    [Fact]
    public async Task MissingTicket_UpdateAndClose_ReturnFalse()
    {
        var service = new TicketService(new InMemoryTicketRepository(), new TestTimeProvider());

        Assert.False(await service.CloseAsync("missing"));
        Assert.False(await service.UpdateIncidentAsync("missing", "Title", "Description", 1, "Portal"));
    }

    [Fact]
    public async Task UpdateIncidentAsync_PreservesCreationAndClosure()
    {
        var service = new TicketService(new InMemoryTicketRepository(), new TestTimeProvider());
        IncidentTicket original = await service.CreateIncidentAsync("INC-UPDATE", "Title", "Description", 1, "Portal");
        await service.CloseAsync(original.Code);
        DateTimeOffset? closedAt = (await service.FindAsync(original.Code))!.ClosedAt;

        Assert.True(await service.UpdateIncidentAsync(original.Code, "New title", "New description", 2, "API"));
        var updated = Assert.IsType<IncidentTicket>(await service.FindAsync(original.Code));

        Assert.Equal("New title", updated.Title);
        Assert.Equal("New description", updated.Description);
        Assert.Equal(2, updated.Priority);
        Assert.Equal("API", updated.AffectedService);
        Assert.Equal(original.CreatedAt, updated.CreatedAt);
        Assert.Equal(closedAt, updated.ClosedAt);
        Assert.Equal(TicketStatus.Closed, updated.Status);
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => UtcNow;
    }

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

        Assert.NotNull(saved);
        Assert.Equal(ticket.Code, saved.Code);
        Assert.Equal(ticket.Title, saved.Title);
        Assert.Equal(ticket.CreatedAt, saved.CreatedAt);
    }
}