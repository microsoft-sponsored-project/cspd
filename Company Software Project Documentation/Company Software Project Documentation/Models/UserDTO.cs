namespace Company_Software_Project_Documentation.Models
{
    public class UserDTO
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }
        public string RoleId { get; set; }


        public UserDTO() {}

        public UserDTO(ApplicationUser user)
        {
            if (user != null)
            {
                Id = user.Id;
                Email = user.Email;
                UserName = user.UserName;
            }
        }
    }
}
