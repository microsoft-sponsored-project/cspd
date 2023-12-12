using Company_Software_Project_Documentation.Data;
using Company_Software_Project_Documentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Index()
        {
            var articles = _context.Articles
                .Include(a => a.User)
                .Include(a => a.Project)
                .ToList();

            ViewBag.Articles = articles;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"];
                ViewBag.alert = TempData["messageType"];
            }
            return View(articles);
        }

        [Authorize(Roles = "User,Editor,Admin")]
        [HttpGet]
        public IActionResult Show(int id)
        {
            Article article = _context.Articles
                .Include("User")
                .Include("Project")
                .Where(art => art.Id == id).First();

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


     /*   // Toate rolurile pot adauga comentarii
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            Comment comm = new Comment();

            comm.UserId = _userManager.GetUserId(User);
            comm.ArticleId = comment.ArticleId;
            comm.content = comment.content;
            comm.Date = DateTime.Now;
            comm.User = _context.Users.Find(comm.UserId);

            if (comm.content != null)
            {
                TempData["message"] = "Comentariul a fost adaugat!";
                _context.Comments.Add(comment);
                _context.SaveChanges();
                return Redirect("/Articles/Show/" +
                                comment.ArticleId);
            }

            else
            {
                TempData["message"] = "Eroare la adaugarea comentariului!";
                Article art =
                    _context.Articles.Include("Category").Include("Comments").Include("Comments").Include("Comments.User")
                        .Where(art => art.Id ==
                                      comment.ArticleId).First();

                SetAccessRights();

                return View(art);
            }
        } */

       
        [Authorize(Roles = "Editor,Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Article article = _context.Articles
                .Include(art => art.Project)
                .Include(art => art.User)
                .Where(art => art.Id == id)
                .First();

            if (article.UserId == _userManager.GetUserId(User) ||
                User.IsInRole("Admin"))
            {
                return View(article);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Article requestArticle)
        {
            Article article = _context.Articles.Include(art => art.Project).Include(art => art.User)
                .Where(art => art.Id == id).First();

            try
            {
                if (ModelState.IsValid)
                {
                    if (article.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                    {
                        article.Title = requestArticle.Title;
                        article.Content = requestArticle.Content;
                        article.DateTime = DateTime.Now; // Actualizam data modificarii in mod dinamic

                        _context.SaveChanges();
                        TempData["message"] = "Articolul a fost modificat!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                        TempData["messageType"] = "alert-danger";
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
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa stergeti un articol care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException e)
            {
                TempData["message"] = "Eroare la adaugarea articolului. DBUpdateException";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                TempData["message"] = "Alta eroare la stergere";
                return RedirectToAction("Index");
            }
        }

        // Doar editorii si adminii pot adauga articole
        [Authorize(Roles = "Editor,Admin")]
        [HttpGet]
        public IActionResult New()
        {
            Article article = new Article();
            article.ProjectId = -1;
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

            try
            {
                if (ModelState.IsValid)
                {
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
