using Scalar.AspNetCore;
using Ticketing.Application.Services;
using Ticketing.Web.Components;
using Ticketing.Web.Configuration;
using Ticketing.Web.Endpoints;
using Ticketing.Web.ErrorHandling;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTicketingPersistence(builder.Configuration);
builder.Services.AddScoped<TicketService>();
builder.Services.AddValidation();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<UnexpectedExceptionHandler>();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.Logger.LogInformation(
    "Starting Ticketing Web App in {Environment}",
    app.Environment.EnvironmentName);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    pages => pages.UseStatusCodePagesWithReExecute(
        "/not-found",
        createScopeForStatusCodePages: true));

app.UseHttpsRedirection();

app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapTicketEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();