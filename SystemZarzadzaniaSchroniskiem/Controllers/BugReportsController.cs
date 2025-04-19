using SystemZarzadzaniaSchroniskiem.Areas.Identity.Data;
using SystemZarzadzaniaSchroniskiem.Models;
using SystemZarzadzaniaSchroniskiem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SystemZarzadzaniaSchroniskiem.Controllers
{
    [Authorize]
    public class BugReportsController : Controller
    {
        public delegate void BugReportEventHandler(object sender, BugReportEventArgs e);

        public event BugReportEventHandler BugReportCreated;
        public event BugReportEventHandler BugReportCommentCreated;

        private readonly SystemZarzadzaniaSchroniskiemDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly BugReportService _bugReportService;

        public BugReportsController(SystemZarzadzaniaSchroniskiemDbContext context, UserManager<IdentityUser> userManager, BugReportService bugReportService)
        {
            _context = context;
            _userManager = userManager;
            _bugReportService = bugReportService;

            BugReportCreated += _bugReportService.OnBugReportCreated;
            BugReportCommentCreated += _bugReportService.OnBugReportCommentCreated;
        }

        public IActionResult Index()
        {
            var uid = _userManager.GetUserId(User);
            List<BugReport> reports;
            bool isAdmin = User.IsInRole("Administrator");

            if (isAdmin)
            {
                reports = _context.BugReports?.Include(br => br.User).ToList() ?? new List<BugReport>();
            } else
            {
                reports = _context.BugReports?.Where(r => r.UserId == uid).ToList() ?? new List<BugReport>();
            }

            ViewBag.Reports = reports;
            ViewBag.IsAdmin = isAdmin;
            return View();
        }

        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var report = _context.BugReports?.Include(br => br.User).FirstOrDefault(br => br.Id == id);
            var uid = _userManager.GetUserId(User);

            if (report == null || (!User.IsInRole("Administrator") && report.UserId != uid))
            {
                return RedirectToAction("Index");
            }

            var comments = _context.BugReportComments?.Where(brc => brc.BugReportId == report.Id).ToList();
            ViewBag.Comments = comments ?? new List<BugReportComment>();

            return View(report);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var report = _context.BugReports?.FirstOrDefault(br => br.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            if (!Enum.TryParse(status, out BugReportStatus reportStatus))
            {
                return BadRequest();
            }

            report.Status = reportStatus;
            _context.Update(report);

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([Bind("Content,BugReportId")] BugReportComment comment)
        {
            comment.DateCreated = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);
            _context.Add(comment);
            await _context.SaveChangesAsync();
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
            report.DateCreated = DateTime.Now;
            report.Status = BugReportStatus.Open;
            report.UserId = _userManager.GetUserId(User);
            _context.Add(report);
            await _context.SaveChangesAsync();
            BugReportCreated?.Invoke(this, new BugReportEventArgs { Report = report });
            return RedirectToAction("Details", new { Id = report.Id });
        }
    }
}
