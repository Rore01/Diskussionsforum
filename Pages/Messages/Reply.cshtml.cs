using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Diskussionsforum.Data;
using Diskussionsforum.Models;

public class ReplyModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReplyModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public string Content { get; set; }

    [BindProperty]
    public string ReceiverId { get; set; }

    public string ReceiverUserName { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var originalMessage = await _context.PrivateMessages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (originalMessage == null)
            return NotFound();

        ReceiverId = originalMessage.SenderId;
        ReceiverUserName = originalMessage.Sender.UserName;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var sender = await _userManager.GetUserAsync(User);

        var message = new PrivateMessage
        {
            SenderId = sender.Id,
            ReceiverId = ReceiverId,
            Content = Content,
            SentAt = DateTime.Now
        };

        _context.PrivateMessages.Add(message);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Meddelandet har skickats!";
        return RedirectToPage("/Messages/Sent");
    }
}
