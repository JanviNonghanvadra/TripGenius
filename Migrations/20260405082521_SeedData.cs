using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TripGenius.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "BookingConfirmations", "ContactEmail", "Currency", "CurrencySymbol", "EmailNotifications", "Encryption", "FixedFee", "LoginAlerts", "NewUserRegistration", "PaymentAlerts", "Phone", "ProcessingFee", "ReviewNotifications", "SMTPHost", "SMTPPassword", "SMTPPort", "SMTPUsername", "SiteDescription", "SiteName", "TwoFactorAuth" },
                values: new object[] { 1, true, "support@trip.com", "INR", "₹", true, null, 10m, false, true, true, "7777777777", 2m, true, null, null, null, null, "Travel booking system", "TripGenius", false });

            migrationBuilder.InsertData(
                table: "Trips",
                columns: new[] { "Id", "CreatedAt", "Description", "Destination", "DurationDays", "ImageUrl", "Price", "Status", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 4, 5, 13, 55, 18, 542, DateTimeKind.Local).AddTicks(6747), "Enjoy beaches", "Goa", 3, null, 10000m, "Active", "Goa Beach Trip" },
                    { 2, new DateTime(2026, 4, 5, 13, 55, 18, 542, DateTimeKind.Local).AddTicks(6755), "Mountain trip", "Manali", 5, null, 15000m, "Active", "Manali Adventure" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "Name", "PasswordHash", "Phone", "Role", "Status", "TotalSpent", "TripsCount" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 4, 5, 13, 55, 18, 542, DateTimeKind.Local).AddTicks(6589), "admin@trip.com", "Admin User", "123456", "9999999999", "Admin", "Active", 0m, 5 },
                    { 2, new DateTime(2026, 4, 5, 13, 55, 18, 542, DateTimeKind.Local).AddTicks(6610), "user@trip.com", "John Doe", "123456", "8888888888", "User", "Active", 20000m, 2 }
                });

            migrationBuilder.InsertData(
                table: "Bookings",
                columns: new[] { "Id", "BookingDate", "NumberOfPeople", "Status", "TotalAmount", "TripId", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 4, 5, 13, 55, 18, 542, DateTimeKind.Local).AddTicks(6776), 2, "Confirmed", 20000m, 1, 2 },
                    { 2, new DateTime(2026, 4, 5, 13, 55, 18, 542, DateTimeKind.Local).AddTicks(6781), 1, "Pending", 15000m, 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "Comment", "CreatedAt", "Rating", "Status", "TripId", "UserId" },
                values: new object[,]
                {
                    { 1, "Amazing trip!", new DateTime(2026, 4, 5, 13, 55, 18, 542, DateTimeKind.Local).AddTicks(6827), 5, "Approved", 1, 2 },
                    { 2, "Good experience", new DateTime(2026, 4, 5, 13, 55, 18, 542, DateTimeKind.Local).AddTicks(6830), 4, "Pending", 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "Amount", "BookingId", "PaymentDate", "PaymentMethod", "Status" },
                values: new object[,]
                {
                    { 1, 20000m, 1, new DateTime(2026, 4, 5, 13, 55, 18, 542, DateTimeKind.Local).AddTicks(6800), "UPI", "Success" },
                    { 2, 15000m, 2, new DateTime(2026, 4, 5, 13, 55, 18, 542, DateTimeKind.Local).AddTicks(6803), "Card", "Pending" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Trips",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Trips",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
