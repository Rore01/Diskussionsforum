namespace Diskussionsforum.DTOs
{
    public class ReportCreateDto
    {
        public int PostId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

}