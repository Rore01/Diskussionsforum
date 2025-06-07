namespace Diskussionsforum.DTOs
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string UserName { get; set; } = string.Empty;
        public string? UserProfilePictureUrl { get; set; }
        public string Email { get; set; } = "";
        public string ForumCategoryTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = "";
        public List<CommentDto> Comments { get; set; } = new();
        
    }
}
