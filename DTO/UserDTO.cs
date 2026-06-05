using PlanningAPI.Models;

namespace PlanningAPI.DTO
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!; // plain password
        public string Role { get; set; } = null!;
        public int Role_Id { get; set; } 
    }

    public class UpdateUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public static class UserMappingExtensions
    {
        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public static User ToEntity(this CreateUserDto dto, string passwordHash)
        {
            return new User
            {
                Username = dto.Username,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = dto.Role,
                RoleId = dto.Role_Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public class UpdatePasswordDto
        {
            public string CurrentPassword { get; set; } = null!;
            public string NewPassword { get; set; } = null!;
        }

        public class ResetPasswordDto
        {
            public string NewPassword { get; set; } = null!;
        }

        public class LoginDto
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        public class LoginResponseDto
        {
            public string Token { get; set; } = null!;
            public string Username { get; set; } = null!;
            public string Role { get; set; } = null!;
            public int User_Id { get; set; } = 0!;
            public string FullName { get; set; } = null!;
        }

    }
}
