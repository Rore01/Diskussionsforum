using Diskussionsforum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace Diskussionsforum.Pages.Posts
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public List<Post> Posts { get; set; } = new();

        public async Task OnGetAsync()
        {
            Posts = await _httpClient.GetFromJsonAsync<List<Post>>("https://localhost:7178/api/PostsApi") ?? new();
        }
    }
}
