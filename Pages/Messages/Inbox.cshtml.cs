using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Diskussionsforum.Models;
using Diskussionsforum.Data;


public class InboxModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InboxModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<PrivateMessage> Messages { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        Messages = await _context.PrivateMessages
            .Where(m => m.ReceiverId == user.Id)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        return Page();
    }
}
