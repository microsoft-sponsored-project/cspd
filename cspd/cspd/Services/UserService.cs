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
            // Retrieve all users from the Identity framework
            var users = _userManager.Users.ToList();

            // Convert the list of ApplicationUser to a list of UserDto
            var userDtos = users.Select(user => new UserDTO(user)).ToList();

            return userDtos;
        }

        // Get the role for a user
        public string? GetUserRole(string userId)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            var role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();
            return role;
        }

    }
}