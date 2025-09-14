using Microsoft.EntityFrameworkCore;

namespace SystemZarzadzaniaSchroniskiem.Models
{
  public enum AppointmentType
  {
    VeterinaryVisit,
    AnimalTherapy,
    AdoptionMeeting
  }

  public class Appointment
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public AppointmentType Type { get; set; }
    public int UserProfileId { get; set; }

    [DeleteBehavior(DeleteBehavior.NoAction)]
    public UserProfile UserProfile { get; set; } = null!;
    public int StaffUserProfileId { get; set; }
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public UserProfile StaffUserProfile { get; set; } = null!;
    public int? PetId { get; set; }
    public Pet? Pet { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime AppointmentEndDate { get; set; }
  }
}
