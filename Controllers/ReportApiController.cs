using Diskussionsforum.Data;
using Diskussionsforum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Diskussionsforum.DTOs;
using System.Security.Claims;

namespace Diskussionsforum.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetReports()
        {
            var reports = await _context.Reports
                .Include(r => r.Post)
                .Include(r => r.User)
                .OrderByDescending(r => r.ReportedAt)
                .Select(r => new ReportDto
                {
                    Id = r.Id,
                    PostId = r.PostId,
                    PostTitle = r.Post.Title,
                    UserId = r.UserId,
                    UserEmail = r.User.Email,
                    Reason = r.Reason,
                    ReportedAt = r.ReportedAt,
                    IsHandled = r.IsHandled
                })
                .ToListAsync();

            return Ok(reports);
        }

        [HttpPost]
        public async Task<IActionResult> PostReport(ReportCreateDto reportDto)
        {
            var post = await _context.Posts.FindAsync(reportDto.PostId);
            if (post == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var report = new Report
            {
                PostId = reportDto.PostId,
                Reason = reportDto.Reason,
                UserId = userId,
                ReportedAt = DateTime.UtcNow,
                IsHandled = false
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("mark-handled/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkReportHandled(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return NotFound();

            report.IsHandled = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO class
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
