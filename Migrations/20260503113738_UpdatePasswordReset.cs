using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TripGenius.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordResets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedTrips_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "Id",
                keyValue: 1,
                column: "BookingDate",
                value: new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(455));

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "Id",
                keyValue: 2,
                column: "BookingDate",
                value: new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(459));

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 1,
                column: "PaymentDate",
                value: new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(551));

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 2,
                column: "PaymentDate",
                value: new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(554));

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(578));

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(580));

            migrationBuilder.InsertData(
                table: "SavedTrips",
                columns: new[] { "Id", "CreatedAt", "TripId", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 3, 11, 37, 37, 535, DateTimeKind.Utc).AddTicks(422), 1, 2 },
                    { 2, new DateTime(2026, 5, 3, 11, 37, 37, 535, DateTimeKind.Utc).AddTicks(424), 2, 2 }
                });

            migrationBuilder.UpdateData(
                table: "Trips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(384), new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(384), new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(383) });

            migrationBuilder.UpdateData(
                table: "Trips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(392), new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(391), new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(391) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(174));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 3, 17, 7, 37, 535, DateTimeKind.Local).AddTicks(203));

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_Token",
                table: "PasswordResets",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_SavedTrips_TripId",
                table: "SavedTrips",
                column: "TripId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResets");

            migrationBuilder.DropTable(
                name: "SavedTrips");

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "Id",
                keyValue: 1,
                column: "BookingDate",
                value: new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8680));

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "Id",
                keyValue: 2,
                column: "BookingDate",
                value: new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8684));

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 1,
                column: "PaymentDate",
                value: new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 2,
                column: "PaymentDate",
                value: new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8714));

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8741));

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8744));

            migrationBuilder.UpdateData(
                table: "Trips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8644), new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8644), new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8643) });

            migrationBuilder.UpdateData(
                table: "Trips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8652), new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8651), new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8651) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8439));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 9, 19, 53, 30, 265, DateTimeKind.Local).AddTicks(8464));
        }
    }
}
