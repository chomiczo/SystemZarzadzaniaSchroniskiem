using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaSchroniskiem.Models;

namespace SystemZarzadzaniaSchroniskiem.Controllers
{
    public class EventsController(
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
            var events = await _dbContext.Events
              .Include(evt => evt.CoordinatorProfile)
              .ThenInclude(p => p.User)
              .Include(evt => evt.Attendees)
              .ToListAsync();
            return View(events);
        }

        [Authorize(Roles = "Administrator,Employee,Volunteer")]
        public async Task<IActionResult> Database()
        {
            var events = await _dbContext.Events
              .Include(evt => evt.CoordinatorProfile)
              .ThenInclude(p => p.User)
              .ToListAsync();
            return View(events);
        }


        [HttpGet("/Events/Attend/{id}")]
        public async Task<IActionResult> Attend([FromRoute] int id)
        {
            if (HttpContext.Items["UserProfile"] is not UserProfile profile)
            {
                return NotFound();
            }

            var evt = await _dbContext.Events.Include(e => e.Attendees).SingleOrDefaultAsync(e => e.Id == id);
            if (evt == null)
            {
                return NotFound();
            }

            var attendance = evt.Attendees.SingleOrDefault(a => a.AttendeeProfileId == profile.Id);
            if (attendance != null)
            {
                _dbContext.EventAttendees.Remove(attendance);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            attendance = new EventAttendee
            {
                AttendeeProfileId = profile.Id,
                EventId = evt.Id
            };

            await _dbContext.EventAttendees.AddAsync(attendance);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Employee,Volunteer")]
        public async Task<IActionResult> Create(Event evt)
        {
            var coordinator = _dbContext.UserProfiles.SingleOrDefault(p => p.Id == evt.CoordinatorProfileId);
            evt.CoordinatorProfile = coordinator;

            if (evt.EndDate <= evt.StartDate)
            {
                ModelState.AddModelError(String.Empty, "Zła data wydarzenia");

                AddViewMessage("Data zakończenia musi być późniejsza niż data rozpoczęcia wydarzenia.", ViewMessageType.Danger, "Błąd");
            }

            if (ModelState.IsValid)
            {
                await _dbContext.AddAsync(evt);
                await _dbContext.SaveChangesAsync();
                AddViewMessage("Pomyślnie dodano wydarzenie.", ViewMessageType.Success, "Sukces");
                return RedirectToAction(nameof(Database));
            }

            ViewBag.StaffProfiles = await GetStaffProfiles();

            return View(evt);
        }


        [Authorize(Roles = "Administrator,Employee,Volunteer")]
        public async Task<IActionResult> Create()
        {
            ViewBag.StaffProfiles = await GetStaffProfiles();
            return View();
        }

        [Authorize(Roles = "Administrator,Employee,Volunteer")]
        public async Task<IActionResult> Edit(int Id)
        {
            var evt = await _dbContext.Events.SingleOrDefaultAsync(p => p.Id == Id);
            if (evt == null)
            {
                return NotFound();
            }

            ViewBag.StaffProfiles = await GetStaffProfiles();
            return View(evt);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Employee,Volunteer")]
        public async Task<IActionResult> Edit(Event evt)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Update(evt);
                await _dbContext.SaveChangesAsync();
                AddViewMessage("Zmiany zostały zapisane.", ViewMessageType.Success, "Edycja wydarzenia");
                return RedirectToAction(nameof(Database));
            }

            ViewBag.StaffProfiles = await GetStaffProfiles();
            return View(evt);
        }

        [Authorize(Roles = "Administrator,Employee,Volunteer")]
        public async Task<IActionResult> Delete(int Id)
        {

            var evt = await _dbContext.Events.SingleOrDefaultAsync(p => p.Id == Id);
            if (evt == null)
            {
                return NotFound();
            }

            return View(evt);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Employee,Volunteer")]
        public async Task<IActionResult> Delete(Event evt)
        {
            _dbContext.Events.Remove(evt);
            await _dbContext.SaveChangesAsync();
            AddViewMessage("Pomyślnie usunięto wydarzenie", ViewMessageType.Success);
            return RedirectToAction(nameof(Database));
        }
    }
}
