using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Diskussionsforum.Models;
using Diskussionsforum.Data;
using Diskussionsforum.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Diskussionsforum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentsApiController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CommentDto>> PostComment(CommentCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var postExists = await _context.Posts.AnyAsync(p => p.Id == dto.PostId);
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

            if (!postExists || !userExists)
            {
                return BadRequest("Ogiltigt PostId eller UserId.");
            }

            var comment = new Comment
            {
                Content = dto.Content,
                PostId = dto.PostId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var createdComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            if (createdComment == null)
                return StatusCode(500, "Failed to retrieve created comment.");

            var commentDto = new CommentDto
            {
                Id = createdComment.Id,
                Content = createdComment.Content,
                CreatedAt = createdComment.CreatedAt,
                UserName = createdComment.User?.UserName ?? "Unknown",
                UserProfilePictureUrl = createdComment.User?.ProfilePictureUrl
            };

            return CreatedAtAction(nameof(PostComment), new { id = commentDto.Id }, commentDto);
        }
    }
}
