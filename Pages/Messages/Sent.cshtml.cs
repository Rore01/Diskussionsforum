using Diskussionsforum.Data;
using Diskussionsforum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Diskussionsforum.Pages.Messages
{
    [Authorize]
    public class SentModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SentModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<PrivateMessageViewModel> Messages { get; set; } = new();

        public async Task OnGetAsync()
        {
            var currentUserId = _userManager.GetUserId(User);

            Messages = await _context.PrivateMessages
                .Where(m => m.SenderId == currentUserId)
                .OrderByDescending(m => m.SentAt)
                .Select(m => new PrivateMessageViewModel
                {
                    Content = m.Content,
                    SentAt = m.SentAt,
                    ReceiverUserName = _context.Users
                        .Where(u => u.Id == m.ReceiverId)
                        .Select(u => u.UserName)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public class PrivateMessageViewModel
        {
            public string ReceiverUserName { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public DateTime SentAt { get; set; }
        }
    }
}
