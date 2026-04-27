using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripGenius.Migrations
{
    /// <inheritdoc />
    public partial class CreateSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    NewUserRegistration = table.Column<bool>(type: "bit", nullable: false),
                    BookingConfirmations = table.Column<bool>(type: "bit", nullable: false),
                    ReviewNotifications = table.Column<bool>(type: "bit", nullable: false),
                    PaymentAlerts = table.Column<bool>(type: "bit", nullable: false),
                    SMTPHost = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SMTPPort = table.Column<int>(type: "int", nullable: false),
                    Encryption = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SMTPUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SMTPPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TwoFactorAuth = table.Column<bool>(type: "bit", nullable: false),
                    LoginAlerts = table.Column<bool>(type: "bit", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrencySymbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FixedFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
