using Diskussionsforum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public ProfileModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _env = env;
    }

    public async Task<IActionResult> OnPostAsync(IFormFile ProfilePicture)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || ProfilePicture == null) return Page();

        var uploadsFolder = Path.Combine(_env.WebRootPath, "profile_pics");
        Directory.CreateDirectory(uploadsFolder);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ProfilePicture.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await ProfilePicture.CopyToAsync(stream);
        }

        user.ProfilePictureUrl = $"/profile_pics/{fileName}";
        await _userManager.UpdateAsync(user);

        return RedirectToPage("/Index");
    }
}
