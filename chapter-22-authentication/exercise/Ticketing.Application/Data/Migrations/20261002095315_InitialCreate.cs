using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Application.Data.Migrations;

/// <inheritdoc />
public partial class _20261002095315_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "dbo");

        migrationBuilder.CreateTable(
            name: "Tickets",
            schema: "dbo",
            columns: table => new
            {
                Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Priority = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                TicketType = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                AffectedService = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tickets", x => x.Code);
                table.CheckConstraint("CK_Tickets_Priority", "[Priority] BETWEEN 1 AND 3");
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Tickets",
            schema: "dbo");
    }
}
