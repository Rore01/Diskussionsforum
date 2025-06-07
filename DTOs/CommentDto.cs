namespace Diskussionsforum.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = "";
        public string? UserProfilePictureUrl { get; set; }
    }

}
