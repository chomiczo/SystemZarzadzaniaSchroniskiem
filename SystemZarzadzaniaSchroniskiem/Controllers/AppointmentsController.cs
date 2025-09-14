using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaSchroniskiem.Models;

namespace SystemZarzadzaniaSchroniskiem.Controllers
{
  public class VeterinaryVisitInput
  {
    public int VetProfileId { get; set; }
    public TimeOnly Hour { get; set; }
    public DateOnly Date { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
  }

  public class AdoptionMeetingInput
  {
    public int StaffProfileId { get; set; }
    public TimeOnly Hour { get; set; }
    public DateOnly Date { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
  }

  public class AppointmentsController(
    Areas.Identity.Data.SchroniskoDbContext context,
    IWebHostEnvironment env,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IUserStore<IdentityUser> userStore,
    IEmailSender sender,
    ILogger<SchroniskoController> logger) : SchroniskoController(
      context, env, userManager, signInManager, userStore, sender, logger)
  {

    public async Task<IActionResult> Index()
    {
      var profile = HttpContext.Items["UserProfile"] as UserProfile;
      var appointments = await _dbContext.Appointments
        .Include(p => p.Pet)
        .Include(p => p.UserProfile)
        .ThenInclude(p => p.User)
        .Include(p => p.StaffUserProfile)
        .ThenInclude(p => p.User)
        .Where(p => p.UserProfileId == profile.Id || p.StaffUserProfileId == profile.Id).ToListAsync();
      return View(appointments);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Database()
    {
      var appointments = await _dbContext.Appointments
        .Include(p => p.UserProfile)
        .ThenInclude(p => p.User)
        .Include(p => p.StaffUserProfile)
        .ThenInclude(p => p.User)
        .Include(p => p.Pet)
        .ToListAsync();
      return View(appointments);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmation(int Id)
    {
      var appointment = await _dbContext.Appointments
        .Include(p => p.UserProfile)
        .ThenInclude(p => p.User)
        .Include(p => p.StaffUserProfile)
        .ThenInclude(p => p.User)
        .Include(p => p.Pet)
        .SingleOrDefaultAsync(p => p.Id == Id);
      if (appointment == null)
      {
        return NotFound();
      }
      return View(appointment);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost(Order = 1)]
    public async Task<IActionResult> Delete(int Id)
    {
      var appointment = await _dbContext.Appointments
        .Include(p => p.UserProfile)
        .ThenInclude(p => p.User)
        .Include(p => p.StaffUserProfile)
        .ThenInclude(p => p.User)
        .Include(p => p.Pet)
        .SingleOrDefaultAsync(p => p.Id == Id);

      if (appointment == null)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        _dbContext.Appointments.Remove(appointment);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction("Database", "Appointments");
      }

      return NotFound();
    }

    [Authorize(Roles = "Administrator")]
    [Route("/Appointments/Timetable/{ProfileId}")]
    public async Task<IActionResult> Timetable(int ProfileId)
    {
      var profile = await _dbContext.UserProfiles
        .Include(p => p.Timetables)
        .Include(p => p.User)
        .SingleOrDefaultAsync(p => p.Id == ProfileId);
      if (profile == null)
      {
        return NotFound();
      }
      var roles = await _userManager.GetRolesAsync(profile.User);
      ViewBag.Roles = roles ?? [];
      return View(profile);
    }


    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteTimetable(int Id)
    {
      var timetable = await _dbContext.Timetables
        .Include(p => p.StaffUserProfile)
        .SingleOrDefaultAsync(p => p.Id == Id);

      if (timetable != null)
      {
        _dbContext.Remove(timetable);
        await _dbContext.SaveChangesAsync();
        AddViewMessage("Usunięto wpis w grafiku", ViewMessageType.Success, "Grafik");
        return RedirectToAction("Timetable", "Appointments", new { ProfileId = timetable.StaffUserProfileId });
      }

      return NotFound();
    }


    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateTimetable(Timetable timetable)
    {
      var profile = await _dbContext.UserProfiles
        .Include(p => p.Timetables)
        .SingleOrDefaultAsync(p => p.Id == timetable.StaffUserProfileId);
      var tt = profile?.Timetables?.SingleOrDefault(p => p.Weekday == timetable.Weekday);

      if (!timetable.StartTime.IsBetween(new TimeOnly(5, 59), new TimeOnly(22, 1))
          || !timetable.EndTime.IsBetween(new TimeOnly(5, 59), new TimeOnly(22, 1)))
      {
        AddViewMessage("Godziny pracy muszą zawierać się w przedziale 6:00 – 22:00", ViewMessageType.Alert, "Grafik");
        return RedirectToAction("Timetable", "Appointments", new { ProfileId = timetable.StaffUserProfileId });
      }

      if (timetable.EndTime - timetable.StartTime < TimeSpan.FromHours(1))
      {
        AddViewMessage("Czas pracy musi wynosić co najmniej godzinę.", ViewMessageType.Alert, "Grafik");
        return RedirectToAction("Timetable", "Appointments", new { ProfileId = timetable.StaffUserProfileId });
      }

      if (tt == null)
      {
        await _dbContext.AddAsync(timetable);
        AddViewMessage("Dodano wpis w grafiku", ViewMessageType.Primary, "Grafik");
      }
      else
      {
        tt.StartTime = timetable.StartTime;
        tt.EndTime = timetable.EndTime;
        _dbContext.Update(tt);
        AddViewMessage("Zaktualizowano istniejący wpis w grafiku", ViewMessageType.Primary, "Grafik");
      }


      await _dbContext.SaveChangesAsync();

      return RedirectToAction("Timetable", "Appointments", new { ProfileId = timetable.StaffUserProfileId });
    }

    public IActionResult AdoptionMeeting(int petId)
    {
      var pet = _dbContext.Pets.Include(p => p.Breed).SingleOrDefault(p => p.Id == petId);
      if (pet == null)
      {
        return NotFound();
      }

      if (pet.AdoptionStatus != AdoptionStatus.AvailableForAdoption)
      {
        AddViewMessage("To zwierzę nie jest dostępne do adopcji", ViewMessageType.Alert);
        return RedirectToAction("Index", "Pets");
      }

      var staff = ProfilesWithRoles()
        .Where(pr => pr.Roles.Contains("Employee") || pr.Roles.Contains("Volunteer"))
        .Select(p => p.Profile).ToList();
      ViewBag.Staff = staff;
      ViewBag.Pet = pet;
      var input = new AdoptionMeetingInput
      {
        PetId = pet.Id,
      };
      return View(input);
    }

    [HttpPost]
    public async Task<IActionResult> AdoptionMeeting(AdoptionMeetingInput input)
    {
      DateTime AppointmentStart = input.Date.ToDateTime(input.Hour);
      var AdoptionMeeting = new Appointment
      {
        AppointmentDate = AppointmentStart,
        Name = "Spotkanie Adopcyjne",
        PetId = input.PetId,
        StaffUserProfileId = input.StaffProfileId,
        AppointmentEndDate = AppointmentStart.AddMinutes(30),
        UserProfileId = (HttpContext.Items["UserProfile"] as UserProfile).Id,
        Type = AppointmentType.AdoptionMeeting
      };

      if (AppointmentStart < DateTime.Now)
      {
        ModelState.AddModelError(String.Empty, "Niepoprawna data wizyty");
      }

      if (ModelState.IsValid)
      {
        AddViewMessage("Dodano wizytę", ViewMessageType.Success);
        await _dbContext.Appointments.AddAsync(AdoptionMeeting);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction("Index", "Appointments");
      }

      AddViewMessage("Coś poszło nie tak", ViewMessageType.Danger);
      var pet = _dbContext.Pets.Include(p => p.Breed).SingleOrDefault(p => p.Id == input.PetId);
      if (pet == null)
      {
        return NotFound();
      }
      var staff = ProfilesWithRoles()
        .Where(pr => pr.Roles.Contains("Employee,Volunteer"))
        .Select(p => p.Profile).ToList();
      ViewBag.Staff = staff;
      ViewBag.Pet = pet;
      return View(input);
    }

    public IActionResult VeterinaryVisit(int petId)
    {
      var pet = _dbContext.Pets.Include(p => p.Breed).SingleOrDefault(p => p.Id == petId);
      if (pet == null)
      {
        return NotFound();
      }
      var vets = ProfilesWithRoles()
        .Where(pr => pr.Roles.Contains("Veterinarian"))
        .Select(p => p.Profile).ToList();
      ViewBag.Vets = vets;
      ViewBag.Pet = pet;
      var input = new VeterinaryVisitInput
      {
        PetId = pet.Id,
      };
      return View(input);
    }

    [HttpPost]
    public async Task<IActionResult> VeterinaryVisit(VeterinaryVisitInput input)
    {
      DateTime AppointmentStart = input.Date.ToDateTime(input.Hour);
      var VetAppointment = new Appointment
      {
        AppointmentDate = AppointmentStart,
        Name = "Wizyta Weterynaryjna",
        PetId = input.PetId,
        StaffUserProfileId = input.VetProfileId,
        AppointmentEndDate = AppointmentStart.AddMinutes(30),
        UserProfileId = (HttpContext.Items["UserProfile"] as UserProfile).Id,
        Type = AppointmentType.VeterinaryVisit
      };

      if (AppointmentStart < DateTime.Now)
      {
        ModelState.AddModelError(String.Empty, "Niepoprawna data wizyty");
      }

      if (ModelState.IsValid)
      {
        AddViewMessage("Dodano wizytę", ViewMessageType.Success);
        await _dbContext.Appointments.AddAsync(VetAppointment);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction("Index", "Appointments");
      }

      AddViewMessage("Coś poszło nie tak", ViewMessageType.Danger);
      var pet = _dbContext.Pets.Include(p => p.Breed).SingleOrDefault(p => p.Id == input.PetId);
      if (pet == null)
      {
        return NotFound();
      }
      var vets = ProfilesWithRoles()
        .Where(pr => pr.Roles.Contains("Veterinarian"))
        .Select(p => p.Profile).ToList();
      ViewBag.Vets = vets;
      ViewBag.Pet = pet;
      return View(input);
    }


    [HttpGet("api/hours/{StaffUserProfileId}/{Date}")]
    public async Task<IActionResult> AvailableHours(int StaffUserProfileId, DateTime Date)
    {
      var Weekday = (int)Date.DayOfWeek;

      var profile = await _dbContext.UserProfiles.Include(p => p.Timetables).SingleOrDefaultAsync(p => p.Id == StaffUserProfileId);
      if (profile == null)
      {
        return NotFound();
      }

      var tt = profile.Timetables?.FirstOrDefault(p => (int)p.Weekday == (int)Date.DayOfWeek);

      List<TimeOnly> Hours = [];
      if (tt == null)
      {
        return Json(Hours);
      }

      var existingAppointments = await _dbContext.Appointments.Where(p => p.StaffUserProfileId == profile.Id && p.AppointmentDate.Date == Date.Date).ToListAsync();

      var Hour = tt.StartTime;
      var DayEnd = tt.EndTime;
      while (Hour < DayEnd)
      {
        if (existingAppointments.FirstOrDefault(p => TimeOnly.FromDateTime(p.AppointmentDate) == Hour) == null)
        {

          Hours.Add(Hour);
        }
        Hour = Hour.AddMinutes(30);
      }
      return Json(Hours);
    }
  }
}
