using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Application.Tickets;
using Ticketing.Web.Endpoints;
using Xunit;

namespace Ticketing.Web.Tests.Endpoints;

public sealed class TicketLifecycleEndpointsTests : IDisposable
{
    private readonly TicketingWebApplicationFactory _baseFactory = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TicketLifecycleEndpointsTests()
    {
        _factory = _baseFactory.WithAuthenticatedUser();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    private static UpdateIncidentTicketRequest ValidUpdate => new("Updated title", "Updated description", 2, "API");

    private async Task<TicketResponse> CreateAsync(string code = "INC-LIFECYCLE")
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync("/api/tickets",
            new CreateIncidentTicketRequest(code, "Title", "Description", 1, "Portal"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TicketResponse>())!;
    }

    [Theory]
    [InlineData("PUT", "/api/tickets/INC-LIFECYCLE")]
    [InlineData("POST", "/api/tickets/INC-LIFECYCLE/close")]
    public async Task Mutation_AnonymousUser_ReturnsUnauthorizedAndDoesNotChangeTicket(string method, string path)
    {
        await CreateAsync();
        _client.DefaultRequestHeaders.Add("X-Test-Anonymous", "true");
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method == "PUT") request.Content = JsonContent.Create(ValidUpdate);

        using HttpResponseMessage response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        TicketResponse? ticket = await _client.GetFromJsonAsync<TicketResponse>("/api/tickets/INC-LIFECYCLE");
        Assert.NotNull(ticket);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Null(ticket.ClosedAt);
        Assert.Equal("Title", ticket.Title);
    }

