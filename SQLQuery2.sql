-- 1. Insert Site Settings (Table name is Settings)
INSERT INTO [Settings] (SiteName, SiteDescription, ContactEmail, Phone, EmailNotifications, NewUserRegistration, BookingConfirmations, ReviewNotifications, PaymentAlerts, SMTPHost, SMTPPort, Encryption, SMTPUsername, SMTPPassword, TwoFactorAuth, LoginAlerts, Currency, CurrencySymbol, ProcessingFee, FixedFee) 
VALUES ('TripGenius', 'Your trusted partner for memorable travel experiences.', 'support@tripgenius.in', '+91 98765 43210', 1, 1, 1, 0, 1, 'smtp.tripgenius.in', 587, 'TLS', 'admin@tripgenius.in', 'Pass123', 0, 1, 'INR', '₹', 2.5, 50.00);

-- 2. Insert Users (Table name is Users)
INSERT INTO [Users] (Name, Email, PasswordHash, Role, Phone, Status, CreatedAt, TripsCount, TotalSpent) 
VALUES ('Amit Sharma', 'admin@tripgenius.com', 'AQAAAAEAACcQAAAAE...', 'Admin', '9820012345', 'Active', GETDATE(), 0, 0);

-- 3. Insert Trips (Table name is Trips)
INSERT INTO [Trips] (Title, Destination, Description, Price, DurationDays, Status, CreatedAt) 
VALUES 
('Golden Triangle Luxury', 'Delhi-Agra-Jaipur', '5-day heritage tour.', 45000.00, 5, 'Active', GETDATE()),
('Manali Adventure', 'Himachal', 'Snow sports and trekking.', 18500.00, 3, 'Active', GETDATE());

-- 4. Insert Bookings (Table name is Bookings)
INSERT INTO [Bookings] (UserId, TripId, BookingDate, NumberOfPeople, TotalAmount, Status) 
VALUES (1, 1, '2026-03-22', 2, 90000.00, 'Confirmed');