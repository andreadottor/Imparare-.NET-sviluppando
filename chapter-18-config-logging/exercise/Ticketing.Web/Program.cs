using Ticketing.Application.Services;
using Ticketing.Web.Configuration;
using Ticketing.Web.Endpoints;
using Ticketing.Web.ErrorHandling;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddTicketingPersistence(builder.Configuration);
builder.Services.AddScoped<TicketService>();
builder.Services.AddValidation();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<UnexpectedExceptionHandler>();

WebApplication app = builder.Build();

string persistenceImplementation = app.Configuration["Persistence:Implementation"] ?? "EfCore";

app.Logger.LogInformation(
    "Starting Ticketing API in {Environment} using {PersistenceImplementation}",
    app.Environment.EnvironmentName,
    persistenceImplementation);

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapTicketEndpoints();

app.Run();