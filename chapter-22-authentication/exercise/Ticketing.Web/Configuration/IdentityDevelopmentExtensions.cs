using Microsoft.AspNetCore.Identity;
using Ticketing.Web.Data;

namespace Ticketing.Web.Configuration;

public static class IdentityDevelopmentExtensions
{
    public static async Task InitializeDevelopmentIdentityAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        await using AsyncServiceScope scope      = app.Services.CreateAsyncScope();
        RoleManager<IdentityRole> roleManager    = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string roleName = "TicketManager";

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            IdentityResult result = await roleManager.CreateAsync(new IdentityRole(roleName));
            EnsureSucceeded(result, "create the TicketManager role");
        }

        string? email = app.Configuration["IdentityDevelopment:TicketManagerEmail"];

        if (string.IsNullOrWhiteSpace(email))
        {
            app.Logger.LogInformation("Development role assignment skipped: IdentityDevelopment:TicketManagerEmail is not configured.");
            return;
        }

        ApplicationUser? user = await userManager.FindByEmailAsync(email.Trim());

        if (user is null)
        {
            app.Logger.LogWarning("Development role assignment skipped: the configured TicketManager user does not exist. Register the user and restart the application.");
            return;
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            IdentityResult result = await userManager.AddToRoleAsync(user, roleName);
            EnsureSucceeded(result, "assign the TicketManager role");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            string errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
            throw new InvalidOperationException($"Unable to {operation}: {errors}");
        }
    }
}
