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

    public class SchroniskoDbContext(
        DbContextOptions<SchroniskoDbContext> options
        ) : IdentityDbContext<IdentityUser>(options)
    {
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<List<string>>().HaveConversion<StringListConverter>();
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Pet>().HasMany(o => o.HealthRecords).WithOne(hr => hr.Pet).HasForeignKey(hr => hr.PetId);
            builder.Entity<Event>().HasMany(e => e.Attendees).WithOne(a => a.Event).HasForeignKey(a => a.EventId);
            builder.Entity<Event>().HasMany(e => e.Pets).WithOne(a => a.Event).HasForeignKey(a => a.EventId);
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<BugReport> BugReports { get; set; }
        public DbSet<BugReportComment> BugReportComments { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Breed> Breeds { get; set; }
        public DbSet<HealthRecord> HealthRecords { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }
        public DbSet<EventPet> EventPets { get; set; }
        public DbSet<Timetable> Timetables { get; set; }
    }

}
