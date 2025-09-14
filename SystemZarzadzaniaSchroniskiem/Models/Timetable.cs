using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SystemZarzadzaniaSchroniskiem.Models
{
  public enum Weekday
  {
    [Display(Name = "Niedziela")]
    Sunday,
    [Display(Name = "Poniedziałek")]
    Monday,
    [Display(Name = "Wtorek")]
    Tuesday,
    [Display(Name = "Środa")]
    Wednesday,
    [Display(Name = "Czwartek")]
    Thursday,
    [Display(Name = "Piątek")]
    Friday,
    [Display(Name = "Sobota")]
    Saturday,
  }

  public class Timetable
  {
    public int Id { get; set; }
    public int StaffUserProfileId { get; set; }
    [DeleteBehavior(DeleteBehavior.Cascade)]
    [Display(Name = "Osoba")]
    public UserProfile StaffUserProfile { get; set; } = null!;
    [Display(Name = "Dzień tygodnia")]
    public Weekday Weekday { get; set; }
    [Display(Name = "Początek")]
    public TimeOnly StartTime { get; set; }
    [Display(Name = "Koniec")]
    public TimeOnly EndTime { get; set; }
  }
}
