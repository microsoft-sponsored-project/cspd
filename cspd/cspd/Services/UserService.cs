using System.Collections.Generic;
using System.Linq;
using Company_Software_Project_Documentation.Models;
using Microsoft.AspNetCore.Identity;

namespace Company_Software_Project_Documentation.Services
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public List<UserDTO> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var userDtos = users.Select(user => new UserDTO(user)).ToList();

            return userDtos;
        }

        public string? GetUserRole(string userId)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            var role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();
            return role;
        }

    }
}