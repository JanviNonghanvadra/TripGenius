using Microsoft.EntityFrameworkCore;
using TripGenius.Models;

namespace TripGenius.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SavedTrip> SavedTrips { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= USERS =================
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Admin User",
                    Email = "admin@trip.com",
                    PasswordHash = "123456",
                    Role = "Admin",
                    Phone = "9999999999",
                    Status = "Active",
                    TripsCount = 5,
                    TotalSpent = 0
                },
                new User
                {
                    Id = 2,
                    Name = "John Doe",
                    Email = "user@trip.com",
                    PasswordHash = "123456",
                    Role = "User",
                    Phone = "8888888888",
                    Status = "Active",
                    TripsCount = 2,
                    TotalSpent = 20000
                }
            );

            // ================= TRIPS =================
            modelBuilder.Entity<Trip>().HasData(
                new Trip
                {
                    Id = 1,
                    Title = "Goa Beach Trip",
                    Destination = "Goa",
                    Description = "Enjoy beaches",
                    Price = 10000,
                    DurationDays = 3,
                    Status = "Active"
                },
                new Trip
                {
                    Id = 2,
                    Title = "Manali Adventure",
                    Destination = "Manali",
                    Description = "Mountain trip",
                    Price = 15000,
                    DurationDays = 5,
                    Status = "Active"
                }
            );

            // ================= SAVED TRIPS =================
            modelBuilder.Entity<SavedTrip>().HasData(
                new SavedTrip { Id = 1, UserId = 2, TripId = 1, CreatedAt = System.DateTime.UtcNow },
                new SavedTrip { Id = 2, UserId = 2, TripId = 2, CreatedAt = System.DateTime.UtcNow }
            );

            // ================= BOOKINGS =================
            modelBuilder.Entity<Booking>().HasData(
                new Booking
                {
                    Id = 1,
                    UserId = 2,
                    TripId = 1,
                    NumberOfPeople = 2,
                    TotalAmount = 20000,
                    Status = "Confirmed"
                },
                new Booking
                {
                    Id = 2,
                    UserId = 2,
                    TripId = 2,
                    NumberOfPeople = 1,
                    TotalAmount = 15000,
                    Status = "Pending"
                }
            );

            // ================= PAYMENTS =================
            modelBuilder.Entity<Payment>().HasData(
                new Payment
                {
                    Id = 1,
                    BookingId = 1,
                    Amount = 20000,
                    PaymentMethod = "UPI",
                    Status = "Success"
                },
                new Payment
                {
                    Id = 2,
                    BookingId = 2,
                    Amount = 15000,
                    PaymentMethod = "Card",
                    Status = "Pending"
                }
            );

            // ================= REVIEWS =================
            modelBuilder.Entity<Review>().HasData(
                new Review
                {
                    Id = 1,
                    UserId = 2,
                    TripId = 1,
                    Rating = 5,
                    Comment = "Amazing trip!",
                    Status = "Approved"
                },
                new Review
                {
                    Id = 2,
                    UserId = 2,
                    TripId = 2,
                    Rating = 4,
                    Comment = "Good experience",
                    Status = "Pending"
                }
            );

            // ================= SETTINGS =================
            modelBuilder.Entity<Setting>().HasData(
                new Setting
                {
                    Id = 1,
                    SiteName = "TripGenius",
                    SiteDescription = "Travel booking system",
                    ContactEmail = "support@trip.com",
                    Phone = "7777777777",
                    EmailNotifications = true,
                    NewUserRegistration = true,
                    BookingConfirmations = true,
                    ReviewNotifications = true,
                    PaymentAlerts = true,
                    Currency = "INR",
                    CurrencySymbol = "₹",
                    ProcessingFee = 2,
                    FixedFee = 10
                }
            );

            // ================= DECIMAL PRECISION =================
            modelBuilder.Entity<Booking>().Property(b => b.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Setting>().Property(s => s.FixedFee).HasPrecision(18, 2);
            modelBuilder.Entity<Setting>().Property(s => s.ProcessingFee).HasPrecision(18, 2);
            modelBuilder.Entity<Trip>().Property(t => t.Budget).HasPrecision(18, 2);
            modelBuilder.Entity<Trip>().Property(t => t.Price).HasPrecision(18, 2);
            modelBuilder.Entity<User>().Property(u => u.TotalSpent).HasPrecision(18, 2);

            // Password resets
            modelBuilder.Entity<TripGenius.Models.PasswordReset>().ToTable("PasswordResets");
        }
    }
}
