using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/comments")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly AppDbContext _context;

    public CommentController(AppDbContext context)
    {
        _context = context;
    }

    // 🔹 Add a Comment (Only Subscribers, Bloggers, Admins)
    [HttpPost]
    [Authorize(Roles = "Subscriber,Blogger,Admin")]
    public IActionResult AddComment([FromBody] Comment comment)
    {
        if (comment == null || string.IsNullOrWhiteSpace(comment.Content))
        {
            return BadRequest("Invalid comment data.");
        }

        // 🔹 Extract UserId from JWT Token
        var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        // Validate BlogPostId
        var blogPostExists = _context.BlogPosts.Any(b => b.Id == comment.BlogPostId);
        if (!blogPostExists)
        {
            return BadRequest("Invalid BlogPostId.");
        }

        // Assign the logged-in user's ID to the comment
        comment.UserId = loggedInUserId;

        _context.Comments.Add(comment);
        _context.SaveChanges();
        return Ok("Comment added successfully.");
    }

    // 🔹 Get All Comments for a Blog Post (Public API)
    [HttpGet("{blogPostId}")]
    public IActionResult GetComments(int blogPostId)
    {
        var comments = _context.Comments
            .Include(c => c.User)
            .Where(c => c.BlogPostId == blogPostId)
            .Select(c => new
            {
                c.Id,
                c.Content,
                Author = c.User.Username,
                c.CreatedAt
            }).ToList();

        return Ok(comments);
    }

    // 🔹 Edit a Comment (Only Comment Owner)
    [HttpPut("{id}")]
    [Authorize(Roles = "Subscriber,Blogger,Admin")]
    public IActionResult EditComment(int id, [FromBody] Comment updatedComment)
    {
        var comment = _context.Comments.Find(id);
        if (comment == null) return NotFound("Comment not found.");

        // 🔹 Get logged-in user ID
        var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        if (comment.UserId != loggedInUserId)
        {
            return Forbid("You can only edit your own comments.");
        }

        // Ensure new content is valid
        if (string.IsNullOrWhiteSpace(updatedComment.Content))
        {
            return BadRequest("Comment content cannot be empty.");
        }

        comment.Content = updatedComment.Content;
        _context.SaveChanges();
        return Ok("Comment updated successfully.");
    }

    // 🔹 Delete a Comment (Only Comment Owner or Admin)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Subscriber,Blogger,Admin")]
    public IActionResult DeleteComment(int id)
    {
        var comment = _context.Comments.Find(id);
        if (comment == null) return NotFound("Comment not found.");

        // 🔹 Get logged-in user ID & Role
        var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var loggedInUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Allow only the comment owner or Admin to delete
        if (loggedInUserRole != "Admin" && comment.UserId != loggedInUserId)
        {
            return Forbid("You can only delete your own comments.");
        }

        _context.Comments.Remove(comment);
        _context.SaveChanges();
        return Ok("Comment deleted successfully.");
    }
}
