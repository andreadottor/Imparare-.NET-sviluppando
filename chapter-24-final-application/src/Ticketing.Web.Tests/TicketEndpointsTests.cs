using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Ticketing.Web.Endpoints;
using Xunit;

namespace Ticketing.Web.Tests.Endpoints;

public sealed class TicketEndpointsTests
    : IClassFixture<TicketingWebApplicationFactory>, IDisposable
{
    private readonly TicketingWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TicketEndpointsTests(TicketingWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task GetTickets_AnonymousUser_ReturnsOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/tickets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        TicketResponse[]? tickets =
            await response.Content.ReadFromJsonAsync<TicketResponse[]>();

        Assert.NotNull(tickets);
        Assert.Empty(tickets);
    }

    [Fact]
    public async Task PostTicket_AnonymousUser_ReturnsUnauthorized()
    {
        var request = new
        {
            code = "INC-001",
            title = "Portale non disponibile",
            description = "Gli utenti non riescono ad accedere.",
            priority = 1,
            affectedService = "Customer Portal"
        };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync("/api/tickets", request);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task PostTicket_AuthenticatedUser_ReturnsCreatedTicket()
    {
        using WebApplicationFactory<Program> factory =
            _factory.WithAuthenticatedUser();

        using HttpClient client = factory.CreateClient();

        var request = new
        {
            code = "INC-001",
            title = "Portale non disponibile",
            description = "Gli utenti non riescono ad accedere.",
            priority = 1,
            affectedService = "Customer Portal"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync("/api/tickets", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(
            "http://localhost/api/tickets/INC-001",
            response.Headers.Location?.ToString());

        TicketResponse? ticket =
            await response.Content.ReadFromJsonAsync<TicketResponse>();

        Assert.NotNull(ticket);
        Assert.Equal("INC-001", ticket.Code);
        Assert.Equal(TicketingWebApplicationFactory.FixedUtcNow, ticket.CreatedAt);
    }

    [Fact]
    public async Task PostTicket_InvalidRequest_ReturnsBadRequest()
    {
        using WebApplicationFactory<Program> factory =
            _factory.WithAuthenticatedUser();

        using HttpClient client = factory.CreateClient();

        var request = new
        {
            code = "",
            title = "",
            description = "",
            priority = 8,
            affectedService = ""
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync("/api/tickets", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}