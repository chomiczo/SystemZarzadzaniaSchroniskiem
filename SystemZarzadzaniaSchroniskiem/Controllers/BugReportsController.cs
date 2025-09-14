using SystemZarzadzaniaSchroniskiem.Models;
using SystemZarzadzaniaSchroniskiem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaSchroniskiem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace SystemZarzadzaniaSchroniskiem.Controllers
{
    [Authorize]
    public class BugReportsController : SchroniskoController
    {
        public delegate void BugReportEventHandler(object sender, BugReportEventArgs e);

        public event BugReportEventHandler BugReportCreated;
        public event BugReportEventHandler BugReportCommentCreated;

        private readonly BugReportService _bugReportService;

        public BugReportsController(
            SchroniskoDbContext context,
            IWebHostEnvironment env,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IUserStore<IdentityUser> userStore,
            IEmailSender sender,
            ILogger<SchroniskoController> logger,
            BugReportService bugReportService) : base(
                context, env, userManager, signInManager, userStore, sender, logger)
        {
            _bugReportService = bugReportService;
            BugReportCreated += _bugReportService.OnBugReportCreated;
            BugReportCommentCreated += _bugReportService.OnBugReportCommentCreated;
        }

        public async Task<IActionResult> Index()
        {
            List<BugReport> reports;

            if (HttpContext.Items["UserProfile"] is not UserProfile profile)
            {
                return Forbid();
            }

            bool isAdmin = await _userManager.IsInRoleAsync(profile.User, "Administrator");

            if (isAdmin)
            {
                reports = await _dbContext.BugReports.Include(br => br.Profile).ThenInclude(p => p.User).ToListAsync();
            }
            else
            {
                reports = await _dbContext.BugReports.Where(r => r.ProfileId == profile.Id).ToListAsync();
            }

            return View(reports);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Items["UserProfile"] is not UserProfile profile)
            {
                return Forbid();
            }

            var report = _dbContext.BugReports
                .Include(br => br.Profile)
                .ThenInclude(p => p.User)
                .Include(br => br.Comments)
                .FirstOrDefault(br => br.Id == id);

            if (report == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(profile.User, "Administrator");

            if (!isAdmin && report.Profile != profile)
            {
                return Forbid();
            }

            return View(report);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeStatus(int BugReportId, BugReportStatus Status)
        {
            var report = await _dbContext.BugReports.SingleOrDefaultAsync(br => br.Id == BugReportId);
            if (report == null)
            {
                return NotFound();
            }

            report.Status = Status;

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { Id = report.Id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([Bind("Content,BugReportId")] BugReportComment comment)
        {
            if (HttpContext.Items["UserProfile"] is not UserProfile profile)
            {
                return Forbid();
            }
            comment.DateCreated = DateTime.Now;
            comment.ProfileId = profile.Id;
            _dbContext.Add(comment);
            await _dbContext.SaveChangesAsync();
            BugReportCommentCreated?.Invoke(this, new BugReportEventArgs { Comment = comment });

            return RedirectToAction("Details", new { Id = comment.BugReportId });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description")] BugReport report)
        {
            if (HttpContext.Items["UserProfile"] is not UserProfile profile)
            {
                return Forbid();
            }
            report.DateCreated = DateTime.Now;
            report.Status = BugReportStatus.Open;
            report.ProfileId = profile.Id;
            _dbContext.Add(report);
            await _dbContext.SaveChangesAsync();
            BugReportCreated?.Invoke(this, new BugReportEventArgs { Report = report });
            return RedirectToAction("Details", new { Id = report.Id });
        }
    }
}
