using Company_Software_Project_Documentation.Data;
using Company_Software_Project_Documentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            SetAccessRights();
            return View(projects);
        }

        [Authorize(Roles = "Guest,Editor,Admin")]
        [HttpGet]
        public IActionResult Show(int id)
        {
            Project project = _context.Projects.Include(p => p.User)
                .Include("Articles")
                .Include("Articles.User")
                .First(art => art.Id == id);
            
            SetAccessRights();
            return View(project);
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

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Show([FromForm] Project project)
        {
            Project project_local = new Project();

            project_local.UserId = _userManager.GetUserId(User);
            project_local.Id = project.Id;
            project_local.Description = project.Description;
            project_local.DateTime = DateTime.Now;
            project_local.User = _context.Users.Find(project.UserId);

            if (project.Description != null)
            {
                TempData["message"] = "Proiectul a fost adaugat!";
                _context.Projects.Add(project_local);
                _context.SaveChanges();
                return Redirect("/Projects/Show/" +
                                project_local.Id);
            }

            else
            {
                TempData["message"] = "Eroare la adaugarea proiectului!";
                Project localproj =
                    _context.Projects
                        .Where(p => p.Id ==
                                            project.Id).First();

                SetAccessRights();
                return View(localproj);
            }
        }

        // Doar editorii si adminii pot modifica proiecte
        // Adminii pot modifica orice proiect
        // Editorii pot modifica doar proiectele care le apartin

        [Authorize(Roles = "Editor,Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Project project = _context.Projects.Find(id);
            if (project.UserId == _userManager.GetUserId(User) ||
                User.IsInRole("Admin"))
            {
                return View(project);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Project requestProject)
        {
            Project local_project = _context.Projects.Find(id);
            try
            {
                if (ModelState.IsValid)
                {
                    if (local_project.UserId == _userManager.GetUserId(User) ||
                        User.IsInRole("Admin"))
                    {

                        local_project.Title = requestProject.Title;
                        local_project.Description = requestProject.Description;
                        local_project.Id = requestProject.Id;
                        _context.SaveChanges();
                        TempData["message"] = "Proiectul a fost modificat!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine";
                        TempData["messageType"] = "alert-danger";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return View(requestProject);
                }
            }
            catch (DbUpdateException e)
            {
                TempData["message"] = "Eroare la modificarea proiectului! DBUpdateException";
                return View(local_project);
            }
        }

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                Project local_project = _context.Projects.Find(id);
                if (local_project.UserId == _userManager.GetUserId(User) ||
                    User.IsInRole("Admin"))
                {
                    _context.Projects.Remove(local_project);
                    _context.SaveChanges();

                    // Stergem de asemenea, si toate articolele asociate proiectului
                    var articles = _context.Articles.Where(a => a.ProjectId == id);
                    foreach (var article in articles)
                    {
                        _context.Articles.Remove(article);
                    }

                    _context.SaveChanges();

                    TempData["message"] = "Proiectul a fost sters!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa stergeti un proiect care nu va apartine";
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
            Project project = new Project();
            return View(project);
        }

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult New(Project project)
        {
            project.DateTime = DateTime.Now;
            project.UserId = _userManager.GetUserId(User);

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Projects.Add(project);

                    _context.SaveChanges();
                    TempData["message"] = "Proiectul a fost adaugat!";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(project);
                }
            }
            catch (DbUpdateException e)
            {
                TempData["message"] = "Eroare la adaugarea proiectului, DBUpdateException";
               
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllArticles()
        {
            var selectList = new List<SelectListItem>();
            var articles = from art in _context.Articles select art;

            foreach (var article in articles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = article.Id.ToString(),
                    Text =  article.Title.ToString()
                });
            }

            return selectList;
        }
    }

}
