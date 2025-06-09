using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Diskussionsforum.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Diskussionsforum.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ReportsModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportsModel(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = factory.CreateClient("api");
            _httpContextAccessor = httpContextAccessor;
        }

        public List<ReportDto> Reports { get; set; } = new();

        public async Task OnGetAsync()
        {
            AttachAuthCookie();

            Reports = await _httpClient.GetFromJsonAsync<List<ReportDto>>("https://localhost:7178/api/ReportsApi");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int postId, int reportId)
        {
            AttachAuthCookie();

            var response = await _httpClient.DeleteAsync($"https://localhost:7178/api/PostsApi/{postId}");

            if (response.IsSuccessStatusCode)
            {
                await _httpClient.PutAsync($"https://localhost:7178/api/ReportsApi/mark-handled/{reportId}", null);
            }
            else
            {
                ModelState.AddModelError("", "Kunde inte ta bort inlägget.");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkHandledAsync(int reportId)
        {
            AttachAuthCookie();

            var response = await _httpClient.PutAsync($"https://localhost:7178/api/ReportsApi/mark-handled/{reportId}", null);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Kunde inte markera rapporten som hanterad.");
            }

            return RedirectToPage();
        }

        private void AttachAuthCookie()
        {
            var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[".AspNetCore.Identity.Application"];

            if (!string.IsNullOrEmpty(cookie))
            {
                if (_httpClient.DefaultRequestHeaders.Contains("Cookie"))
                    _httpClient.DefaultRequestHeaders.Remove("Cookie");

                _httpClient.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={cookie}");
            }
        }
    }

    public class ReportDto
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string? PostTitle { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public bool IsHandled { get; set; }
    }
}
