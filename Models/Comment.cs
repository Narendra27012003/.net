using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Content { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User? User { get; set; } // Allow Nullable

    [ForeignKey("BlogPost")]
    public int BlogPostId { get; set; }
    public BlogPost? BlogPost { get; set; } // Allow Nullable

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ✅ Ensure CreatedAt is present
}
