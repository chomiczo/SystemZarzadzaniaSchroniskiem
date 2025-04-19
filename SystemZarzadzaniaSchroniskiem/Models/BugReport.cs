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
        public string UserId {  get; set; }
        public IdentityUser User { get; set; }
        public string Description { get; set; }
        public BugReportStatus Status { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class BugReportComment
    {
        public int Id { get; set; }
        public string UserId {  get; set; }
        public IdentityUser User { get; set; }
        public int BugReportId { get; set; }
        public DateTime DateCreated { get; set; }
        public string Content { get; set; }
    }
}
