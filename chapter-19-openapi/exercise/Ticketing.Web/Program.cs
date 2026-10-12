using Scalar.AspNetCore;
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
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.Logger.LogInformation(
    "Starting Ticketing API in {Environment}",
    app.Environment.EnvironmentName);

app.UseExceptionHandler();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapTicketEndpoints();

app.Run();