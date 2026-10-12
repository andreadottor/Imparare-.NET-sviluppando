using Ticketing.Application.Tickets;
using Xunit;

namespace Ticketing.Application.Tests.Tickets;

public sealed class TicketContractTests
{
    [Theory]
    [InlineData("count")]
    [InlineData("INC-001")]
    [InlineData("INC.001")]
    [InlineData("a.Z-09")]
    public void ValidCode_IsPreserved(string code)
    {
        Assert.Equal(code, Create(code).Code);
    }

    [Theory]
    [InlineData("INC/001")]
    [InlineData("INC#001")]
    [InlineData("INC 001")]
    [InlineData("INC_001")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("é001")]
    public void InvalidCode_IsRejected(string code)
    {
        Assert.Throws<ArgumentException>(() => Create(code));
    }

    [Fact]
    public void PersistenceLimits_AreProtectedByDomain()
    {
        Create(new string('A', 30), new string('T', 200), new string('S', 100));
        Assert.Throws<ArgumentException>(() => Create(new string('A', 31)));
        Assert.Throws<ArgumentException>(() => Create(title: new string('T', 201)));
        Assert.Throws<ArgumentException>(() => Create(service: new string('S', 101)));
    }

    [Fact]
    public void Description_AllowsEmptyButRejectsNull()
    {
        Assert.Equal("", Create(description: "").Description);
        Assert.Throws<ArgumentNullException>(() => Create(description: null!));
    }

    private static IncidentTicket Create(string code = "INC-001",
        string title = "Title", string service = "Portal", string description = "Text")
        => new(code, title, description, 1, DateTimeOffset.UtcNow, service);
}
