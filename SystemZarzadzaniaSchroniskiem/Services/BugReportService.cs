using SystemZarzadzaniaSchroniskiem.Areas.Identity.Data;
using SystemZarzadzaniaSchroniskiem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace SystemZarzadzaniaSchroniskiem.Services
{
    public class BugReportEventArgs : EventArgs
    {
        public BugReport? Report { get; set; }
        public BugReportComment? Comment { get; set; }
}


    public class BugReportService
    {
        private readonly SystemZarzadzaniaSchroniskiemDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public BugReportService(SystemZarzadzaniaSchroniskiemDbContext context, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public void OnBugReportCreated(object sender, BugReportEventArgs e)
        {
            var report = e.Report;
            if (report == null)
            {
                return;
            }

            var admins = _userManager.GetUsersInRoleAsync("Administrator").Result;
            foreach (var admin in admins)
            {
                _emailSender.SendEmailAsync(
                    admin.Email,
                    "Nowe zgłoszenie",
                    $"Pojawiło się nowe zgłoszenie błędu (nr {report.Id}): <p>{report.Description}</p>");
            }
        }

        public void OnBugReportCommentCreated(object sender, BugReportEventArgs e)
        {
            var comment = e.Comment;
            if (comment == null)
            {
                return;
            }

            var admins = _userManager.GetUsersInRoleAsync("Administrator").Result;
            foreach (var admin in admins)
            {
                _emailSender.SendEmailAsync(
                    admin.Email,
                    "Nowy komentarz do zgłoszenia",
                    $"Pojawił się nowy komentarz do zgłoszenia nr {comment.BugReportId}:" +
                    $"<p>{comment.Content}</p>");
            }
        }
    }
}
