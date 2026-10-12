using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Data;
using Ticketing.Application.Services;
using Ticketing.Web.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString =
    Environment.GetEnvironmentVariable("TICKETING_CONNECTION_STRING")
    ?? throw new InvalidOperationException(
        "La variabile TICKETING_CONNECTION_STRING non è configurata.");

builder.Services.AddDbContextFactory<TicketingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ITicketRepository, EfCoreTicketRepository>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddValidation();

WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.MapTicketEndpoints();

app.Run();