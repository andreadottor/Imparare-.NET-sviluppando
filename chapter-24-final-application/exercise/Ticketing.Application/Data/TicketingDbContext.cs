using Ticketing.Application.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Ticketing.Application.Data;

public sealed class TicketingDbContext : DbContext
{
    public TicketingDbContext(DbContextOptions<TicketingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets", "dbo");
            entity.HasKey(ticket => ticket.Code);
            entity.Property(ticket => ticket.Code).HasMaxLength(30);
            entity.Property(ticket => ticket.Title).HasMaxLength(200).IsRequired();
            entity.Property(ticket => ticket.Description).IsRequired();
            entity.Property(ticket => ticket.Priority).IsRequired();
            entity.Property(ticket => ticket.CreatedAt).IsRequired();
            entity.Property(ticket => ticket.Status).HasDefaultValue(TicketStatus.Open).IsRequired();
            entity.Property(ticket => ticket.ClosedAt);
            entity.HasDiscriminator<string>("TicketType")
                .HasValue<IncidentTicket>("Incident");
            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_Tickets_Priority",
                    "[Priority] BETWEEN 1 AND 3"));
            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_Tickets_Status_ClosedAt",
                    "([Status] = 0 AND [ClosedAt] IS NULL) OR ([Status] = 1 AND [ClosedAt] IS NOT NULL)"));
        });

        modelBuilder.Entity<IncidentTicket>()
            .Property(ticket => ticket.AffectedService).HasMaxLength(100).IsRequired();
    }
}
