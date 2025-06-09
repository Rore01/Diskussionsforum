using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;


namespace Diskussionsforum.Models
{
    public class ForumCategory
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Post> Posts { get; set; } = new();
    }

    public class Post
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; } = null!;

        [Required(ErrorMessage = "Välj en kategori")]
        public int? ForumCategoryId { get; set; }
        public ForumCategory ForumCategory { get; set; } = null!;

        public List<Comment> Comments { get; set; } = new();
        public List<Report> Reports { get; set; } = new();
    }

    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int PostId { get; set; }
        public Post? Post { get; set; } = null!;
    }

    public class PrivateMessage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Meddelandet får vara max 1000 tecken.")]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [ForeignKey(nameof(SenderId))]
        public ApplicationUser Sender { get; set; } = null!;

        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        [ForeignKey(nameof(ReceiverId))]
        public ApplicationUser Receiver { get; set; } = null!;
    }


    public class Report
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public string Reason { get; set; } = string.Empty;

        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        public bool IsHandled { get; set; } = false;
    }
}
