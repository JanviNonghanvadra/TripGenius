using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using TripGenius.Data;
using TripGenius.Models;
using Microsoft.AspNetCore.Identity;

namespace TripGenius.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= LOGIN GET =================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ================= LOGIN POST =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            // Verify hashed password safely — handle legacy/plaintext seeds that aren't valid Base64
            var hasher = new PasswordHasher<User>();
            PasswordVerificationResult result;
            try
            {
                result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            }
            catch (FormatException)
            {
                // Stored password is not a valid hashed value (likely seeded plain-text). Try plaintext fallback.
                if (user.PasswordHash == password)
                {
                    // Re-hash and persist the password in hashed form for future logins
                    user.PasswordHash = hasher.HashPassword(user, password);
                    await _context.SaveChangesAsync();
                    result = PasswordVerificationResult.Success;
                }
                else
                {
                    result = PasswordVerificationResult.Failed;
                }
            }

            // If verification indicates rehash is needed, re-hash and save
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = hasher.HashPassword(user, password);
                await _context.SaveChangesAsync();
                result = PasswordVerificationResult.Success;
            }

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            // Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "TripGeniusCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("TripGeniusCookie", principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("Home", "RegisterUser");
            }
        }

        // ================= LOGOUT =================
        [HttpGet, HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("TripGeniusCookie");
            return RedirectToAction("Index", "Home");
        }

        // ================= REGISTER GET =================
        public IActionResult Register()
        {
            return View();
        }

        // ================= REGISTER POST =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string password, string phone)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["RegErrors"] = "All fields are required.";
                return View();
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                ViewData["RegErrors"] = "Email already registered.";
                return View();
            }

            var name = $"{firstName} {lastName}".Trim();
            var user = new User { Name = name, Email = email, Phone = phone, Role = "User", Status = "Active", CreatedAt = DateTime.Now };
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account created! Please login.";
            return RedirectToAction("Login");
        }
    }
}
