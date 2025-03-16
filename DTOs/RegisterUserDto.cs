using System.ComponentModel.DataAnnotations;

public class RegisterUserDto
{
    [Required]
    public string Username { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }
}
