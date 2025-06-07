using Diskussionsforum.Data;
using Diskussionsforum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Diskussionsforum.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CategoriesModel : PageModel
    {
        private readonly AppDbContext _context;

        public CategoriesModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ForumCategory NewCategory { get; set; } = new();

        public List<ForumCategory> ExistingCategories { get; set; } = new();

        public async Task OnGetAsync()
        {
            ExistingCategories = await _context.ForumCategories
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(NewCategory.Title))
            {
                await OnGetAsync(); // Reload categories
                return Page();
            }

            _context.ForumCategories.Add(NewCategory);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var category = await _context.ForumCategories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            if (category.Posts.Any())
            {
                ModelState.AddModelError(string.Empty, "Kategorin innehåller inlägg och kan inte tas bort.");
                await OnGetAsync();
                return Page();
            }

            _context.ForumCategories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
