using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Diskussionsforum.Data;
using Diskussionsforum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Diskussionsforum.DTOs;
using System.Security.Claims;

namespace Diskussionsforum.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostsApiController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.ForumCategory)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var postDtos = posts.Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                UserName = p.User?.UserName ?? "Unknown",
                Email = p.User?.Email ?? "",
                UserProfilePictureUrl = p.User?.ProfilePictureUrl,
                ForumCategoryTitle = p.ForumCategory?.Title ?? string.Empty,
                Comments = p.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserName = c.User?.UserName ?? "Unknown",
                    UserProfilePictureUrl = c.User?.ProfilePictureUrl
                }).ToList()
            }).ToList();

            return Ok(postDtos);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetPostsByCategory(int categoryId)
        {
            var categoryExists = await _context.ForumCategories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                return NotFound(new { message = "Category not found." });
            }

            var posts = await _context.Posts
                .Where(p => p.ForumCategoryId == categoryId)
                .Include(p => p.User)
                .Include(p => p.ForumCategory)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var postDtos = posts.Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                UserName = p.User?.UserName ?? "Unknown",
                Email = p.User?.Email ?? "",
                UserProfilePictureUrl = p.User?.ProfilePictureUrl,
                ForumCategoryTitle = p.ForumCategory?.Title ?? string.Empty,
                Comments = p.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserName = c.User?.UserName ?? "Unknown",
                    UserProfilePictureUrl = c.User?.ProfilePictureUrl
                }).ToList()
            }).ToList();

            return Ok(postDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.ForumCategory)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            var postDto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UserName = post.User?.UserName ?? "Unknown",
                Email = post.User?.Email ?? "unknown@email.com",
                UserProfilePictureUrl = post.User?.ProfilePictureUrl,
                ForumCategoryTitle = post.ForumCategory?.Title ?? "",
                UserId = post.UserId,
                Comments = post.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserName = c.User?.UserName ?? "Unknown",
                    UserProfilePictureUrl = c.User?.ProfilePictureUrl
                }).ToList()
            };

            return Ok(postDto);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] Post post)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            post.UserId = userId;
            post.CreatedAt = DateTime.UtcNow;

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoryExists = await _context.ForumCategories.AnyAsync(c => c.Id == post.ForumCategoryId);
            if (!categoryExists)
                return BadRequest("Ogiltig kategori (ForumCategoryId).");

            _context.Posts.Add(post);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (post.UserId != userId && !isAdmin)
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> AddComment(int postId, Comment comment)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();

            comment.PostId = postId;
            comment.UserId = userId;
            comment.CreatedAt = DateTime.UtcNow;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPost), new { id = postId }, comment);
        }
    }
}
