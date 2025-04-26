using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaSchroniskiem.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SystemZarzadzaniaSchroniskiem.Areas.Identity.Data
{
    public class StringListConverter : ValueConverter<List<string>, string>
    {
        public StringListConverter() : base(
            v => string.Join('\x1e', v),
            v => new List<string>(v.Split(new[] { '\x1e' }).Where(s => s != "_"))
        )
        { }
    }

    public class SystemZarzadzaniaSchroniskiemDbContext : IdentityDbContext<IdentityUser>
    {
        public SystemZarzadzaniaSchroniskiemDbContext(DbContextOptions<SystemZarzadzaniaSchroniskiemDbContext> options)
            : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<List<string>>().HaveConversion<StringListConverter>();
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        public DbSet<Userek> Userek { get; set; }
        public DbSet<BugReport> BugReports { get; set; }
        public DbSet<BugReportComment> BugReportComments { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Breed> Breeds { get; set; }
        public DbSet<HealthRecord> HealthRecords { get; set; }
    }
}