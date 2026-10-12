using Ticketing.Application.Tickets;
using Xunit;

namespace Ticketing.Application.Tests.Tickets;

public sealed class IncidentTicketTests
{
    [Fact]
    public void Constructor_ValidValues_CreatesTicket()
    {
        DateTimeOffset createdAt = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

        var ticket = new IncidentTicket(
            "  INC-001  ",
            "  Portale non disponibile  ",
            "Gli utenti non riescono ad accedere.",
            1,
            createdAt,
            "  Customer Portal  ");

        Assert.Equal("INC-001", ticket.Code);
        Assert.Equal("Portale non disponibile", ticket.Title);
        Assert.Equal(1, ticket.Priority);
        Assert.Equal(createdAt, ticket.CreatedAt);
        Assert.Equal("Customer Portal", ticket.AffectedService);
        Assert.Equal("Incident", ticket.TypeName);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Null(ticket.ClosedAt);
    }

    [Fact]
    public void Close_RepeatedCall_PreservesFirstClosureDate()
    {
        DateTimeOffset createdAt = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);
        var ticket = new IncidentTicket("INC-001", "Title", "Description", 1, createdAt, "Portal");
        DateTimeOffset closedAt = createdAt.AddHours(1);

        ticket.Close(closedAt);
        ticket.Close(closedAt.AddHours(1));

        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.Equal(closedAt, ticket.ClosedAt);
        Assert.Equal(createdAt, ticket.CreatedAt);
    }

    [Fact]
    public void Constructor_ClosedTicket_RestoresLifecycle()
    {
        DateTimeOffset createdAt = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);
        var ticket = new IncidentTicket("INC-001", "Title", "Description", 1,
            createdAt, "Portal", createdAt.AddHours(1));

        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.Equal(createdAt.AddHours(1), ticket.ClosedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Constructor_InvalidPriority_ThrowsException(int priority)
    {
        Action action = () => new IncidentTicket(
        "INC-001",
        "Portale non disponibile",
        "Gli utenti non riescono ad accedere.",
        priority,
        DateTimeOffset.UtcNow,
        "Customer Portal");

        ArgumentOutOfRangeException exception =
        Assert.Throws<ArgumentOutOfRangeException>(action);

        Assert.Equal("priority", exception.ParamName);
    }
}