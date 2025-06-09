using Diskussionsforum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Security.Claims;
using Diskussionsforum.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Diskussionsforum.Pages.Posts
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DetailsModel(HttpClient httpClient, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public CommentCreateDto NewComment { get; set; } = new();

        public PostDto Post { get; set; } = null!;
        public bool IsOwnPost { get; set; }

        public bool IsOwnerOrAdmin { get; set; }

        [BindProperty]
        public string Reason { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Post = await _httpClient.GetFromJsonAsync<PostDto>($"https://localhost:7178/api/PostsApi/{id}");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                IsOwnerOrAdmin = isAdmin || Post.UserId == userId;
                IsOwnPost = Post.UserId == userId;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCommentAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(NewComment.Content) || userId == null)
            {
                Post = await _httpClient.GetFromJsonAsync<PostDto>($"https://localhost:7178/api/PostsApi/{id}");
                ModelState.AddModelError(string.Empty, "Kommentaren måste ha innehåll.");
                return Page();
            }

            var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[".AspNetCore.Identity.Application"];
            if (!string.IsNullOrEmpty(cookie))
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");
                _httpClient.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={cookie}");
            }

            var dto = new CommentCreateDto
            {
                Content = NewComment.Content
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://localhost:7178/api/PostsApi/{id}/comments",
                dto);

            if (!response.IsSuccessStatusCode)
            {
                Post = await _httpClient.GetFromJsonAsync<PostDto>($"https://localhost:7178/api/PostsApi/{id}");
                ModelState.AddModelError(string.Empty, "Kunde inte spara kommentaren.");
                return Page();
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostReportAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(Reason) || userId == null)
            {
                Post = await _httpClient.GetFromJsonAsync<PostDto>($"https://localhost:7178/api/PostsApi/{id}");
                ModelState.AddModelError(string.Empty, "Du måste ange en anledning för rapport.");
                return Page();
            }

            var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[".AspNetCore.Identity.Application"];
            if (!string.IsNullOrEmpty(cookie))
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");
                _httpClient.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={cookie}");
            }

            var reportDto = new ReportCreateDto
            {
                PostId = id,
                Reason = Reason
            };

            var response = await _httpClient.PostAsJsonAsync("https://localhost:7178/api/ReportsApi", reportDto);

            if (!response.IsSuccessStatusCode)
            {
                Post = await _httpClient.GetFromJsonAsync<PostDto>($"https://localhost:7178/api/PostsApi/{id}");
                ModelState.AddModelError(string.Empty, "Misslyckades med att rapportera inlägget.");
                return Page();
            }

            return RedirectToPage(new { id });
        }
    }
}
