using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SystemZarzadzaniaSchroniskiem.Models
{
    public enum BugReportStatus
    {
        [Display(Name = "Otwarte")]
        Open,
        [Display(Name = "W toku")]
        InProgress,
        [Display(Name = "Zamknięte")]
        Closed,
        [Display(Name = "Rozwiązane")]
        Resolved
    }

    public class BugReport
    {
        public int Id { get; set; }
        public int? ProfileId { get; set; }
        [DeleteBehavior(DeleteBehavior.SetNull)]
        public UserProfile? Profile { get; set; }
        public string Description { get; set; } = null!;
        public BugReportStatus Status { get; set; }
        public DateTime DateCreated { get; set; }
        public List<BugReportComment> Comments { get; set; } = null!;

        [NotMapped]
        public bool Commentable => Status == BugReportStatus.Open || Status == BugReportStatus.InProgress;
    }

    public class BugReportComment
    {
        public int Id { get; set; }
        public int? ProfileId { get; set; }
        [DeleteBehavior(DeleteBehavior.SetNull)]
        public UserProfile? Profile { get; set; }
        public int BugReportId { get; set; }
        public BugReport BugReport { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public string Content { get; set; } = null!;
    }
}
