using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ticketing.Application.Data;
using Ticketing.Application.Services;
using Ticketing.Application.Tickets;
using Ticketing.Web.Components.Pages;
using Xunit;

namespace Ticketing.Web.Tests;

public sealed class TicketEditTests
{
    [Fact]
    public async Task LoadFailure_ShowsErrorAndReturnLink()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITicketRepository, FailingRepository>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<TicketService>();
        services.AddSingleton<NavigationManager, TestNavigationManager>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider,
            provider.GetRequiredService<ILoggerFactory>());
        string html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(TicketEdit.Code)] = "INC-001"
            });
            var component = await renderer.RenderComponentAsync<TicketEdit>(parameters);
            return component.ToHtmlString();
        });
        Assert.Contains("Unable to load the ticket.", html);
        Assert.Contains("href=\"/tickets\"", html);
        Assert.DoesNotContain("<form", html);
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager() => Initialize("http://localhost/", "http://localhost/tickets/INC-001/edit");
    }

    private sealed class FailingRepository : ITicketRepository
    {
        public Task<Ticket?> FindAsync(string code, CancellationToken ct)
            => Task.FromException<Ticket?>(new InvalidOperationException("Database unavailable"));
        public Task InsertAsync(Ticket ticket, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<Ticket>> GetAllAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task<bool> UpdateAsync(Ticket ticket, CancellationToken ct) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(string code, CancellationToken ct) => throw new NotSupportedException();
        public Task<int> CountAsync(CancellationToken ct) => throw new NotSupportedException();
    }
}
