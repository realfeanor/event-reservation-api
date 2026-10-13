using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReservationLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HeldByReservationId",
                table: "SessionSeats",
                newName: "ReservationId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConcurrencyToken",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAtUtc",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAtUtc",
                table: "Reservations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ConfirmedAtUtc",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ExpiredAtUtc",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ReservationId",
                table: "SessionSeats",
                newName: "HeldByReservationId");
        }
    }
}
