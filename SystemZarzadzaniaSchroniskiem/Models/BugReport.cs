using Microsoft.AspNetCore.Identity;

namespace SystemZarzadzaniaSchroniskiem.Models
{
    public enum BugReportStatus
    {
        Open,
        InProgress,
        Closed,
        Resolved
    }

    public class BugReport
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
        public string Description { get; set; } = null!;
        public BugReportStatus Status { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class BugReportComment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!; 
        public IdentityUser User { get; set; } = null!;
        public int BugReportId { get; set; }
        public DateTime DateCreated { get; set; }
        public string Content { get; set; } = null!;
    }
}
