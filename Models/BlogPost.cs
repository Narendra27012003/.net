using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BlogPost
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }  // Ensure this is used instead of User object

    public User? User { get; set; } // Make it nullable to prevent validation errors
}
