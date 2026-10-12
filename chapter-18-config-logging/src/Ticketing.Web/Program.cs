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
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<UnexpectedExceptionHandler>();

WebApplication app = builder.Build();

app.Logger.LogInformation(
    "Starting Ticketing API in {Environment}",
    app.Environment.EnvironmentName);

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapTicketEndpoints();

app.Run();