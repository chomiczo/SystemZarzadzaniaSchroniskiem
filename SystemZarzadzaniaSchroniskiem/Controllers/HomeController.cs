using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaSchroniskiem.Models;

namespace SystemZarzadzaniaSchroniskiem.Controllers
{
    public class HomeController(
        Areas.Identity.Data.SchroniskoDbContext context,
        IWebHostEnvironment env,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IUserStore<IdentityUser> userStore,
        IEmailSender sender,
        ILogger<SchroniskoController> logger) : SchroniskoController(
            context, env, userManager, signInManager, userStore, sender, logger)
    {
        public async Task<IActionResult> Index()
        {
            var events = await _dbContext.Events
                .Where(evt => evt.StartDate > DateTime.Now)
                .OrderBy(evt => evt.StartDate).Take(2).ToListAsync();
            ViewBag.Events = events;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error(int? id)
        {
            ViewBag.StatusCode = id;

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
