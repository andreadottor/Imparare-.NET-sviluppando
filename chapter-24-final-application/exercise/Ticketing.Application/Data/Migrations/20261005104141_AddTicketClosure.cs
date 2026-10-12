using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Application.Data.Migrations;

/// <inheritdoc />
public partial class _20261005104141_AddTicketClosure : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "ClosedAt",
            schema: "dbo",
            table: "Tickets",
            type: "datetimeoffset",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Status",
            schema: "dbo",
            table: "Tickets",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddCheckConstraint(
            name: "CK_Tickets_Status_ClosedAt",
            schema: "dbo",
            table: "Tickets",
            sql: "([Status] = 0 AND [ClosedAt] IS NULL) OR ([Status] = 1 AND [ClosedAt] IS NOT NULL)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_Tickets_Status_ClosedAt",
            schema: "dbo",
            table: "Tickets");

        migrationBuilder.DropColumn(
            name: "ClosedAt",
            schema: "dbo",
            table: "Tickets");

        migrationBuilder.DropColumn(
            name: "Status",
            schema: "dbo",
            table: "Tickets");
    }
}
