using System.Collections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaSchroniskiem.Areas.Identity.Data;
using SystemZarzadzaniaSchroniskiem.Controllers;
using SystemZarzadzaniaSchroniskiem.Models;

namespace SystemZarzadzaniaSchroniskiem
{
  public enum ViewMessageType
  {
    Primary,
    Alert,
    Danger,
    Success,
  }

  public class SchroniskoController(
    SchroniskoDbContext context,
    IWebHostEnvironment env,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IUserStore<IdentityUser> userStore,
    IEmailSender sender,
    ILogger<SchroniskoController> logger) : Controller
  {
    protected readonly ILogger<SchroniskoController> _logger = logger;
    protected readonly SchroniskoDbContext _dbContext = context;
    protected readonly UserManager<IdentityUser> _userManager = userManager;
    protected readonly SignInManager<IdentityUser> _signInManager = signInManager;
    protected readonly IWebHostEnvironment _env = env;
    protected readonly IUserStore<IdentityUser> _userStore = userStore;
    protected readonly IUserEmailStore<IdentityUser> _emailStore = (IUserEmailStore<IdentityUser>)userStore;
    protected readonly IEmailSender _sender = sender;

    protected void AddViewMessage(string content, ViewMessageType type, string title = "")
    {
      TempData["ViewMessage"] = content;
      TempData["ViewMessageType"] = $"{type}";
      TempData["ViewMessageTitle"] = $"{title}";
    }

    protected async Task<List<UserProfile>> GetStaffProfiles()
    {
      return await _dbContext.UserProfiles
            .Include(p => p.User)
            .Select(p => new
            {
              Profile = p,
              Roles = (
                  from ur in _dbContext.UserRoles
                  join role in _dbContext.Roles
                  on ur.RoleId equals role.Id
                  where ur.UserId == p.UserId
                  select role.Name).ToList()
            }).Where(pr => pr.Roles.Contains("Employee") || pr.Roles.Contains("Volunteer")).Select(pr => pr.Profile).ToListAsync();
    }

    protected IEnumerable<ProfileWithRoles> ProfilesWithRoles()
    {
      return _dbContext.UserProfiles
                         .Include(p => p.User)
                         .Include(p => p.Timetables)
                         .Select(p => new ProfileWithRoles
                         {
                           Profile = p,
                           Roles = (from ur in _dbContext.UserRoles
                                    join role in _dbContext.Roles on ur.RoleId equals role.Id
                                    where ur.UserId == p.UserId
                                    select role.Name).ToList()
                         });
    }
  }
}
