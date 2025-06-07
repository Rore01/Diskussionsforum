using Microsoft.AspNetCore.Mvc;
using Diskussionsforum.Data;
using Diskussionsforum.Models;
using Microsoft.EntityFrameworkCore;

namespace Diskussionsforum.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ForumApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ForumApi/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.ForumCategories.ToListAsync();
            return Ok(categories);
        }

        // Här kan du lägga till fler API-metoder för inlägg, kommentarer etc.
    }
}
