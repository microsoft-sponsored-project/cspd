using Company_Software_Project_Documentation.Models;
using Company_Software_Project_Documentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Company_Software_Project_Documentation.Controllers
{
    public class AdminPageController : Controller
    {
        private readonly UserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminPageController(UserService userService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _userService = userService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index()
        {
            // Example: Get a list of UserDto objects using UserService
            var userDtos = _userService.GetAllUsers();
            // Get the roles for each user
            foreach (var userDto in userDtos)
            {
                userDto.Role = _userService.GetUserRole(userDto.Id);
                userDto.Role ??= "No role";
            }

            // Use userDtos as needed, such as passing them to a view
            return View(userDtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult EditUser(string id)
        {
            // Example: Get a list of UserDto objects using UserService
            var userDto = _userService.GetAllUsers().Find(u => u.Id == id);

            if (userDto != null)
            {
                // Get the roles for each user
                userDto.Role = _userService.GetUserRole(userDto.Id);
                userDto.Role ??= "No role";

                var role = _roleManager.FindByNameAsync(userDto.Role).Result;

                if (role != null)
                {
                    userDto.RoleId = role.Id;
                }
                else
                {
                    userDto.RoleId = "DefaultRoleId";
                }
            }
            else
            {
                return NotFound();
            }

            ViewBag.Roles = GetAllRolesFromDatabase();
            // Use userDtos as needed, such as passing them to a view
            return View(userDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditUser(UserDTO userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id);
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            if (role != null)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            var newRole = await _roleManager.FindByIdAsync(userDto.RoleId);
            if (newRole != null)
            {
                await _userManager.AddToRoleAsync(user, newRole.Name);
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("EditUser", new { id });
            }
        }


        [NonAction]
        public IEnumerable<SelectListItem> GetAllRolesFromDatabase()
        {
            var roles = _roleManager.Roles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = role.Id
            }).ToList();
            return roles;
        }

    }
}
