using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    // 🔹 User Registration (Default role: Subscriber)
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterUserDto userDto)
    {
        if (_context.Users.Any(u => u.Email == userDto.Email))
            return BadRequest(new { message = "Email already exists." });

        var newUser = new User
        {
            Username = userDto.Username,
            Email = userDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.PasswordHash),
            Role = "Subscriber"  // Default role
        };

        _context.Users.Add(newUser);
        _context.SaveChanges();

        return Ok(new { message = "User registered successfully." });
    }

    // 🔹 User Login (JWT Authentication)
    [HttpPost("login")]
    public IActionResult Login([FromBody] RegisterUserDto userDto)
    {
        var existingUser = _context.Users.FirstOrDefault(u => u.Email == userDto.Email);
        if (existingUser == null || !BCrypt.Net.BCrypt.Verify(userDto.PasswordHash, existingUser.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        var token = _jwtService.GenerateToken(existingUser);

        return Ok(new
        {
            message = "Login successful.",
            token,
            user = new
            {
                id = existingUser.Id,
                username = existingUser.Username,
                email = existingUser.Email,
                role = existingUser.Role
            }
        });
    }

    // 🔹 Assign Role (Admin Only)
    [HttpPut("assign-role/{userId}")]
    [Authorize(Roles = "Admin")]
    public IActionResult AssignRole(int userId, [FromBody] AssignRoleDto roleDto)
    {
        var user = _context.Users.Find(userId);
        if (user == null) return NotFound(new { message = "User not found." });

        var validRoles = new List<string> { "Subscriber", "Blogger", "Admin" };
        if (!validRoles.Contains(roleDto.Role))
        {
            return BadRequest(new { message = "Invalid role. Allowed roles: Subscriber, Blogger, Admin." });
        }

        user.Role = roleDto.Role;
        _context.SaveChanges();

        return Ok(new { message = $"User role updated to {roleDto.Role}." });
    }

    // 🔹 Delete User (Admin Only)
    [HttpDelete("delete-user/{userId}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteUser(int userId)
    {
        var user = _context.Users.Find(userId);
        if (user == null) return NotFound(new { message = "User not found." });

        if (user.Role == "Admin")
        {
            return BadRequest(new { message = "You cannot delete another admin." });
        }

        _context.Users.Remove(user);
        _context.SaveChanges();

        return Ok(new { message = "User deleted successfully." });
    }

    // 🔹 Reset Password (Only Authenticated Users or Admins)
    [HttpPost("reset-password")]
    [Authorize]
    public IActionResult ResetPassword([FromBody] ResetPasswordDto resetDto)
    {
        var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var loggedInUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var user = _context.Users.FirstOrDefault(u => u.Email == resetDto.Email);
        if (user == null) return NotFound(new { message = "User not found." });

        // 🔹 Only allow users to reset their own password, except Admins
        if (loggedInUserRole != "Admin" && user.Id != loggedInUserId)
        {
            return Forbid("You can only reset your own password.");
        }

        // 🔹 Hash new password before saving
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
        _context.SaveChanges();

        return Ok(new { message = "Password reset successfully." });
    }
}
