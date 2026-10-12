using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Application.Data.Migrations;

/// <inheritdoc />
public partial class _20261005095412_AddTicketsCreatedAtIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Tickets_CreatedAt",
            schema: "dbo",
            table: "Tickets",
            column: "CreatedAt",
            descending: new bool[0]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Tickets_CreatedAt",
            schema: "dbo",
            table: "Tickets");
    }
}
