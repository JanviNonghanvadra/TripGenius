INSERT INTO Trips
(Title, Destination, Description, Price, DurationDays, ImageUrl, CreatedAt, Status, Budget, EndDate, IsSaved, StartDate, UserId)
VALUES 

('Goa Beach Trip', 'Beach', 'Enjoy beaches', 10000.00, 3, NULL, GETDATE(), 'Active', 0.00, GETDATE(), 0, GETDATE(), 0),

('Manali Adventure', 'Mountain', 'Mountain trip', 15000.00, 5, NULL, GETDATE(), 'Active', 0.00, GETDATE(), 0, GETDATE(), 0),

('Santorini Trip', 'Greece', 'Iconic white-washed buildings above blue sea', 95000.00, 5, 'https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff', GETDATE(), 'Active', 95000.00, DATEADD(DAY,5,GETDATE()), 0, GETDATE(), 1),

('Machu Picchu Trip', 'Peru', 'Ancient Incan citadel in Andes', 110000.00, 6, 'https://images.unsplash.com/photo-1526392060635-9d6019884377', GETDATE(), 'Active', 110000.00, DATEADD(DAY,6,GETDATE()), 0, GETDATE(), 1),

('Kenya Safari', 'Africa', 'Wildlife safari experience', 150000.00, 7, 'https://images.unsplash.com/photo-1516426122078-c23e76319801', GETDATE(), 'Active', 150000.00, DATEADD(DAY,7,GETDATE()), 0, GETDATE(), 1),

('New York Tour', 'USA', 'City that never sleeps', 120000.00, 5, 'https://images.unsplash.com/photo-1496442226666-8d4d0e62e6e9', GETDATE(), 'Active', 120000.00, DATEADD(DAY,5,GETDATE()), 0, GETDATE(), 1),

('Amalfi Coast Trip', 'Italy', 'Cliffs and sea views', 88000.00, 4, 'https://images.unsplash.com/photo-1533606688076-b6683a5f5f62', GETDATE(), 'Active', 88000.00, DATEADD(DAY,4,GETDATE()), 0, GETDATE(), 1),

('Kyoto Culture Tour', 'Japan', 'Temples and traditions', 78000.00, 5, 'https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e', GETDATE(), 'Active', 78000.00, DATEADD(DAY,5,GETDATE()), 0, GETDATE(), 1),

('Patagonia Adventure', 'Argentina', 'Glaciers and mountains', 135000.00, 7, 'https://images.unsplash.com/photo-1519331379826-f10be5486c6f', GETDATE(), 'Active', 135000.00, DATEADD(DAY,7,GETDATE()), 0, GETDATE(), 1),

('Dubai City Tour', 'UAE', 'Luxury lifestyle and desert', 65000.00, 3, 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c', GETDATE(), 'Active', 65000.00, DATEADD(DAY,3,GETDATE()), 0, GETDATE(), 1),

('Amazon Jungle Trip', 'Brazil', 'Rainforest exploration', 100000.00, 6, 'https://images.unsplash.com/photo-1558618666-fcd25c85cd64', GETDATE(), 'Active', 100000.00, DATEADD(DAY,6,GETDATE()), 0, GETDATE(), 1),

('Bali Vacation', 'Indonesia', 'Beaches and temples', 45000.00, 4, 'https://images.unsplash.com/photo-1501854140801-50d01698950b', GETDATE(), 'Active', 45000.00, DATEADD(DAY,4,GETDATE()), 0, GETDATE(), 1),

('Swiss Alps Tour', 'Switzerland', 'Snow mountains and skiing', 140000.00, 6, 'https://images.unsplash.com/photo-1506905925346-21bda4d32df4', GETDATE(), 'Active', 140000.00, DATEADD(DAY,6,GETDATE()), 0, GETDATE(), 1),

('Serengeti Safari', 'Tanzania', 'Savannah wildlife', 160000.00, 7, 'https://images.unsplash.com/photo-1547471080-7cc2caa01a7e', GETDATE(), 'Active', 160000.00, DATEADD(DAY,7,GETDATE()), 0, GETDATE(), 1);

SELECT * FROM Trips
