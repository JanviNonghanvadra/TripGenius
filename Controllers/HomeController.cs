using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using TripGenius.Data;
using TripGenius.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;

namespace TripGenius.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Guest Landing Page
        public async Task<IActionResult> Index()
        {
            var trips = await _context.Trips.Where(t => t.Status == "Active").ToListAsync();
            return View(trips);
        }

        // Guest Explore
        public async Task<IActionResult> Explore()
        {
            var trips = await _context.Trips.Where(t => t.Status == "Active").ToListAsync();
            return View(trips);
        }

        // Save trip (mark as saved by current user)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTrip(int id)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
                return NotFound();

            // Mark as saved for this user (uses Trip.UserId and IsSaved fields present in model)
            trip.IsSaved = true;
            trip.UserId = userId;

            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // Guest Plan Trip
        public IActionResult PlanTrip()
        {
            return View();
        }



        // Forgot Password - GET
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Forgot Password - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // keep same UX: always redirect to confirmation
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    // create token
                    var token = Guid.NewGuid().ToString("N");
                    var pr = new PasswordReset { UserId = user.Id, Token = token, ExpiresAt = DateTime.UtcNow.AddHours(1), CreatedAt = DateTime.UtcNow };
                    _context.Add(pr);
                    await _context.SaveChangesAsync();

                    // build reset link
                    var resetUrl = Url.Action("ResetPassword", "Home", new { token = token, email = user.Email }, Request.Scheme);

                    // send email using SMTP settings saved in Settings table
                    try
                    {
                        var s = _context.Settings.FirstOrDefault();
                        if (s != null && !string.IsNullOrEmpty(s.SMTPHost) && !string.IsNullOrEmpty(s.SMTPUsername))
                        {
                            var msg = new MimeMessage();
                            msg.From.Add(new MailboxAddress(s.SiteName ?? "TripGenius", s.SMTPUsername));
                            msg.To.Add(MailboxAddress.Parse(user.Email));
                            msg.Subject = "Reset your TripGenius password";

                            msg.Body = new TextPart("html") { Text = $"<p>Hello {user.Name},</p><p>Click the link below to reset your password (valid for 60 minutes):</p><p><a href=\"{resetUrl}\">Reset password</a></p><p>If you didn't request this, ignore this email.</p>" };

                            using var client = new SmtpClient();
                            client.Connect(s.SMTPHost, s.SMTPPort ?? 587, s.Encryption == "SSL");
                            if (!string.IsNullOrEmpty(s.SMTPUsername)) client.Authenticate(s.SMTPUsername, s.SMTPPassword);
                            client.Send(msg);
                            client.Disconnect(true);
                        }
                        else
                        {
                            _logger.LogWarning("SMTP settings not configured - skipping email send");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send reset email");
                    }
                }

                TempData["ForgotEmail"] = email;
            }
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");
            var pr = await _context.PasswordResets.FirstOrDefaultAsync(p => p.Token == token && p.ExpiresAt > DateTime.UtcNow);
            if (pr == null) { TempData["Error"] = "Invalid or expired token."; return RedirectToAction("ForgotPassword"); }
            var vm = new ResetPasswordViewModel { Token = token, Email = email };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var pr = await _context.PasswordResets.FirstOrDefaultAsync(p => p.Token == model.Token && p.ExpiresAt > DateTime.UtcNow);
            if (pr == null) { ModelState.AddModelError("", "Invalid or expired token."); return View(model); }
            var user = await _context.Users.FindAsync(pr.UserId);
            if (user == null) { ModelState.AddModelError("", "User not found."); return View(model); }

            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
            _context.PasswordResets.Remove(pr);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Password updated. Please sign in.";
            return RedirectToAction("Login", "Account");
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
