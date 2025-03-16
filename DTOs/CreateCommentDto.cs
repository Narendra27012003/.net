using System.ComponentModel.DataAnnotations;

public class CreateCommentDto
{
    [Required]
    public string Content { get; set; }

    [Required]
    public int BlogPostId { get; set; }
}