    [Theory]
    [InlineData("PUT", "/api/tickets/missing")]
    [InlineData("POST", "/api/tickets/missing/close")]
    public async Task Mutation_MissingTicket_ReturnsNotFoundProblem(string method, string path)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method == "PUT") request.Content = JsonContent.Create(ValidUpdate);
        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Ticket not found", problem.Title);
        Assert.Contains("missing", problem.Detail);
    }

    [Fact]
    public async Task Close_RepeatedRequest_IsIdempotentAndVisibleToAnonymousReaders()
    {
        TicketResponse original = await CreateAsync();
        Assert.Equal(TicketStatus.Open, original.Status);
        Assert.Null(original.ClosedAt);

        using HttpResponseMessage first = await _client.PostAsync("/api/tickets/INC-LIFECYCLE/close", null);
        using HttpResponseMessage second = await _client.PostAsync("/api/tickets/INC-LIFECYCLE/close", null);
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        Assert.Equal("", await second.Content.ReadAsStringAsync());

        _client.DefaultRequestHeaders.Add("X-Test-Anonymous", "true");
        TicketResponse? closed = await _client.GetFromJsonAsync<TicketResponse>("/api/tickets/INC-LIFECYCLE");
        Assert.NotNull(closed);
        Assert.Equal(TicketStatus.Closed, closed.Status);
        Assert.Equal(TicketingWebApplicationFactory.FixedUtcNow, closed.ClosedAt);
        Assert.Equal(original.CreatedAt, closed.CreatedAt);
        TicketResponse[]? list = await _client.GetFromJsonAsync<TicketResponse[]>("/api/tickets");
        Assert.Equal(closed, Assert.Single(list!));
        string json = await _client.GetStringAsync("/api/tickets/INC-LIFECYCLE");
        Assert.Contains("\"status\":\"Closed\"", json);
    }

    [Fact]
    public async Task Put_ValidRequest_UpdatesEditableFieldsAndPreservesLifecycle()
    {
        TicketResponse original = await CreateAsync();
        await _client.PostAsync("/api/tickets/INC-LIFECYCLE/close", null);
        using HttpResponseMessage response = await _client.PutAsJsonAsync("/api/tickets/INC-LIFECYCLE", new
        {
            ValidUpdate.Title, ValidUpdate.Description, ValidUpdate.Priority, ValidUpdate.AffectedService,
            Code = "cannot-change", CreatedAt = DateTimeOffset.MinValue
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("", await response.Content.ReadAsStringAsync());
        TicketResponse? updated = await _client.GetFromJsonAsync<TicketResponse>("/api/tickets/INC-LIFECYCLE");
        Assert.NotNull(updated);
        Assert.Equal(original.Code, updated.Code);
        Assert.Equal(original.CreatedAt, updated.CreatedAt);
        Assert.Equal(TicketStatus.Closed, updated.Status);
        Assert.Equal(TicketingWebApplicationFactory.FixedUtcNow, updated.ClosedAt);
        Assert.Equal(ValidUpdate.Title, updated.Title);
        Assert.Equal(ValidUpdate.Description, updated.Description);
        Assert.Equal(ValidUpdate.Priority, updated.Priority);
        Assert.Equal(ValidUpdate.AffectedService, updated.AffectedService);
    }

    [Theory]
    [InlineData("", "Description", 1, "Portal")]
    [InlineData("Title", null, 1, "Portal")]
    [InlineData("Title", "Description", 4, "Portal")]
    [InlineData("Title", "Description", 1, " ")]
    [InlineData(null, "Description", 1, "Portal")]
    public async Task Put_InvalidRequest_ReturnsValidationProblemWithoutChangingTicket(
        string? title, string? description, int priority, string? affectedService)
    {
        TicketResponse original = await CreateAsync();
        using HttpResponseMessage response = await _client.PutAsJsonAsync("/api/tickets/INC-LIFECYCLE",
            new { title, description, priority, affectedService });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        TicketResponse? unchanged = await _client.GetFromJsonAsync<TicketResponse>("/api/tickets/INC-LIFECYCLE");
        Assert.Equal(original, unchanged);
    }

    [Theory]
    [InlineData("PUT", "/api/tickets/INC-LIFECYCLE")]
    [InlineData("POST", "/api/tickets/INC-LIFECYCLE/close")]
    public async Task Mutation_AuthenticatedButPolicyDenied_ReturnsForbidden(string method, string path)
    {
        using WebApplicationFactory<Program> deniedFactory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddAuthorization(options =>
                options.AddPolicy("ManageTickets", policy => policy.RequireClaim("manage-tickets")))));
        using HttpClient client = deniedFactory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method == "PUT") request.Content = JsonContent.Create(ValidUpdate);

        using HttpResponseMessage response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OpenApi_DescribesUpdateCloseAndLifecycle()
    {
        using JsonDocument document = JsonDocument.Parse(await _client.GetStringAsync("/openapi/v1.json"));
        JsonElement root = document.RootElement;
        JsonElement paths = root.GetProperty("paths");
        JsonElement put = paths.GetProperty("/api/tickets/{code}").GetProperty("put");
        JsonElement close = paths.GetProperty("/api/tickets/{code}/close").GetProperty("post");
        foreach (string status in new[] { "204", "400", "404", "401", "403" })
            Assert.True(put.GetProperty("responses").TryGetProperty(status, out _));
        foreach (string status in new[] { "204", "404", "401", "403" })
            Assert.True(close.GetProperty("responses").TryGetProperty(status, out _));
        JsonElement schemas = root.GetProperty("components").GetProperty("schemas");
        JsonElement updateProperties = schemas.GetProperty("UpdateIncidentTicketRequest").GetProperty("properties");
        Assert.False(updateProperties.TryGetProperty("code", out _));
        Assert.False(updateProperties.TryGetProperty("createdAt", out _));
        Assert.True(updateProperties.TryGetProperty("title", out _));
        JsonElement responseProperties = schemas.GetProperty("TicketResponse").GetProperty("properties");
        Assert.True(responseProperties.TryGetProperty("status", out _));
        Assert.True(responseProperties.TryGetProperty("closedAt", out _));
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        _baseFactory.Dispose();
    }
}
