// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable


using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SystemZarzadzaniaSchroniskiem.Models;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaSchroniskiem.Areas.Identity.Data;


public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly SystemZarzadzaniaSchroniskiemDbContext _context;

    public IndexModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        SystemZarzadzaniaSchroniskiemDbContext context,
        IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _webHostEnvironment = webHostEnvironment;
        _context = context;
    }

    public string Username { get; set; }
    public string ProfileImagePath { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {

        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile ProfilePicture { get; set; }
    }

    private async Task LoadAsync(IdentityUser user)
    {
        var userName = await _userManager.GetUserNameAsync(user);
        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        var userId = _userManager.GetUserId(User);

        Username = userName;

        // Check if either a PNG or JPG profile image exists
        var pngPath = $"/img/{userId}.png";
        var jpgPath = $"/img/{userId}.jpg";
        ProfileImagePath = System.IO.File.Exists(Path.Combine(_webHostEnvironment.WebRootPath, "images/profiles", $"{userId}.png"))
        ? pngPath
            : jpgPath;

    }


    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Nie można załadować użytkownika.");
        }

        await LoadAsync(user);
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Nie można załadować użytkownika.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }


        /*
        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Input.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";
                return RedirectToPage();
            }
        }
        */

        // Handle Profile Picture Upload
        if (Input.ProfilePicture != null && Input.ProfilePicture.Length > 0)
        {
            var extension = Path.GetExtension(Input.ProfilePicture.FileName).ToLower();
            var allowedExtensions = new[] { ".png", ".jpg" };

            if (allowedExtensions.Contains(extension))
            {
                // Define the path to save the profile picture in the wwwroot/img directory
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", $"{user.Id}{extension}");

                // Delete existing profile picture files if any (both .png and .jpg)
                var existingPng = Path.Combine(_webHostEnvironment.WebRootPath, "img", $"{user.Id}.png");
                var existingJpg = Path.Combine(_webHostEnvironment.WebRootPath, "img", $"{user.Id}.jpg");
                if (System.IO.File.Exists(existingPng)) System.IO.File.Delete(existingPng);
                if (System.IO.File.Exists(existingJpg)) System.IO.File.Delete(existingJpg);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePicture.CopyToAsync(stream);
                }

                // Update ProfileImagePath to reflect the new file
                ProfileImagePath = $"/img/{user.Id}{extension}";
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid file format. Only PNG and JPG files are allowed.");
                await LoadAsync(user);
                return Page();
            }
        }

        // Password change logic
        if (!string.IsNullOrEmpty(Input.CurrentPassword) &&
            !string.IsNullOrEmpty(Input.NewPassword) &&
            !string.IsNullOrEmpty(Input.ConfirmPassword))
        {
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadAsync(user);
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Twoje hasło zostało zmienione!";
        }

        StatusMessage = "Twój profil został zaktualizowany!";
        return RedirectToPage();
    }
}
