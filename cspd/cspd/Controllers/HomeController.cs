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
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using RegisterModel = Company_Software_Project_Documentation.Areas.Identity.Pages.Account.RegisterModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;

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

        public IActionResult Index()
        {
            var articles = _context.Articles
                .Include(a => a.User)
                .Include(a => a.Project)
                .OrderByDescending(a => a.DateTime)
                .Take(6)
                .ToList();

            ViewBag.Articles = articles;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"];
                ViewBag.alert = TempData["messageType"];
            }
            SetAccessRights();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public void SetAccessRights()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.afisareButoane = true;
            }
            else if (User.IsInRole("Editor"))
            {
                ViewBag.afisareButoane = true;
            }
            else if (User.IsInRole("Guest"))
            {
                ViewBag.afisareButoane = false;
            }
            ViewBag.esteAdmin = User.IsInRole("Admin");
            ViewBag.esteEditor = User.IsInRole("Editor");
            ViewBag.userCurent = _userManager.GetUserId(User);
        }

        ////////////////////////////// Logic for registration page
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(LoginAndRegisterViewModel model)
        {
            if (ModelState["RegisterModel.Email"].Errors.Count == 0 &&
                ModelState["RegisterModel.Password"].Errors.Count == 0 &&
                ModelState["RegisterModel.ConfirmPassword"].Errors.Count == 0)
            
            {
                var user = new ApplicationUser { 
                    UserName = model.RegisterModel.Email, 
                    Email = model.RegisterModel.Email,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, model.RegisterModel.Password);

                if (result.Succeeded) {
                    await _userManager.AddToRoleAsync(user, "Guest");
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else {
                TempData["shortMessage"] = "Inregistrarea a esuat, formular invalid";
                return RedirectToAction("Index", "Home");
            }

            TempData["shortMessage"] = "Datele introduse nu sunt corecte! Inregistrarea a esuat";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginAndRegisterViewModel model)
        {
            if (ModelState["LoginModel.Email"].Errors.Count == 0 
                && ModelState["LoginModel.Password"].Errors.Count == 0
                && ModelState["LoginModel.RememberMe"].Errors.Count == 0)
            {
                var result = await _signInManager.PasswordSignInAsync(model.LoginModel.Email, model.LoginModel.Password, model.LoginModel.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            else {
                TempData["shortMessage"] = "Logarea a esuat, formular invalid";
                return RedirectToAction("Index", "Home");
            }

            TempData["shortMessage"] = "Datele introduse nu sunt corecte! Logarea a esuat";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> OnPostAsync([FromForm] RegisterViewModel regViewModel)
        {
            string returnUrl = Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, regViewModel.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, regViewModel.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, regViewModel.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.AddToRoleAsync(user, "Guest"); // Adaugarea rolului de User la inregistrare

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Identity/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("/Identity/Account/RegisterConfirmation", new { email = regViewModel.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            TempData["message"] = "Datele introduse nu sunt corecte!";
            return RedirectToAction("Index", "Home");
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
