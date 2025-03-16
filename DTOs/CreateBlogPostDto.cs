using System.ComponentModel.DataAnnotations;

public class CreateBlogPostDto
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }
}
