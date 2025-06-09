using Diskussionsforum.Data;
using Diskussionsforum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Diskussionsforum.Pages.Posts
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public PostInputModel Post { get; set; } = new();

        public List<SelectListItem> Categories { get; set; } = new();

        public class PostInputModel
        {
            [Required(ErrorMessage = "Titel krävs")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Innehåll krävs")]
            public string Content { get; set; } = string.Empty;

            [Required(ErrorMessage = "Välj en kategori")]
            public int? ForumCategoryId { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCategoriesAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var post = new Post
            {
                Title = Post.Title,
                Content = Post.Content,
                ForumCategoryId = Post.ForumCategoryId!.Value,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }

        private async Task LoadCategoriesAsync()
        {
            Categories = await _context.ForumCategories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Title
                }).ToListAsync();
        }
    }
}
