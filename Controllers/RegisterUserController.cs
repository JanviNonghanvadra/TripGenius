using Microsoft.AspNetCore.Mvc;
using TripGenius.Data;
using TripGenius.Models;
using TripGenius.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Identity;
using System;

namespace TripGenius.Controllers
{
    [Authorize]
    [Route("RegisterUser/[action]")]
    public class RegisterUserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterUserController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly PasswordHasher<User> _passwordHasher;

        public RegisterUserController(ApplicationDbContext context, ILogger<RegisterUserController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<IActionResult> Home()
        {
            var trips = await _context.Trips.Where(t => t.Status == "Active").ToListAsync();
            return View(trips);
        }

        public async Task<IActionResult> Explore()
        {
            var trips = await _context.Trips.Where(t => t.Status == "Active").ToListAsync();

            // determine which trips are already saved by current user (if authenticated)
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out var userId))
                {
                    var savedIds = await _context.SavedTrips
                        .Where(s => s.UserId == userId)
                        .Select(s => s.TripId)
                        .ToListAsync();
                    ViewBag.SavedTripIds = savedIds;
                }
                else
                {
                    ViewBag.SavedTripIds = new List<int>();
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Unable to determine saved trips for user when rendering Explore view.");
                ViewBag.SavedTripIds = new List<int>();
            }

