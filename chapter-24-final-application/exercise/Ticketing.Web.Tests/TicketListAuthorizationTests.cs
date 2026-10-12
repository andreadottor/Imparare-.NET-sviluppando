using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Application.Services;
using Ticketing.Application.Tickets;
using Ticketing.Web.Components.Pages;
using Xunit;

namespace Ticketing.Web.Tests;

public sealed class TicketListAuthorizationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CloseCallback_ChecksPolicyBeforeCallingUseCase(bool authenticated)
    {
        using var factory = new TicketingWebApplicationFactory();
        using IServiceScope scope = factory.Services.CreateScope();
        TicketService service = scope.ServiceProvider.GetRequiredService<TicketService>();
        await service.CreateIncidentAsync("INC-UI", "Title", "Description", 1, "Portal");
        using var component = new TicketList();
        var user = new ClaimsPrincipal(authenticated
            ? new ClaimsIdentity([new Claim(ClaimTypes.Name, "manager")], "Test")
            : new ClaimsIdentity());

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(TicketList).GetProperty("TicketService", flags)!.SetValue(component, service);
        typeof(TicketList).GetProperty("AuthorizationService", flags)!.SetValue(component,
            scope.ServiceProvider.GetRequiredService<IAuthorizationService>());
        typeof(TicketList).GetProperty("AuthenticationStateTask", flags)!.SetValue(component,
            Task.FromResult(new AuthenticationState(user)));

        // Exercise the callback itself, independently of whether AuthorizeView renders a button.
        var callback = (Task)typeof(TicketList).GetMethod("CloseTicketAsync", flags)!
            .Invoke(component, ["INC-UI"])!;
        await callback;

        Ticket? ticket = await service.FindAsync("INC-UI");
        Assert.NotNull(ticket);
        Assert.Equal(authenticated ? TicketStatus.Closed : TicketStatus.Open, ticket.Status);
        if (!authenticated)
        {
            Assert.Null(ticket.ClosedAt);
            Assert.Equal("You are not authorized to close tickets.",
                typeof(TicketList).GetField("errorMessage", flags)!.GetValue(component));
        }
    }
}
