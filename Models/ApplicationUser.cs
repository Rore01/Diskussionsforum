using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Diskussionsforum.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FullName { get; set; }

        [Display(Name = "Profilbild")]
        public string? ProfilePictureUrl { get; set; }
    }
}
