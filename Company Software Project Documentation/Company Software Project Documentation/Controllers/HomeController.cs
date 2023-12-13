using Company_Software_Project_Documentation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Company_Software_Project_Documentation.Data;
using Company_Software_Project_Documentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Company_Software_Project_Documentation.Areas.Identity.Pages.Account;
using Company_Software_Project_Documentation.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Company_Software_Project_Documentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILogger<RegisterModel> _register_logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;

        public HomeController(
            ILogger<RegisterModel> registerLogger,
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _logger = logger;
            _register_logger = registerLogger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;

            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        public IActionResult Index()
        {
            var articles = _context.Articles
                .Include(a => a.User)
                .Include(a => a.Project)
                .OrderByDescending(a => a.DateTime)
                .Take(5)
                .ToList();

            ViewBag.Articles = articles;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"];
                ViewBag.alert = TempData["messageType"];
            }

            return View(new RegisterModel(_userManager, _userStore, _signInManager, _register_logger, _emailSender));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
