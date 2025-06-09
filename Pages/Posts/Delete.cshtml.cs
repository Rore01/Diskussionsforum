using Diskussionsforum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Diskussionsforum.Pages.Posts
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteModel(IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("api");
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }


        [BindProperty]
        public Post Post { get; set; } = null!;

        public bool IsOwnerOrAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var post = await _httpClient.GetFromJsonAsync<Post>($"https://localhost:7178/api/PostsApi/{id}");
            if (post == null)
                return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

            if (post.UserId != currentUserId && !isAdmin)
                return Forbid();

            Post = post;
            IsOwnerOrAdmin = true;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var post = await _httpClient.GetFromJsonAsync<Post>($"https://localhost:7178/api/PostsApi/{id}");
            if (post == null)
                return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

            if (post.UserId != currentUserId && !isAdmin)
                return Forbid();

            var authCookie = _httpContextAccessor.HttpContext?.Request.Cookies[".AspNetCore.Identity.Application"];
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://localhost:7178/api/PostsApi/{id}");
            request.Headers.Add("Cookie", $".AspNetCore.Identity.Application={authCookie}");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Index");
            }

            ModelState.AddModelError(string.Empty, "Kunde inte ta bort inlägget.");
            Post = post;
            return Page();
        }

    }
}
