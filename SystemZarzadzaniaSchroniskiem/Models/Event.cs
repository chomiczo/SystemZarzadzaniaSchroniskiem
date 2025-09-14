using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SystemZarzadzaniaSchroniskiem.Models
{
  public class Event
  {
    public int Id { get; set; }
    [Display(Name = "Nazwa")]
    public string Name { get; set; } = null!;
    [Display(Name = "Opis")]
    public string Description { get; set; } = null!;
    public int? CoordinatorProfileId { get; set; }
    [DeleteBehavior(DeleteBehavior.SetNull)]
    [Display(Name = "Koordynator")]
    [ForeignKey("CoordinatorProfileId")]
    public UserProfile? CoordinatorProfile { get; set; }
    [Display(Name = "Data rozpoczęcia")]
    public DateTime StartDate { get; set; }
    [Display(Name = "Data zakończenia")]
    public DateTime EndDate { get; set; }
    [Display(Name = "Miejsce")]
    public string Location { get; set; } = null!;

    public List<EventAttendee> Attendees { get; set; } = [];
    public List<EventPet> Pets { get; set; } = [];
  }

  public class EventAttendee
  {
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int? AttendeeProfileId { get; set; }
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public UserProfile? AttendeeProfile { get; set; }
  }

  public class EventPet
  {
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
  }
}
