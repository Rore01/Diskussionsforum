using Diskussionsforum.Data;
using Diskussionsforum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Diskussionsforum.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class PostDetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public PostDetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Post? Post { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.ForumCategory)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Post == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Reports");
        }
    }
}
