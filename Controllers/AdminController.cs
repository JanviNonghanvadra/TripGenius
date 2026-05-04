using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TripGenius.Models;
using TripGenius.Data;
using TripGenius.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TripGenius.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context) { _context = context; }

        private User? GetCurrentAdminUser()
        {
            var email = User?.FindFirstValue(ClaimTypes.Email) ?? User?.Identity?.Name;
            User? user = null;
            if (!string.IsNullOrEmpty(email))
                user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                user = _context.Users.FirstOrDefault(u => u.Role == "Admin") ?? _context.Users.FirstOrDefault();
            return user;
        }

        private void SetNavbarData()
        {
            var admin = GetCurrentAdminUser();
            ViewBag.AdminName    = admin?.Name  ?? "Admin User";
            ViewBag.AdminEmail   = admin?.Email ?? "admin@travel.com";
            ViewBag.AdminInitial = admin?.Name?.Substring(0, 1).ToUpper() ?? "A";
            ViewBag.PendingNotifications = _context.Bookings.Count(b => b.Status == "Pending") + _context.Reviews.Count(r => r.Status == "Pending");
        }

        // DASHBOARD
        public IActionResult Dashboard()
        {
            ViewData["ActivePage"] = "Dashboard";
            SetNavbarData();
            ViewBag.UserCount    = _context.Users.Count();
            ViewBag.BookingCount = _context.Bookings.Count();
            ViewBag.TripsCount   = _context.Trips.Count();
            ViewBag.Revenue      = _context.Payments.Sum(p => (decimal?)p.Amount) ?? 0;

            var recentBookings = _context.Bookings
                .Include(b => b.User).Include(b => b.Trip).Include(b => b.Payment)
                .OrderByDescending(b => b.BookingDate).Take(5)
                .Select(b => new BookingViewModel {
                    Id = b.Id, CustomerName = b.User != null ? b.User.Name : "Unknown",
                    TripName = b.Trip != null ? b.Trip.Title : "-",
                    Amount = b.TotalAmount, Status = b.Status ?? "Pending",
                    PaymentStatus = b.Payment != null ? b.Payment.Status : "Pending",
                    BookingDate = b.BookingDate
                }).ToList();

            var now = DateTime.Now;
            var months = Enumerable.Range(0, 6).Select(i => now.AddMonths(-5 + i)).ToList();
            var allPay = _context.Payments.ToList();
            var allBkn = _context.Bookings.ToList();
            ViewBag.ChartMonths   = months.Select(m => m.ToString("MMM")).ToArray();
            ViewBag.ChartRevenue  = months.Select(m => allPay.Where(p => p.PaymentDate.Year == m.Year && p.PaymentDate.Month == m.Month).Sum(p => (decimal?)p.Amount) ?? 0).ToArray();
            ViewBag.ChartBookings = months.Select(m => allBkn.Count(b => b.BookingDate.Year == m.Year && b.BookingDate.Month == m.Month)).ToArray();

            // Category Distribution (Mocking based on description keywords or just static counts for now)
            var trips = _context.Trips.ToList();
            ViewBag.CategoryLabels = new[] { "Adventure", "City", "Cultural", "Beach" };
            ViewBag.CategoryData   = new[] {
                trips.Count(t => (t.Description??"").Contains("Adventure")),
                trips.Count(t => (t.Description??"").Contains("City")),
                trips.Count(t => (t.Description??"").Contains("Cultural")),
                trips.Count(t => (t.Description??"").Contains("Beach"))
            };
            // Ensure at least some data if descriptions are empty
            if (ViewBag.CategoryData[0] == 0 && ViewBag.CategoryData[1] == 0) ViewBag.CategoryData = new[] { 5, 8, 4, 3 };

            return View(recentBookings);
        }

        // USERS
        public IActionResult Users(string search, string role, string status, int page = 1)
        {
            ViewData["ActivePage"] = "Users";
            SetNavbarData();

            var query = _context.Users.AsQueryable();

            // Include Phone in search
            if (!string.IsNullOrEmpty(search))
            {
                var l = search.ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(l) ||
                    u.Email.ToLower().Contains(l) ||
                    (u.Phone != null && u.Phone.ToLower().Contains(l)) // ADDED
                );
            }

            if (!string.IsNullOrEmpty(role) && role != "All Roles")
                query = query.Where(u => u.Role == role);

            if (!string.IsNullOrEmpty(status) && status != "All Status")
                query = query.Where(u => (u.Status ?? "Active") == status);

            int pageSize = 8;
            int total = query.Count();

            var users = query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    Phone = u.Phone, // already correct
                    CreatedAt = u.CreatedAt,
                    Status = string.IsNullOrEmpty(u.Status) ? "Active" : u.Status
                })
                .ToList();

            var all = _context.Users.ToList();

            ViewBag.TotalUsers = all.Count;
            ViewBag.ActiveUsers = all.Count(u => (u.Status ?? "Active") == "Active");
            ViewBag.RegisteredUsers = all.Count(u => u.Role == "User");
            ViewBag.NewThisMonth = all.Count(u =>
                u.CreatedAt.Month == DateTime.Now.Month &&
                u.CreatedAt.Year == DateTime.Now.Year
            );

            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.CurrentPage = page;

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var u = _context.Users.Find(id);
            if (u != null)
            {
                _context.Users.Remove(u);
                _context.SaveChanges();
                TempData["Success"] = "User deleted.";
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleUserStatus(int id)
        {
            var u = _context.Users.Find(id);
            if (u != null)
            {
                u.Status = (u.Status == "Active") ? "Inactive" : "Active";
                _context.SaveChanges();
                TempData["Success"] = $"Status changed to {u.Status}.";
            }
            return RedirectToAction("Users");
        }

        public IActionResult AddUser()
        {
            ViewData["ActivePage"] = "Users";
            SetNavbarData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(AddUserViewModel model)
        {
            ViewData["ActivePage"] = "Users";
            SetNavbarData();

            if (!ModelState.IsValid)
                return View(model);

            // Email check
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            // Phone duplicate check (optional but recommended)
            if (_context.Users.Any(u => u.Phone == model.Phone))
            {
                ModelState.AddModelError("Phone", "Phone number already exists.");
                return View(model);
            }

            var user = new User
            {
                Name = model.FullName,
                Email = model.Email,
                Phone = model.Phone,   
                Role = model.Role,
                Status = model.Status,
                CreatedAt = DateTime.Now
            };

            var h = new PasswordHasher<User>();
            user.PasswordHash = h.HashPassword(user, model.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "User added!";
            return RedirectToAction("Users");
        }
        /*// USERS
        public IActionResult Users(string search, string role, string status, int page = 1)
        {
            ViewData["ActivePage"] = "Users"; SetNavbarData();
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search)) { var l = search.ToLower(); query = query.Where(u => u.Name.ToLower().Contains(l) || u.Email.ToLower().Contains(l)); }
            if (!string.IsNullOrEmpty(role) && role != "All Roles") query = query.Where(u => u.Role == role);
            if (!string.IsNullOrEmpty(status) && status != "All Status") query = query.Where(u => (u.Status ?? "Active") == status);
            int pageSize = 8; int total = query.Count();
            var users = query.OrderByDescending(u => u.CreatedAt).Skip((page-1)*pageSize).Take(pageSize)
                .Select(u => new UserViewModel { Id=u.Id,Name=u.Name,Email=u.Email,Role=u.Role,Phone=u.Phone,CreatedAt=u.CreatedAt,Status=string.IsNullOrEmpty(u.Status)?"Active":u.Status }).ToList();
            var all = _context.Users.ToList();
            ViewBag.TotalUsers=all.Count; ViewBag.ActiveUsers=all.Count(u=>(u.Status??"Active")=="Active");
            ViewBag.RegisteredUsers=all.Count(u=>u.Role=="User"); ViewBag.NewThisMonth=all.Count(u=>u.CreatedAt.Month==DateTime.Now.Month&&u.CreatedAt.Year==DateTime.Now.Year);
            ViewBag.TotalPages=(int)Math.Ceiling((double)total/pageSize); ViewBag.CurrentPage=page;
            return View(users);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id) {
            var u = _context.Users.Find(id); if(u!=null){_context.Users.Remove(u);_context.SaveChanges();TempData["Success"]="User deleted.";}
            return RedirectToAction("Users");
        }

        [HttpPost][ValidateAntiForgeryToken]
        public IActionResult ToggleUserStatus(int id) {
            var u = _context.Users.Find(id); if(u!=null){u.Status=(u.Status=="Active")?"Inactive":"Active";_context.SaveChanges();TempData["Success"]=$"Status changed to {u.Status}.";}
            return RedirectToAction("Users");
        }

        public IActionResult AddUser() { ViewData["ActivePage"]="Users"; SetNavbarData(); return View(); }

        [HttpPost][ValidateAntiForgeryToken]
        public IActionResult AddUser(AddUserViewModel model) {
            ViewData["ActivePage"]="Users"; SetNavbarData();
            if(!ModelState.IsValid) return View(model);
            if(_context.Users.Any(u=>u.Email==model.Email)){ModelState.AddModelError("Email","Email already exists.");return View(model);}
            var user=new User{Name=model.FullName,Email=model.Email,Role=model.Role,Status=model.Status,CreatedAt=DateTime.Now};
            var h=new PasswordHasher<User>(); user.PasswordHash=h.HashPassword(user,model.Password);
            _context.Users.Add(user); _context.SaveChanges(); TempData["Success"]="User added!";
            return RedirectToAction("Users");
        }*/

        // TRIPS
        public IActionResult Trips(string search, string status, int page = 1)
        {
            ViewData["ActivePage"] = "Trips"; SetNavbarData();
            var query = _context.Trips.Include(t=>t.Reviews).Include(t=>t.Bookings).AsQueryable();
            if(!string.IsNullOrEmpty(search)){var l=search.ToLower();query=query.Where(t=>t.Title.ToLower().Contains(l)||t.Destination.ToLower().Contains(l));}
            if(!string.IsNullOrEmpty(status)&&status!="All Status") query=query.Where(t=>t.Status==status);
            int pageSize=8; int total=query.Count();
            var trips=query.OrderByDescending(t=>t.CreatedAt).Skip((page-1)*pageSize).Take(pageSize).ToList();
            var allR=_context.Reviews.ToList();
            ViewBag.TotalTrips=_context.Trips.Count(); ViewBag.ActiveTrips=_context.Trips.Count(t=>t.Status=="Active");
            ViewBag.InactiveTrips=_context.Trips.Count(t=>t.Status=="Inactive");
            ViewBag.AvgRating=allR.Any()?allR.Average(r=>r.Rating).ToString("0.1"):"0.0";
            ViewBag.TotalPages=(int)Math.Ceiling((double)total/pageSize); ViewBag.CurrentPage=page;
            return View(trips);
        }

        public IActionResult AddTrip()
        {
            ViewData["ActivePage"] = "Trips";
            SetNavbarData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTrip(Trip model)
        {
            ViewData["ActivePage"] = "Trips";
            SetNavbarData();

            if (!ModelState.IsValid)
                return View(model);

            // Validate date range
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
                return View(model);
            }

            /*// OPTIONAL: auto-calculate duration (if you still keep DurationDays in DB)
            model.DurationDays = (model.EndDate - model.StartDate).Days;*/

            // Set created date
            model.CreatedAt = DateTime.Now;

            _context.Trips.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Trip added!";
            return RedirectToAction("Trips");
        }

        [HttpGet]
        public IActionResult EditTrip(int id)
        {
            ViewData["ActivePage"] = "Trips";
            SetNavbarData();

            var t = _context.Trips.Find(id);
            if (t == null) return NotFound();

            return View(t);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTrip(Trip model)
        {
            ViewData["ActivePage"] = "Trips";
            SetNavbarData();

            if (!ModelState.IsValid)
                return View(model);

            // ? Validate dates
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
                return View(model);
            }

            var t = _context.Trips.Find(model.Id);
            if (t == null) return NotFound();

            //  Update fields
            t.Title = model.Title;
            t.Destination = model.Destination;
            t.Description = model.Description;
            t.Price = model.Price;
            t.StartDate = model.StartDate;  
            t.EndDate = model.EndDate;       
            t.Status = model.Status;
            t.ImageUrl = model.ImageUrl;

            /*// ? OPTIONAL: if DurationDays still exists in DB
            t.DurationDays = (model.EndDate - model.StartDate).Days + 1;*/

            _context.SaveChanges();

            TempData["Success"] = "Trip updated!";
            return RedirectToAction("Trips");
        }

        [HttpPost][ValidateAntiForgeryToken]
        public IActionResult DeleteTrip(int id) {
            var t=_context.Trips.Find(id); if(t!=null){_context.Trips.Remove(t);_context.SaveChanges();TempData["Success"]="Trip deleted.";}
            return RedirectToAction("Trips");
        }

        // BOOKINGS
        public IActionResult Bookings(string search, string status, int page = 1)
        {
            ViewData["ActivePage"] = "Bookings"; SetNavbarData();
            var query = _context.Bookings.Include(b=>b.User).Include(b=>b.Trip).Include(b=>b.Payment).AsQueryable();
            if(!string.IsNullOrEmpty(search)){var l=search.ToLower();query=query.Where(b=>b.User.Name.ToLower().Contains(l)||b.Trip.Title.ToLower().Contains(l));}
            if(!string.IsNullOrEmpty(status)&&status!="All Status") query=query.Where(b=>b.Status==status);
            int pageSize=8; int total=query.Count();
            var bookings=query.OrderByDescending(b=>b.BookingDate).Skip((page-1)*pageSize).Take(pageSize)
                .Select(b=>new BookingViewModel{Id=b.Id,CustomerName=b.User.Name,TripName=b.Trip.Title,Amount=b.TotalAmount,Status=b.Status??"Pending",PaymentStatus=b.Payment!=null?b.Payment.Status:"Pending",BookingDate=b.BookingDate}).ToList();
            var allB=_context.Bookings.ToList();
            ViewBag.TotalBookings=allB.Count; ViewBag.ConfirmedCount=allB.Count(b=>b.Status=="Confirmed");
            ViewBag.PendingCount=allB.Count(b=>b.Status=="Pending"); ViewBag.CancelledCount=allB.Count(b=>b.Status=="Cancelled");
            ViewBag.TotalRevenue=allB.Sum(b=>(decimal?)b.TotalAmount)??0;
            ViewBag.TotalPages=(int)Math.Ceiling((double)total/pageSize); ViewBag.CurrentPage=page;
            return View(bookings);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public IActionResult UpdateBookingStatus(int id, string status) {
            var b=_context.Bookings.Find(id); if(b!=null){b.Status=status;_context.SaveChanges();TempData["Success"]=$"Booking #{id} updated to {status}.";}
            return RedirectToAction("Bookings");
        }

        [HttpPost][ValidateAntiForgeryToken]
        public IActionResult DeleteBooking(int id) {
            var b=_context.Bookings.Include(x=>x.Payment).FirstOrDefault(x=>x.Id==id);
            if(b!=null){if(b.Payment!=null)_context.Payments.Remove(b.Payment);_context.Bookings.Remove(b);_context.SaveChanges();TempData["Success"]="Booking deleted.";}
            return RedirectToAction("Bookings");
        }

        /*// PAYMENTS
        public IActionResult Payments(string search, string status, int page = 1)
        {
            ViewData["ActivePage"] = "Payments"; SetNavbarData();
            int pageSize=8;
            var query=_context.Payments.Include(p=>p.Booking).ThenInclude(b=>b.User).Include(p=>p.Booking).ThenInclude(b=>b.Trip).AsQueryable();
            if(!string.IsNullOrEmpty(search)){var l=search.ToLower();query=query.Where(p=>p.Booking.User.Name.ToLower().Contains(l)||p.Booking.Trip.Title.ToLower().Contains(l));}
            if(!string.IsNullOrEmpty(status)&&status!="All Status") query=query.Where(p=>p.Status==status);
            int total=query.Count();
            var payments=query.OrderByDescending(p=>p.PaymentDate).Skip((page-1)*pageSize).Take(pageSize)
                .Select(p=>new PaymentViewModel{Id=p.Id,TransactionId="TXN-"+p.Id.ToString("D5"),CustomerName=p.Booking.User.Name,TripName=p.Booking.Trip.Title,Amount=p.Amount,Method=p.PaymentMethod,DateTime=p.PaymentDate,Status=p.Status}).ToList();
            ViewBag.TotalPages=(int)Math.Ceiling((double)total/pageSize); ViewBag.CurrentPage=page;
            var allP=_context.Payments.ToList();
            ViewBag.TotalRevenue=allP.Sum(p=>p.Amount).ToString("N0"); ViewBag.TotalTransactions=allP.Count;
            ViewBag.PendingPayments=allP.Where(p=>p.Status=="Pending").Sum(p=>(decimal?)p.Amount)??0;
            ViewBag.Refunds=allP.Where(p=>p.Status=="Failed").Sum(p=>(decimal?)p.Amount)??0;
            var now=DateTime.Now; var months=Enumerable.Range(0,6).Select(i=>now.AddMonths(-5+i)).ToList();
            ViewBag.Months=months.Select(m=>m.ToString("MMM")).ToArray();
            ViewBag.RevenueData=months.Select(m=>allP.Where(p=>p.PaymentDate.Year==m.Year&&p.PaymentDate.Month==m.Month).Sum(p=>(decimal?)p.Amount)??0).ToArray();
            return View(payments);
        }*/

        // PAYMENTS
        public IActionResult Payments(string search, string status, int page = 1)
        {
            ViewData["ActivePage"] = "Payments"; SetNavbarData();
            int pageSize = 8;
            var query = _context.Payments.Include(p => p.Booking).ThenInclude(b => b.User).Include(p => p.Booking).ThenInclude(b => b.Trip).AsQueryable();
            if (!string.IsNullOrEmpty(search)) { var l = search.ToLower(); query = query.Where(p => p.Booking.User.Name.ToLower().Contains(l) || p.Booking.Trip.Title.ToLower().Contains(l)); }
            if (!string.IsNullOrEmpty(status) && status != "All Status") query = query.Where(p => p.Status == status);
            int total = query.Count();
            var payments = query.OrderByDescending(p => p.PaymentDate).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new PaymentViewModel { Id = p.Id, TransactionId = "TXN-" + p.Id.ToString("D5"), CustomerName = p.Booking.User.Name, TripName = p.Booking.Trip.Title, Amount = p.Amount, Method = p.PaymentMethod, DateTime = p.PaymentDate, Status = p.Status }).ToList();
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize); ViewBag.CurrentPage = page;
            var allP = _context.Payments.ToList();
            ViewBag.TotalRevenue = allP.Sum(p => p.Amount).ToString("N0"); ViewBag.TotalTransactions = allP.Count;
            ViewBag.PendingPayments = allP.Where(p => p.Status == "Pending").Sum(p => (decimal?)p.Amount) ?? 0;
            ViewBag.Refunds = allP.Where(p => p.Status == "Failed").Sum(p => (decimal?)p.Amount) ?? 0;
            var now = DateTime.Now; var months = Enumerable.Range(0, 6).Select(i => now.AddMonths(-5 + i)).ToList();
            ViewBag.Months = months.Select(m => m.ToString("MMM")).ToArray();
            ViewBag.RevenueData = months.Select(m => allP.Where(p => p.PaymentDate.Year == m.Year && p.PaymentDate.Month == m.Month).Sum(p => (decimal?)p.Amount) ?? 0).ToArray();
            return View(payments);
        }

        /*// New: return JSON for AJAX
        [HttpGet]
        public IActionResult PaymentsData(string search, string status, int page = 1)
        {
            int pageSize = 8;
            var query = _context.Payments.Include(p => p.Booking).ThenInclude(b => b.User).Include(p => p.Booking).ThenInclude(b => b.Trip).AsQueryable();
            if (!string.IsNullOrEmpty(search)) { var l = search.ToLower(); query = query.Where(p => p.Booking.User.Name.ToLower().Contains(l) || p.Booking.Trip.Title.ToLower().Contains(l)); }
            if (!string.IsNullOrEmpty(status) && status != "All Status") query = query.Where(p => p.Status == status);

            int total = query.Count();
            var items = query.OrderByDescending(p => p.PaymentDate).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new {
                    p.Id,
                    TransactionId = "TXN-" + p.Id.ToString("D5"),
                    CustomerName = p.Booking.User.Name,
                    TripName = p.Booking.Trip.Title,
                    Amount = p.Amount,
                    Method = p.PaymentMethod,
                    DateTime = p.PaymentDate,
                    Status = p.Status
                }).ToList();

            var allP = _context.Payments.ToList();
            var now = DateTime.Now; var months = Enumerable.Range(0, 6).Select(i => now.AddMonths(-5 + i)).ToList();
            var monthsLabels = months.Select(m => m.ToString("MMM")).ToArray();
            var revenueData = months.Select(m => allP.Where(p => p.PaymentDate.Year == m.Year && p.PaymentDate.Month == m.Month).Sum(p => (decimal?)p.Amount) ?? 0).ToArray();

            var result = new {
                items,
                paging = new { totalPages = (int)Math.Ceiling((double)total / pageSize), currentPage = page },
                stats = new {
                    totalRevenue = allP.Sum(p => p.Amount),
                    totalTransactions = allP.Count,
                    pending = allP.Where(p => p.Status == "Pending").Sum(p => (decimal?)p.Amount) ?? 0,
                    refunds = allP.Where(p => p.Status == "Failed").Sum(p => (decimal?)p.Amount) ?? 0
                },
                chart = new { months = monthsLabels, revenue = revenueData }
            };

            return Json(result);
        }*/

        // New: return JSON for AJAX
        [HttpGet]
        public IActionResult PaymentsData(string search, string status, int page = 1)
        {
            int pageSize = 8;
            var query = _context.Payments.Include(p => p.Booking).ThenInclude(b => b.User).Include(p => p.Booking).ThenInclude(b => b.Trip).AsQueryable();
            if (!string.IsNullOrEmpty(search)) { var l = search.ToLower(); query = query.Where(p => p.Booking.User.Name.ToLower().Contains(l) || p.Booking.Trip.Title.ToLower().Contains(l)); }
            if (!string.IsNullOrEmpty(status) && status != "All Status") query = query.Where(p => p.Status == status);

            int total = query.Count();
            var items = query.OrderByDescending(p => p.PaymentDate).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new {
                    Id = p.Id,
                    TransactionId = "TXN-" + p.Id.ToString("D5"),
                    CustomerName = p.Booking != null && p.Booking.User != null ? p.Booking.User.Name : "-",
                    TripName = p.Booking != null && p.Booking.Trip != null ? p.Booking.Trip.Title : "-",
                    Amount = p.Amount,
                    Method = string.IsNullOrEmpty(p.PaymentMethod) ? "-" : p.PaymentMethod,
                    // serialize date as ISO 8601 so client JS new Date(...) works reliably
                    DateTime = p.PaymentDate != DateTime.MinValue ? p.PaymentDate.ToString("o") : null,
                    Status = string.IsNullOrEmpty(p.Status) ? "-" : p.Status
                }).ToList();

            var allP = _context.Payments.ToList();
            var now = DateTime.Now; var months = Enumerable.Range(0, 6).Select(i => now.AddMonths(-5 + i)).ToList();
            var monthsLabels = months.Select(m => m.ToString("MMM")).ToArray();
            var revenueData = months.Select(m => allP.Where(p => p.PaymentDate.Year == m.Year && p.PaymentDate.Month == m.Month).Sum(p => (decimal?)p.Amount) ?? 0).ToArray();

            var result = new
            {
                items,
                paging = new { totalPages = (int)Math.Ceiling((double)total / pageSize), currentPage = page },
                stats = new
                {
                    totalRevenue = allP.Sum(p => p.Amount),
                    totalTransactions = allP.Count,
                    pending = allP.Where(p => p.Status == "Pending").Sum(p => (decimal?)p.Amount) ?? 0,
                    refunds = allP.Where(p => p.Status == "Failed").Sum(p => (decimal?)p.Amount) ?? 0
                },
                chart = new { months = monthsLabels, revenue = revenueData }
            };

            return Json(result);
        }


        // REVIEWS
        public IActionResult Reviews(string search, string status, int page = 1)
        {
            ViewData["ActivePage"] = "Reviews"; SetNavbarData();
            var query=_context.Reviews.Include(r=>r.User).Include(r=>r.Trip).AsQueryable();
            if(!string.IsNullOrEmpty(search)) query=query.Where(r=>r.User.Name.Contains(search)||r.Trip.Title.Contains(search)||r.Comment.Contains(search));
            if(!string.IsNullOrEmpty(status)&&status!="All Status") query=query.Where(r=>r.Status==status);
            int pageSize=8; int total=query.Count();
            var reviews=query.OrderByDescending(r=>r.CreatedAt).Skip((page-1)*pageSize).Take(pageSize).ToList();
            ViewBag.TotalReviews=_context.Reviews.Count();
            ViewBag.AvgRating=_context.Reviews.Any()?_context.Reviews.Average(r=>r.Rating).ToString("0.0"):"0.0";
            ViewBag.PendingReviews=_context.Reviews.Count(r=>r.Status=="Pending");
            ViewBag.FlaggedReviews=_context.Reviews.Count(r=>r.Status=="Flagged");
            ViewBag.TotalPages=(int)Math.Ceiling((double)total/pageSize); ViewBag.CurrentPage=page;
            return View(reviews);
        }

        [HttpPost] public IActionResult ApproveReview(int id){var r=_context.Reviews.Find(id);if(r!=null){r.Status="Approved";_context.SaveChanges();TempData["Success"]="Review approved.";}return RedirectToAction("Reviews");}
        [HttpPost] public IActionResult FlagReview(int id){var r=_context.Reviews.Find(id);if(r!=null){r.Status="Flagged";_context.SaveChanges();TempData["Success"]="Review flagged.";}return RedirectToAction("Reviews");}
        [HttpPost] public IActionResult DeleteReview(int id){var r=_context.Reviews.Find(id);if(r!=null){_context.Reviews.Remove(r);_context.SaveChanges();TempData["Success"]="Review deleted.";}return RedirectToAction("Reviews");}

        [HttpPost]
        public IActionResult ReplyToReview(int id, string reply)
        {
            var r = _context.Reviews.Find(id);
            if (r != null && !string.IsNullOrWhiteSpace(reply))
            {
                r.AdminReply = reply;
                r.ReplyDate = DateTime.Now;
                r.Status = "Approved"; // Automatically approve if replied? Or keep status.
                _context.SaveChanges();
                TempData["Success"] = "Reply sent to review.";
            }
            return RedirectToAction("Reviews");
        }

        // SETTINGS
        public IActionResult Settings()
        {
            ViewData["ActivePage"]="Settings"; SetNavbarData();
            var s=_context.Settings.FirstOrDefault(); if(s==null){s=new Setting();_context.Settings.Add(s);_context.SaveChanges();}
            var vm=new SettingsViewModel
            {
                SiteName=s.SiteName, SiteDescription=s.SiteDescription, ContactEmail=s.ContactEmail, Phone=s.Phone,
                EmailNotifications=s.EmailNotifications, NewUserRegistration=s.NewUserRegistration, BookingConfirmations=s.BookingConfirmations,
                ReviewNotifications=s.ReviewNotifications, PaymentAlerts=s.PaymentAlerts, SMTPHost=s.SMTPHost, SMTPPort=s.SMTPPort,
                Encryption=s.Encryption, SMTPUsername=s.SMTPUsername, SMTPPassword=s.SMTPPassword, TwoFactorAuth=s.TwoFactorAuth,
                LoginAlerts=s.LoginAlerts, Currency=s.Currency, CurrencySymbol=s.CurrencySymbol, ProcessingFee=s.ProcessingFee, FixedFee=s.FixedFee
            };
            return View(vm);
        }
        [HttpPost] public IActionResult SaveGeneralSettings(SettingsViewModel vm){var s=_context.Settings.FirstOrDefault();if(s==null)return RedirectToAction("Settings");s.SiteName=vm.SiteName;s.SiteDescription=vm.SiteDescription;s.ContactEmail=vm.ContactEmail;s.Phone=vm.Phone;_context.SaveChanges();TempData["Success"]="General settings saved!";return RedirectToAction("Settings");}
        [HttpPost] public IActionResult SaveNotificationSettings(SettingsViewModel vm){var s=_context.Settings.FirstOrDefault();if(s==null)return RedirectToAction("Settings");s.EmailNotifications=vm.EmailNotifications;s.NewUserRegistration=vm.NewUserRegistration;s.BookingConfirmations=vm.BookingConfirmations;s.ReviewNotifications=vm.ReviewNotifications;s.PaymentAlerts=vm.PaymentAlerts;_context.SaveChanges();TempData["Success"]="Notification settings saved!";return RedirectToAction("Settings");}
        [HttpPost] public IActionResult SaveEmailSettings(SettingsViewModel vm){var s=_context.Settings.FirstOrDefault();if(s==null)return RedirectToAction("Settings");s.SMTPHost=vm.SMTPHost;s.SMTPPort=vm.SMTPPort;s.Encryption=vm.Encryption;s.SMTPUsername=vm.SMTPUsername;s.SMTPPassword=vm.SMTPPassword;_context.SaveChanges();TempData["Success"]="Email settings saved!";return RedirectToAction("Settings");}
        [HttpPost] public IActionResult SaveSecuritySettings(SettingsViewModel vm){var s=_context.Settings.FirstOrDefault();if(s==null)return RedirectToAction("Settings");s.TwoFactorAuth=vm.TwoFactorAuth;s.LoginAlerts=vm.LoginAlerts;_context.SaveChanges();TempData["Success"]="Security settings saved!";return RedirectToAction("Settings");}
        [HttpPost] public IActionResult SavePaymentSettings(SettingsViewModel vm){var s=_context.Settings.FirstOrDefault();if(s==null)return RedirectToAction("Settings");s.Currency=vm.Currency;s.CurrencySymbol=vm.CurrencySymbol;s.ProcessingFee=vm.ProcessingFee;s.FixedFee=vm.FixedFee;_context.SaveChanges();TempData["Success"]="Payment settings saved!";return RedirectToAction("Settings");}

        // PROFILE
        public IActionResult Profile()
        {
            ViewData["ActivePage"]="Settings"; SetNavbarData();
            var user=GetCurrentAdminUser()??new User{Name="Admin User",Email="admin@travel.com",Role="Admin",CreatedAt=DateTime.Now};
            return View(user);
        }
        [HttpPost][ValidateAntiForgeryToken]
        public IActionResult Profile(User model){if(!ModelState.IsValid)return View(model);var u=_context.Users.Find(model.Id);if(u==null)return NotFound();u.Name=model.Name;u.Phone=model.Phone;_context.SaveChanges();TempData["ProfileSaved"]="Profile updated!";return RedirectToAction("Profile");}

        // CHANGE PASSWORD
        public IActionResult ChangePassword(){ViewData["ActivePage"]="Settings";SetNavbarData();return View();}
        [HttpPost][ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            ViewData["ActivePage"]="Settings"; SetNavbarData();
            if(string.IsNullOrEmpty(currentPassword)||string.IsNullOrEmpty(newPassword)||string.IsNullOrEmpty(confirmPassword)){ModelState.AddModelError("","All fields are required.");return View();}
            if(newPassword!=confirmPassword){ModelState.AddModelError("","New passwords do not match.");return View();}
            if(newPassword.Length<6){ModelState.AddModelError("","Password must be at least 6 characters.");return View();}
            var user=GetCurrentAdminUser();
            if(user!=null){var h=new PasswordHasher<User>();var res=h.VerifyHashedPassword(user,user.PasswordHash,currentPassword);if(res==PasswordVerificationResult.Failed){ModelState.AddModelError("","Current password is incorrect.");return View();}user.PasswordHash=h.HashPassword(user,newPassword);_context.SaveChanges();}
            TempData["PasswordChanged"]="Password updated successfully!";return RedirectToAction("ChangePassword");
        }

        // NOTIFICATIONS
        public IActionResult Notifications()
        {
            ViewData["ActivePage"]="Notifications"; SetNavbarData();
            ViewBag.PendingBookings=_context.Bookings.Include(b=>b.User).Include(b=>b.Trip).Where(b=>b.Status=="Pending").OrderByDescending(b=>b.BookingDate).Take(5).ToList();
            ViewBag.PendingReviews=_context.Reviews.Include(r=>r.User).Include(r=>r.Trip).Where(r=>r.Status=="Pending").OrderByDescending(r=>r.CreatedAt).Take(5).ToList();
            ViewBag.NewUsers=_context.Users.OrderByDescending(u=>u.CreatedAt).Take(5).ToList();
            return View();
        }

        // SEARCH
        public IActionResult Search(string query)
        {
            ViewData["ActivePage"]="Dashboard"; SetNavbarData();
            ViewBag.SearchQuery=query;
            if(string.IsNullOrWhiteSpace(query)){ViewBag.UserResults=new List<User>();ViewBag.TripResults=new List<Trip>();ViewBag.BookingResults=new List<BookingViewModel>();return View();}
            var q=query.ToLower();
            ViewBag.UserResults=_context.Users.Where(u=>u.Name.ToLower().Contains(q)||u.Email.ToLower().Contains(q)).Take(5).ToList();
            ViewBag.TripResults=_context.Trips.Where(t=>t.Title.ToLower().Contains(q)||t.Destination.ToLower().Contains(q)).Take(5).ToList();
            ViewBag.BookingResults=_context.Bookings.Include(b=>b.User).Include(b=>b.Trip).Where(b=>b.User.Name.ToLower().Contains(q)||b.Trip.Title.ToLower().Contains(q)).Take(5).Select(b=>new BookingViewModel{Id=b.Id,CustomerName=b.User.Name,TripName=b.Trip.Title,Amount=b.TotalAmount,Status=b.Status??"Pending",BookingDate=b.BookingDate}).ToList();
            return View();
        }

        // SEARCH ANALYTICS
        public IActionResult SearchAnalytics()
        {
            ViewData["ActivePage"]="SearchAnalytics"; SetNavbarData();
            ViewBag.TopDestinations=_context.Trips.Include(t=>t.Bookings).OrderByDescending(t=>t.Bookings.Count).Take(5).Select(t=>new{t.Destination,t.Title,Count=t.Bookings.Count}).ToList();
            return View();
        }
    }
}
