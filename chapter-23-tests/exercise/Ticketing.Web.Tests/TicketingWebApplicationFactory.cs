using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ticketing.Application.Data;
using Ticketing.Application.Tests.TestDoubles;
using Ticketing.Web.Tests.TestDoubles;

namespace Ticketing.Web.Tests;

public sealed class TicketingWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly DateTimeOffset FixedUtcNow =
        new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            "ConnectionStrings:Ticketing",
            "Server=localhost;Database=TicketingTests;" +
            "Integrated Security=True;TrustServerCertificate=True");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITicketRepository>();
            services.AddSingleton<ITicketRepository, InMemoryTicketRepository>();

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new FixedTimeProvider(FixedUtcNow));
        });
    }

    public WebApplicationFactory<Program> WithAuthenticatedUser()
    {
        return WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme =
                        TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
            });
        });
    }
}