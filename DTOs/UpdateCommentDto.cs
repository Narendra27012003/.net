using System.ComponentModel.DataAnnotations;

public class UpdateCommentDto
{
    [Required]
    public string Content { get; set; }
}
