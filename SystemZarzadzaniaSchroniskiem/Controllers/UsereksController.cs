using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaSchroniskiem.Areas.Identity.Data;
using SystemZarzadzaniaSchroniskiem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SystemZarzadzaniaSchroniskiem.Controllers
{
    [Authorize]
    public class UsereksController : Controller
    {
        private readonly SystemZarzadzaniaSchroniskiemDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<UsereksController> _logger;

        public UsereksController(SystemZarzadzaniaSchroniskiemDbContext context, UserManager<IdentityUser> userManager, ILogger<UsereksController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Usereks
        public async Task<IActionResult> Index()
        {
            IdentityUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            return _context.Userek != null
                ? View(await _context.Userek.Where(u => u.UserId == user.Id).ToListAsync())
                : Problem("Entity set 'SystemZarzadzaniaSchroniskiemDbContext.Userek' is null.");
        }

        // GET: Usereks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Userek == null)
            {
                return NotFound();
            }

            var userek = await _context.Userek.FirstOrDefaultAsync(m => m.Id == id);
            if (userek == null)
            {
                return NotFound();
            }

            return View(userek);
        }

        // GET: Usereks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usereks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Email")] Userek userek)
        {
            IdentityUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            userek.UserId = user.Id;

            var existingUserek = _context.Userek.FirstOrDefault(u => u.UserId == user.Id);
            if (existingUserek != null)
            {
                ModelState.AddModelError(string.Empty, "Można wypełnić formularz tylko raz. Konto użytkownika już istnieje.");
            }

            if (ModelState.IsValid && existingUserek == null)
            {
                _context.Add(userek);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(userek);
        }

        // GET: Usereks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Userek == null)
            {
                return NotFound();
            }

            var userek = await _context.Userek.FindAsync(id);
            if (userek == null)
            {
                return NotFound();
            }

            return View(userek);
        }

        // POST: Usereks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,UserId")] Userek userek)
        {
            if (id != userek.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userek);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserekExists(userek.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(userek);
        }

        // GET: Usereks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Userek == null)
            {
                return NotFound();
            }

            var userek = await _context.Userek.FirstOrDefaultAsync(m => m.Id == id);
            if (userek == null)
            {
                return NotFound();
            }

            return View(userek);
        }

        // POST: Usereks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Userek == null)
            {
                return Problem("Entity set 'SystemZarzadzaniaSchroniskiemDbContext.Userek' is null.");
            }

            var userek = await _context.Userek.FindAsync(id);
            if (userek != null)
            {
                _context.Userek.Remove(userek);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserekExists(int id)
        {
            return (_context.Userek?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
