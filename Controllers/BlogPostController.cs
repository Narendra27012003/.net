using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/blogs")]
[ApiController]
public class BlogPostController : ControllerBase
{
    private readonly AppDbContext _context;

    public BlogPostController(AppDbContext context)
    {
        _context = context;
    }

    // 🔹 Create Blog Post (Only Bloggers & Admins)
    [HttpPost]
    [Authorize(Roles = "Blogger,Admin")]
    public IActionResult CreateBlogPost([FromBody] CreateBlogPostDto postDto)
    {
        if (postDto == null || string.IsNullOrWhiteSpace(postDto.Title) || string.IsNullOrWhiteSpace(postDto.Content))
        {
            return BadRequest("Invalid blog post data.");
        }

        // 🔹 Extract UserId from JWT Token (Authenticated User)
        var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        // Create a new blog post instance
        var newPost = new BlogPost
        {
            Title = postDto.Title,
            Content = postDto.Content,
            UserId = loggedInUserId
        };

        _context.BlogPosts.Add(newPost);
        _context.SaveChanges();
        return Ok(new { message = "Blog post created successfully." });
    }

    // 🔹 Get All Blog Posts (Public API - Everyone Can Access)
    [HttpGet]
    public IActionResult GetAllBlogPosts()
    {
        var posts = _context.BlogPosts
            .Include(b => b.User)  // Include User details
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Content,
                Author = b.User.Username  // Display username instead of UserId
            }).ToList();

        return Ok(posts);
    }

    // 🔹 Get Blog Post by ID
    [HttpGet("{id}")]
    public IActionResult GetBlogPostById(int id)
    {
        var post = _context.BlogPosts
            .Include(b => b.User)
            .Where(b => b.Id == id)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Content,
                Author = b.User.Username
            }).FirstOrDefault();

        if (post == null)
            return NotFound("Blog post not found.");

        return Ok(post);
    }

    // 🔹 Delete Blog Post (Only for Blog Owner & Admin)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Blogger,Admin")]
    public IActionResult DeleteBlogPost(int id)
    {
        var post = _context.BlogPosts.Find(id);
        if (post == null) return NotFound("Blog post not found.");

        // Get logged-in user ID & Role
        var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var loggedInUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Bloggers can only delete their own posts; Admins can delete any post
        if (loggedInUserRole != "Admin" && post.UserId != loggedInUserId)
        {
            return Forbid("You can only delete your own blog posts.");
        }

        _context.BlogPosts.Remove(post);
        _context.SaveChanges();
        return Ok(new { message = "Blog post deleted successfully." });
    }

    // 🔹 Update Blog Post (Only for Blog Owner)
    [HttpPut("{id}")]
    [Authorize(Roles = "Blogger,Admin")]
    public IActionResult UpdateBlogPost(int id, [FromBody] CreateBlogPostDto updatedPost)
    {
        var post = _context.BlogPosts.Find(id);
        if (post == null) return NotFound("Blog post not found.");

        // Get logged-in user ID & Role
        var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var loggedInUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Bloggers can only edit their own posts
        if (loggedInUserRole != "Admin" && post.UserId != loggedInUserId)
        {
            return Forbid("You can only edit your own blog posts.");
        }

        // Update Blog Post
        post.Title = updatedPost.Title;
        post.Content = updatedPost.Content;
        _context.SaveChanges();

        return Ok(new { message = "Blog post updated successfully." });
    }
}
