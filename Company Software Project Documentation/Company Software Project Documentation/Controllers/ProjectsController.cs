using Company_Software_Project_Documentation.Data;
using Company_Software_Project_Documentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Company_Software_Project_Documentation.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProjectsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Guest,Editor,Admin")]
        public IActionResult Index()
        {
            var projects = _context.Projects.Include(a => a.User).ToList();
                
            ViewBag.Projects = projects;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"];
                ViewBag.alert = TempData["messageType"];
            }
            return View(projects);
        }


    }
}
