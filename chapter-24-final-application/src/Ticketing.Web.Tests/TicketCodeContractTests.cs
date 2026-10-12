using System.Net;
using System.Net.Http.Json;
using Ticketing.Web.Endpoints;
using Xunit;

namespace Ticketing.Web.Tests;

public sealed class TicketCodeContractTests : IClassFixture<TicketingWebApplicationFactory>
{
    private readonly TicketingWebApplicationFactory _factory;
    public TicketCodeContractTests(TicketingWebApplicationFactory factory) => _factory = factory;

    [Theory]
    [InlineData("count")]
    [InlineData("INC-001")]
    [InlineData("INC.001")]
    public async Task CreatedCode_CanBeRetrievedFromLocation(string code)
    {
        using var factory = _factory.WithAuthenticatedUser();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/tickets",
            new CreateIncidentTicketRequest(code, "Title", "Description", 1, "Portal"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var get = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var ticket = await get.Content.ReadFromJsonAsync<TicketResponse>();
        Assert.Equal(code, ticket!.Code);
    }

    [Theory]
    [InlineData("INC/001")]
    [InlineData("INC#001")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("INC_001")]
    public async Task InvalidCode_ReturnsBadRequest(string code)
    {
        using var factory = _factory.WithAuthenticatedUser();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/tickets",
            new CreateIncidentTicketRequest(code, "Title", "Description", 1, "Portal"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
