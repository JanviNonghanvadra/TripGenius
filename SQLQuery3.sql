-- 1. CLEAN UP (Delete existing data to reset IDs)
-- We delete in this order to avoid constraint errors
DELETE FROM [Payments];
DELETE FROM [Bookings];
DELETE FROM [Trips];
DELETE FROM [Users];
DELETE FROM [Settings];

-- 2. INSERT SETTINGS
INSERT INTO [Settings] (SiteName, SiteDescription, ContactEmail, Phone, EmailNotifications, NewUserRegistration, BookingConfirmations, ReviewNotifications, PaymentAlerts, SMTPHost, SMTPPort, Encryption, SMTPUsername, SMTPPassword, TwoFactorAuth, LoginAlerts, Currency, CurrencySymbol, ProcessingFee, FixedFee) 
VALUES ('TripGenius', 'Your trusted partner for memorable travel experiences.', 'support@tripgenius.in', '+91 98765 43210', 1, 1, 1, 1, 1, 'smtp.tripgenius.in', 587, 'TLS', 'admin@tripgenius.in', 'Pass123', 0, 1, 'INR', '₹', 2.5, 50.00);

-- 3. INSERT TRIP & USER
INSERT INTO [Trips] (Title, Destination, Description, Price, DurationDays, Status, CreatedAt) 
VALUES ('Golden Triangle Luxury', 'Delhi-Agra-Jaipur', '5-day heritage tour.', 45000.00, 5, 'Active', GETDATE());

INSERT INTO [Users] (Name, Email, PasswordHash, Role, Phone, Status, CreatedAt, TripsCount, TotalSpent) 
VALUES ('Amit Sharma', 'admin@tripgenius.com', 'AQAAAAEAACcQAAAAE...', 'Admin', '9820012345', 'Active', GETDATE(), 0, 0);

-- 4. INSERT BOOKING (Dynamic ID Selection)
-- This part automatically finds the ID of the Trip and User we just created
DECLARE @UserId INT = (SELECT TOP 1 Id FROM Users WHERE Email = 'admin@tripgenius.com');
DECLARE @TripId INT = (SELECT TOP 1 Id FROM Trips WHERE Title = 'Golden Triangle Luxury');

INSERT INTO [Bookings] (UserId, TripId, BookingDate, NumberOfPeople, TotalAmount, Status) 
VALUES (@UserId, @TripId, GETDATE(), 2, 90000.00, 'Confirmed');

-- 5. INSERT PAYMENT (Dynamic ID Selection)
DECLARE @BookingId INT = (SELECT TOP 1 Id FROM Bookings WHERE UserId = @UserId);

INSERT INTO [Payments] (BookingId, Amount, PaymentMethod, Status, PaymentDate) 
VALUES (@BookingId, 90000.00, 'UPI', 'Success', GETDATE());

-- 6. VERIFY DATA
SELECT 'Success' as Status;
SELECT * FROM Settings;