            return View(trips);
        }

        public async Task<IActionResult> PlanTrip()
        {
            // Try to fetch the current user's name and pass it to the view for pre-filling the card name
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdString))
                {
                    if (int.TryParse(userIdString, out var userId))
                    {
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                        if (user != null)
                        {
                            ViewBag.CardName = user.Name ?? string.Empty;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Unable to prefill card name for PlanTrip view.");
            }

            return View();
        }

        // Toggle save/un-save for existing trip for the authenticated user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSave(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized();

            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
            if (trip == null)
            {
                // If AJAX expect JSON, otherwise redirect back with error
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Trip not found" });
                TempData["Error"] = "Trip not found.";
                return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Explore", "RegisterUser"));
            }

            try
            {
                // Use SavedTrips table to store per-user bookmarks
                var existing = await _context.SavedTrips.FirstOrDefaultAsync(s => s.TripId == id && s.UserId == userId);
                if (existing != null)
                {
                    _context.SavedTrips.Remove(existing);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = true, isSaved = false });

                    TempData["Success"] = "Removed from saved trips.";
                    return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Explore", "RegisterUser"));
                }
                else
                {
                    var saved = new SavedTrip
                    {
                        TripId = id,
                        UserId = userId,
                        CreatedAt = System.DateTime.UtcNow
                    };
                    _context.SavedTrips.Add(saved);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = true, isSaved = true });

                    TempData["Success"] = "Saved to your trips.";
                    return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Explore", "RegisterUser"));
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle save state for trip.");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Failed to toggle save" });

                TempData["Error"] = "Failed to toggle save state.";
                return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Explore", "RegisterUser"));
            }
        }

        // Load saved trips from DB and pass to view
        public async Task<IActionResult> SavedTrip()
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
                int userId = int.Parse(userIdString);

                var trips = await _context.SavedTrips
                    .Where(s => s.UserId == userId)
                    .Include(s => s.Trip)
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => s.Trip!)
                    .Where(t => t != null)
                    .ToListAsync();

                return View(trips);
            }
            catch (System.Exception ex)
            {
                // Log and fall back to an empty list so the page still loads on static or mismatch schemas
                _logger.LogError(ex, "Failed to load saved trips from database. Falling back to empty list.");

                // Optionally show a friendly message in the view
                ViewData["SavedTripsError"] = "Saved trips are not available right now (database schema mismatch or migrations missing).";

                return View(Enumerable.Empty<Trip>());
            }
        }

        public async Task<IActionResult> Profile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var user = await _context.Users
                .Include(u => u.Reviews)
                .Include(u => u.Bookings).ThenInclude(b => b.Trip)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // compute some helper counts
            var savedCount = await _context.SavedTrips.CountAsync(s => s.UserId == userId);
            var reviewsCount = await _context.Reviews.CountAsync(r => r.UserId == userId);
            var tripsCreated = await _context.Trips.CountAsync(t => t.UserId == userId);
            var countriesCount = await _context.Bookings
                .Include(b => b.Trip)
                .Where(b => b.UserId == userId && b.Trip != null)
                .Select(b => b.Trip.Destination)
                .Distinct()
                .CountAsync();

            ViewBag.SavedCount = savedCount;
            ViewBag.ReviewsCount = reviewsCount;
            ViewBag.TripsCount = tripsCreated > 0 ? tripsCreated : user.TripsCount;
            ViewBag.CountriesCount = countriesCount;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(IFormCollection form, IFormFile? profileImage)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            try
            {
                // Handle profile image upload if present
                if (profileImage != null && profileImage.Length > 0)
                {
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var ext = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
                    if (!allowed.Contains(ext))
                    {
                        TempData["Error"] = "Invalid image type.";
                        return RedirectToAction("Profile");
                    }

                    var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "profiles");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    var fileName = System.Guid.NewGuid().ToString() + ext;
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }

                    user.ProfileImageUrl = "/uploads/profiles/" + fileName;
                }

                // update basic fields if present
                var first = form["firstName"].ToString();
                var last = form["lastName"].ToString();
                var email = form["email"].ToString();
                var phone = form["phone"].ToString();

                if (!string.IsNullOrWhiteSpace(first) || !string.IsNullOrWhiteSpace(last))
                {
                    user.Name = (first + " " + last).Trim();
                }
                if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
                if (!string.IsNullOrWhiteSpace(phone)) user.Phone = phone;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile updated successfully.";
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to update profile.");
                TempData["Error"] = "Failed to update profile.";
            }

            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> Settings()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            var settings = await _context.Settings.FirstOrDefaultAsync();

            var vm = new SettingsViewModel
            {
                User = user,
                Settings = settings,
                // populate preferences/notifications from settings
                EmailNotifications = settings?.EmailNotifications ?? false,
                NewUserRegistration = settings?.NewUserRegistration ?? false,
                BookingConfirmations = settings?.BookingConfirmations ?? false,
                ReviewNotifications = settings?.ReviewNotifications ?? false,
                PaymentAlerts = settings?.PaymentAlerts ?? false,
                Currency = settings?.Currency ?? "INR",
                Language = "en",
                TravelStyle = settings?.SiteDescription ?? "Standard",
                TimeZone = "IST"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotifications(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // repopulate view model pieces and return
                TempData["Error"] = "Invalid notification data.";
                return RedirectToAction("Settings");
            }

            var settings = await _context.Settings.FirstOrDefaultAsync();
            if (settings == null)
            {
                TempData["Error"] = "Settings not available.";
                return RedirectToAction("Settings");
            }

            settings.EmailNotifications = model.EmailNotifications;
            settings.NewUserRegistration = model.NewUserRegistration;
            settings.BookingConfirmations = model.BookingConfirmations;
            settings.ReviewNotifications = model.ReviewNotifications;
            settings.PaymentAlerts = model.PaymentAlerts;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Notification preferences updated.";
            return RedirectToAction("Settings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePreferences(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid preferences data.";
                return RedirectToAction("Settings");
            }

            var settings = await _context.Settings.FirstOrDefaultAsync();
            if (settings == null)
            {
                TempData["Error"] = "Settings not available.";
                return RedirectToAction("Settings");
            }

            if (!string.IsNullOrWhiteSpace(model.Currency))
            {
                settings.Currency = model.Currency;
                settings.CurrencySymbol = model.Currency switch
                {
                    "USD" => "$",
                    "EUR" => "€",
                    "GBP" => "£",
                    _ => "?"
                };
            }

            if (!string.IsNullOrWhiteSpace(model.TravelStyle)) settings.SiteDescription = model.TravelStyle;
            if (!string.IsNullOrWhiteSpace(model.Language)) { /* store if needed */ }
            if (!string.IsNullOrWhiteSpace(model.TimeZone)) { /* store if needed */ }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Preferences updated.";
            return RedirectToAction("Settings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // collect validation messages
                TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("Settings");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var current = model.CurrentPassword ?? string.Empty;
            var nw = model.NewPassword ?? string.Empty;

            // Try verifying hashed password first
            var verifyResult = PasswordVerificationResult.Failed;
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                try
                {
                    verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, current);
                }
                catch
                {
                    verifyResult = PasswordVerificationResult.Failed;
                }
            }

            // Fallback to plain-text comparison if verification failed (legacy)
            var ok = verifyResult == PasswordVerificationResult.Success || user.PasswordHash == current;
            if (!ok)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Settings");
            }

            // Hash new password and save
            user.PasswordHash = _passwordHasher.HashPassword(user, nw);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Password updated.";
            return RedirectToAction("Settings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateAccount()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Status = "Inactive";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Account deactivated.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Account deleted.";
                return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to delete account.");
                TempData["Error"] = "Failed to delete account.";
                return RedirectToAction("Settings");
            }
        }

        // Handle requests to /Delete (root) by redirecting to the controller confirmation page
        [AllowAnonymous]
        [HttpGet("/Delete")]
        public IActionResult DeleteRoot()
        {
            // Redirect to the RegisterUser/Delete confirmation page
            return RedirectToAction("Delete");
        }

        [HttpGet("Delete")]
        [AllowAnonymous]
        public IActionResult Delete()
        {
            // Show a confirmation page that posts to DeleteAccount or DeleteConfirmed
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed()
        {
            // If a form posts to /RegisterUser/DeleteConfirmed use same logic as DeleteAccount
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Account deleted.";
                return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to delete account.");
                TempData["Error"] = "Failed to delete account.";
                return RedirectToAction("Settings");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(IFormCollection form)
        {
            // Accept POST to /RegisterUser/Delete (some forms or JS may post here). Reuse DeleteConfirmed logic.
            return await DeleteConfirmed();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // DTO for incoming save requests
        public class SaveTripDto
        {
            public string Destination { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;
            public string? ImageUrl { get; set; }
            public string? Price { get; set; }
            public string? Tag { get; set; }
            public string? Rating { get; set; }
            public decimal Budget { get; set; } = 0m;
        }

        [HttpPost]
        public async Task<IActionResult> SaveTrip([FromBody] SaveTripDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Destination))
                return BadRequest(new { success = false, message = "Invalid data" });

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized(new { success = false, message = "Not authenticated" });
            int userId = int.Parse(userIdString);

            var trip = new Trip
            {
                Destination = dto.Destination,
                Description = dto.Tag ?? dto.Country,
                ImageUrl = dto.ImageUrl,
                Budget = dto.Budget,
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow,
                UserId = userId,
                IsSaved = true,
                CreatedAt = System.DateTime.UtcNow
            };

            try
            {
                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = trip.Id });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to save trip to database. Returning failure.");
                return StatusCode(500, new { success = false, message = "Error saving trip" });
            }
        }

        // DTO for incoming booking requests from PlanTrip Checkout Modal
        public class TripBookingDto
        {
            public string Destination { get; set; } = string.Empty;
            public System.DateTime DepartureDate { get; set; }
            public System.DateTime ReturnDate { get; set; }
            public int Travelers { get; set; }
            public decimal Budget { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitBooking([FromBody] TripBookingDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Destination))
                return BadRequest(new { success = false, message = "Invalid data" });

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized(new { success = false, message = "Not authenticated" });
            int userId = int.Parse(userIdString);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Create Trip (Booked but not "Saved" in the bookmark sense)
                var trip = new Trip
                {
                    Title = "Planned Trip to " + dto.Destination,
                    Destination = dto.Destination,
                    Description = "Custom Trip",
                    Budget = dto.Budget,
                    Price = dto.Budget,
                    StartDate = dto.DepartureDate,
                    EndDate = dto.ReturnDate,
                    UserId = userId,
                    IsSaved = false,
                    CreatedAt = System.DateTime.UtcNow
                };
                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();

                // 2. Create Booking
                var totalAmount = dto.Budget * dto.Travelers; // Internal calculation per requirement

                var booking = new Booking
                {
                    UserId = userId,
                    TripId = trip.Id,
                    BookingDate = System.DateTime.UtcNow,
                    NumberOfPeople = dto.Travelers,
                    TotalAmount = totalAmount,
                    Status = "Confirmed"
                };
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // 3. Create Payment
                var payment = new Payment
                {
                    BookingId = booking.Id,
                    Amount = totalAmount,
                    PaymentMethod = "Card", // Hardcoded mock
                    Status = "Success",
                    PaymentDate = System.DateTime.UtcNow
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // 4. Update User Data
                var user2 = await _context.Users.FindAsync(userId);
                if (user2 != null)
                {
                    user2.TripsCount += 1;
                    user2.TotalSpent += totalAmount;
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Json(new { success = true });
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to submit booking workflow.");
                return StatusCode(500, new { success = false, message = "Error processing payment and booking" });
            }
        }

        // DTO for full plan submission from PlanTrip page
        public class PlanTripDto
        {
            public string Destination { get; set; } = string.Empty;
            public string? TripType { get; set; }
            public DateTime? DepartureDate { get; set; }
            public DateTime? ReturnDate { get; set; }
            public int Travelers { get; set; } = 1;
            public decimal Budget { get; set; } = 0m;
            public string? Notes { get; set; }
            public string? BudgetPref { get; set; }
            public string? ImageUrl { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SavePlannedTrip([FromBody] PlanTripDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Destination))
                return BadRequest(new { success = false, message = "Invalid data" });

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized(new { success = false, message = "Not authenticated" });
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized(new { success = false, message = "Invalid user" });

            try
            {
                var trip = new Trip
                {
                    Title = !string.IsNullOrWhiteSpace(dto.TripType) ? dto.TripType + " - " + dto.Destination : "Planned Trip to " + dto.Destination,
                    Destination = dto.Destination,
                    Description = dto.Notes ?? dto.TripType ?? string.Empty,
                    ImageUrl = dto.ImageUrl,
                    Budget = dto.Budget,
                    Price = dto.Budget,
                    StartDate = dto.DepartureDate ?? DateTime.UtcNow,
                    EndDate = dto.ReturnDate ?? (dto.DepartureDate ?? DateTime.UtcNow),
                    DurationDays = (dto.DepartureDate.HasValue && dto.ReturnDate.HasValue) ? (int)Math.Max(0, (dto.ReturnDate.Value - dto.DepartureDate.Value).TotalDays) : 0,
                    UserId = userId,
                    IsSaved = true,
                    Status = "Planned",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();

                return Json(new { success = true, id = trip.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save planned trip.");
                return StatusCode(500, new { success = false, message = "Error saving planned trip" });
            }
        }
    }
}

