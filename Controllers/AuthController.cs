using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        if (_context.Users.Any(u => u.Email == user.Email))
            return BadRequest("Email already exists");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.Role = "Subscriber";
        _context.Users.Add(user);
        _context.SaveChanges();
        return Ok("User registered successfully");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] User user)
    {
        var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
        if (existingUser == null || !BCrypt.Net.BCrypt.Verify(user.PasswordHash, existingUser.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = _jwtService.GenerateToken(existingUser);
        return Ok(new { token });
    }
}
