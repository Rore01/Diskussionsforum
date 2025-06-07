using Diskussionsforum.Data;
using Diskussionsforum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Diskussionsforum.Pages.Messages
{
    [Authorize]
    public class SendModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SendModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public MessageInput Input { get; set; } = new();

        public class MessageInput
        {
            [Required]
            public string ReceiverId { get; set; } = string.Empty;

            [Required]
            [StringLength(1000, MinimumLength = 1)]
            public string Content { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(string receiverId)
        {
            var user = await _userManager.FindByIdAsync(receiverId);
            if (user == null)
            {
                return NotFound();
            }

            Input.ReceiverId = receiverId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var senderId = _userManager.GetUserId(User);
            var receiver = await _userManager.FindByIdAsync(Input.ReceiverId);

            if (receiver == null)
                return NotFound();

            var message = new PrivateMessage
            {
                SenderId = senderId,
                ReceiverId = Input.ReceiverId,
                Content = Input.Content,
                SentAt = DateTime.UtcNow
            };

            _context.PrivateMessages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Meddelande skickat!";
            return RedirectToPage("/Index"); // or wherever you want to redirect
        }
    }
}
