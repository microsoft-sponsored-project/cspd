using Company_Software_Project_Documentation.Data;
using Company_Software_Project_Documentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ganss.Xss;

namespace Company_Software_Project_Documentation.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ArticlesController(
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
            var articles = new List<Article>();

            if (Convert.ToString(HttpContext.Request.Query["search"]) == null)
            {
                articles = _context.Articles
                    .Include(a => a.User)
                    .Include(a => a.Project)
                    .ToList();

                ViewBag.SearchString = "";
            }
            else
            {
                var search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
               
                List<int> articleIds = _context.Articles
                    .Where(at => at.Title.Contains(search) ||
                        at.Content.Contains(search))
                    .Select(a => a.Id).ToList();

                articles = _context.Articles
                    .Where(a => articleIds.Contains(a.Id))
                    .Include(a => a.User)
                    .Include(a => a.Project)
                    .ToList();

                ViewBag.SearchString = search;
            }

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"];
                ViewBag.alert = TempData["messageType"];
            }

            ViewBag.Articles = articles;
            SetAccessRights();

            return View(articles);
        }

        [Authorize(Roles = "Guest,Editor,Admin")]
        [HttpGet]
        public IActionResult Show(int id)
        {
            var article = _context.Articles
                .Include("User")
                .Include("Project")
                .FirstOrDefault(a => a.Id == id);

            if (article == null)
            {
                TempData["message"] = "Articolul nu există sau nu aveți dreptul să îl vizualizați.";
                TempData["messageType"] = "alert-danger";
                SetAccessRights();
                return RedirectToAction("Index");
            }

            SetAccessRights();
            return View(article);
        }

        // Conditii de afisare a butoanelor de editare si stergere
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
       
        [Authorize(Roles = "Editor,Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Article article = _context.Articles
                .Include(a => a.Project)
                .Include(a => a.User)
                .Where(a => a.Id == id)
                .First();

            if ((!article.IsProtected && article.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                return View(article);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra acestui articol";
                TempData["messageType"] = "alert-danger";
                SetAccessRights();
                return RedirectToAction("Index", "Articles");
            }
        }
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Article requestArticle)
        {
            Article article = _context.Articles.Include(art => art.Project).Include(art => art.User)
                .Where(art => art.Id == id).First();

            var sanitizer = new HtmlSanitizer();

            try
            {
                if (ModelState.IsValid)
                {

                    if ((!article.IsProtected && article.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                    { 
                        article.Title = requestArticle.Title;
                        article.Content = sanitizer.Sanitize(requestArticle.Content);
                        article.DateTime = DateTime.Now; // Actualizam data modificarii in mod dinamic

                        _context.SaveChanges();
                        TempData["message"] = "Articolul a fost modificat!";
                        SetAccessRights();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                        TempData["messageType"] = "alert-danger";
                        SetAccessRights();
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["message"] = "Eroare la editarea articolului";
                    TempData["messageType"] = "alert-danger";
                    return View(requestArticle);
                }
            }
            catch (DbUpdateException e)
            {
                TempData["message"] = "Eroare la modificarea articolului! DBUpdateException";
                SetAccessRights();
                return View(article);
            }
        }

        // Doar editorii si adminii pot sterge articole
        // Adminii pot sterge orice articol
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                Article article = _context.Articles.Find(id);
                if (article.UserId == _userManager.GetUserId(User) ||
                    User.IsInRole("Admin"))
                {
                    _context.Articles.Remove(article);
                    _context.SaveChanges();

                    TempData["message"] = "Articolul a fost sters!";
                    SetAccessRights();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa stergeti un articol care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    SetAccessRights();
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException e)
            {
                TempData["message"] = "Eroare la adaugarea articolului. DBUpdateException";
                SetAccessRights();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                TempData["message"] = "Alta eroare la stergere";
                SetAccessRights();
                return RedirectToAction("Index");
            }
        }

        // Doar editorii si adminii pot adauga articole
        [Authorize(Roles = "Editor,Admin")]
        [HttpGet]
        public IActionResult New()
        {
            Article article = new Article();
            ViewBag.Projects = GetAllProjects();
            return View(article);
        }

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult New(Article article)
        {
            article.DateTime = DateTime.Now;
            // Preluam id-ul userului curent
            article.UserId = _userManager.GetUserId(User);
            article.User = _context.Users.Find(article.UserId);
            article.Project = _context.Projects.Find(article.ProjectId);
            ViewBag.Projects = GetAllProjects();

            var sanitizer = new HtmlSanitizer();

            try
            {
                if (ModelState.IsValid)
                {
                    article.Content = sanitizer.Sanitize(article.Content);

                    _context.Articles.Add(article);
                    _context.SaveChanges();
                    TempData["message"] = "Articolul a fost adaugat!";
                    TempData["messageType"] = "alert-success"; // success, danger, warning, info
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(article);
                }

            }
            catch (DbUpdateException e)
            {
                TempData["message"] = "Eroare la adaugarea articolului, DBUpdateException";
                SetAccessRights();
                return RedirectToAction("Index");
            }
        }

       [NonAction]
        public IEnumerable<SelectListItem> GetAllProjects()
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var selectList = new List<SelectListItem>();

            var projects = _context.Projects
                .Include(proj => proj.User)
                .Where(proj => proj.User.Id == userId || isAdmin)
                .ToList();
            
            foreach (var proj in projects)
            {
                selectList.Add(new SelectListItem
                {
                    Value = proj.Id.ToString(),
                    Text = proj.Title.ToString()
                });
            }

            return selectList;
        }
    }
}
