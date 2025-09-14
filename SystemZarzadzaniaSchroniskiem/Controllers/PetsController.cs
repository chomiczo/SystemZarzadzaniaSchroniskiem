using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaSchroniskiem.Models;

namespace SystemZarzadzaniaSchroniskiem.Controllers
{
    public class PetFilters
    {
        public Species? Species { get; set; }
        public Gender? Gender { get; set; }
        public AdoptionStatus? Status { get; set; }
    }

    public class AssignOwnerInput
    {
        public int PetId { get; set; }
        public int OwnerProfileId { get; set; }
        public AdoptionStatus Status { get; set; }
    }

    public class AcceptAdoptionInput
    {
        public int PetId { get; set; }
        public Pet Pet { get; set; } = null!;
        public int AdopterId { get; set; }
        public UserProfile Adopter { get; set; } = null!;
        public AdoptionStatus Status { get; set; }
    }

    public class PetsController(
        Areas.Identity.Data.SchroniskoDbContext context,
        IWebHostEnvironment env,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IUserStore<IdentityUser> userStore,
        IEmailSender sender,
        ILogger<SchroniskoController> logger) : SchroniskoController(
            context, env, userManager, signInManager, userStore, sender, logger)
    {
        public async Task<IActionResult> Index([FromQuery] PetFilters query)
        {
            AdoptionStatus? status = query.Status;
            var pets = await _dbContext.Pets
            .Include(p => p.Breed)
            .Where(p => query.Gender == null || p.Gender == query.Gender)
            .Where(p => query.Species == null || p.Species == query.Species)
            .Where(p => status == null || status == p.AdoptionStatus)
            .ToListAsync();

            ViewBag.Breeds = await _dbContext.Breeds.ToListAsync();
            return View(pets);
        }

        [Authorize(Roles = "Administrator,Veterinarian,Employee,Volunteer")]
        public async Task<IActionResult> Database()
        {
            var systemZarzadzaniaSchroniskiemDbContext = _dbContext.Pets
                .Include(p => p.Breed)
                .Include(p => p.OwnerProfile)
                .ThenInclude(o => o.User);

            ViewBag.Breeds = await _dbContext.Breeds.ToListAsync();
            return View(await systemZarzadzaniaSchroniskiemDbContext.ToListAsync());
        }


        [HttpGet("/Pets/AssignOwner")]
        [Authorize(Roles = "Administrator,Employee,Volunteer")]
        public async Task<IActionResult> AssignOwner([FromQuery] int petId)
        {
            var input = new AssignOwnerInput { PetId = petId };

            var pet = await _dbContext.Pets.Include(p => p.Breed).SingleOrDefaultAsync(p => p.Id == petId);
            var users = await _dbContext.UserProfiles.Include(up => up.User).Where(up => up.User.EmailConfirmed).ToListAsync();

            ViewBag.Pet = pet;
            ViewBag.Users = users;

            return View(input);
        }

        [Authorize(Roles = "Administrator,Employee,Volunteer")]
        [ValidateAntiForgeryToken]
        [HttpPost("/Pets/AssignOwner")]
        public async Task<IActionResult> AssignOwner([Bind("PetId,OwnerProfileId,Status")] AssignOwnerInput input)
        {
            var pet = await _dbContext.Pets.SingleOrDefaultAsync(p => p.Id == input.PetId);
            var users = await _dbContext.UserProfiles.Include(up => up.User).Where(up => up.User.EmailConfirmed).ToListAsync();

            var owner = await _dbContext.UserProfiles.SingleOrDefaultAsync(o => o.Id == input.OwnerProfileId);

            if (pet == null)
            {
                ModelState.AddModelError("PetId", "Brak zwierzęcia");
                ViewBag.Users = users;
                return View(input);
            }

            if (owner == null)
            {
                ModelState.AddModelError("OwnerProfileId", "Brak użytkownika");
                ViewBag.Users = users;
                return View(input);
            }

            pet.OwnerProfileId = owner.Id;
            pet.AdoptionStatus = input.Status;

            if (ModelState.IsValid)
            {
                await _dbContext.SaveChangesAsync();
            }

            AddViewMessage("Nadano opiekuna", ViewMessageType.Success);

            return Redirect(nameof(Database));
        }


        [Authorize(Roles = "Administrator,Volunteer,Employee")]
        public async Task<IActionResult> AcceptAdoption(int PetId, int AdopterId, AdoptionStatus AdoptionStatus)
        {
            var pet = await _dbContext.Pets.SingleOrDefaultAsync(p => p.Id == PetId);
            var adopter = await _dbContext.UserProfiles.Include(p => p.User).SingleOrDefaultAsync(p => p.Id == AdopterId);

            if (pet == null)
            {
                AddViewMessage("Brak podanego zwierzęcia.", ViewMessageType.Danger);
                return RedirectToAction("Index", "Appointments");
            }

            if (adopter == null)
            {
                AddViewMessage("Brak podanego użytkownika.", ViewMessageType.Danger);
                return RedirectToAction("Index", "Appointments");
            }

            var input = new AcceptAdoptionInput
            {
                AdopterId = AdopterId,
                Adopter = adopter,
                PetId = PetId,
                Pet = pet,
                Status = AdoptionStatus
            };
            return View(input);
        }

        [Authorize(Roles = "Administrator,Volunteer,Employee")]
        [HttpPost]
        public async Task<IActionResult> AcceptAdoption(AcceptAdoptionInput Input)
        {
            var pet = await _dbContext.Pets.SingleOrDefaultAsync(p => p.Id == Input.PetId);
            var adopter = await _dbContext.UserProfiles.Include(p => p.User).SingleOrDefaultAsync(p => p.Id == Input.AdopterId);

            if (pet == null)
            {
                AddViewMessage("Brak podanego zwierzęcia.", ViewMessageType.Danger);
                return RedirectToAction("Index", "Appointments");
            }

            if (adopter == null)
            {
                AddViewMessage("Brak podanego użytkownika.", ViewMessageType.Danger);
                return RedirectToAction("Index", "Appointments");
            }

            pet.OwnerProfileId = adopter.Id;
            pet.AdoptionStatus = Input.Status;
            _dbContext.Pets.Update(pet);
            await _dbContext.SaveChangesAsync();

            AddViewMessage("Zatwierdzono adopcję.", ViewMessageType.Success);
            return RedirectToAction("Index", "Appointments");
        }

        public async Task<IActionResult> Owned()
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Redirect("/Identity/Account/Login");
            }


            if (HttpContext.Items["UserProfile"] is not UserProfile profile)
            {
                AddViewMessage("Błąd pobierania profilu", ViewMessageType.Danger);
                return RedirectToAction("Index", "Pets");
            }

            var pets = await _dbContext.Pets
                .Include(p => p.HealthRecords)
                .Include(p => p.Breed)
                .Where(p => p.OwnerProfileId == profile.Id)
                .ToListAsync();

            return View(pets);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _dbContext.Pets
                .Include(p => p.Breed)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        public async Task<IActionResult> HealthRecords(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _dbContext.Pets
                .Include(p => p.Breed)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pet == null)
            {
                return NotFound();
            }

            var healthRecords = await _dbContext.HealthRecords
                .Include(r => r.UserProfile)
                .ThenInclude(p => p.User)
                .Where(r => r.PetId == pet.Id)
                .ToListAsync();
            ViewBag.HealthRecords = healthRecords;

            return View(pet);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Veterinarian")]
        public async Task<IActionResult> CreateHealthRecord([Bind("PetId,Content")] HealthRecord healthRecord)
        {
            healthRecord.CreationDate = DateTime.Now;
            healthRecord.UserProfileId = (HttpContext.Items["UserProfile"] as UserProfile).Id;
            await _dbContext.HealthRecords.AddAsync(healthRecord);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("HealthRecords", new { Id = healthRecord.PetId });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Veterinarian")]
        public async Task<IActionResult> DeleteHealthRecord([Bind("Id")] HealthRecord healthRecord)
        {
            _dbContext.HealthRecords.Remove(healthRecord);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("HealthRecords", new { Id = healthRecord.PetId });
        }

        [Authorize(Roles = "Administrator,Volunteer,Employee,Veterinarian")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Volunteer,Employee,Veterinarian")]
        public async Task<IActionResult> Create(Pet pet)
        {
            var breedName = HttpContext.Request.Form["BreedName"];
            var imageFile = HttpContext.Request.Form.Files["Image"];

            if (await _dbContext.Breeds.SingleOrDefaultAsync(b => b.Id == pet.BreedId) == null)
            {
                ModelState.AddModelError("BreedId", $"Nie znaleziono podanej rasy '{breedName}'");
            }

            if (DateTime.Now.Subtract(pet.AdmissionDate).TotalDays > 365 * 100)
            {

                ModelState.AddModelError("AdmissionDate", "Data przyjęcia jest niepoprawna");
            }

            if (ModelState.IsValid)
            {
                _dbContext.Add(pet);
                await _dbContext.SaveChangesAsync();

                if (imageFile != null)
                {
                    var dst = Path.Combine(_env.WebRootPath, "uploads", $"pet-{pet.Id}{Path.GetExtension(imageFile.FileName)}");
                    using (var stream = new FileStream(dst, FileMode.OpenOrCreate))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    pet.ImagePath = Url.Content($"pet-{pet.Id}{Path.GetExtension(imageFile.FileName)}");
                    _dbContext.Update(pet);
                    await _dbContext.SaveChangesAsync();
                }

                var redirectUrl = Url.Action(nameof(Database));
                if (redirectUrl == null)
                {
                    return NotFound();
                }
                return Redirect(redirectUrl);
            }

            ViewBag.BreedName = breedName;
            return View(pet);
        }

        [Authorize(Roles = "Administrator,Volunteer,Employee,Veterinarian")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _dbContext.Pets.Include(p => p.Breed).SingleOrDefaultAsync(p => p.Id == id);
            if (pet == null)
            {
                return NotFound();
            }
            ViewData["BreedId"] = new SelectList(_dbContext.Breeds, "Id", "Id", pet.BreedId);
            return View(pet);
        }

        [HttpPost(Order = 1)]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Volunteer,Employee,Veterinarian")]
        public async Task<IActionResult> Edit(Pet pet)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _dbContext.Update(pet);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PetExists(pet.Id))
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
            ViewData["BreedId"] = new SelectList(_dbContext.Breeds, "Id", "Id", pet.BreedId);
            return View(pet);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateBreed(Breed breed)
        {
            if (ModelState.IsValid)
            {
                var speciesText = breed.Species == Species.Cat ? "kota" : "psa";
                await _dbContext.Breeds.AddAsync(breed);
                await _dbContext.SaveChangesAsync();
                AddViewMessage($@"Dodano rasę {speciesText}: ""{breed.Name}""", ViewMessageType.Success);
                return RedirectToAction(nameof(Breeds));
            }

            AddViewMessage("Coś poszło nie tak przy dodawaniu nowej rasy.", ViewMessageType.Danger);

            return RedirectToAction(nameof(Breeds));
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteBreed(Breed breed)
        {
            if (_dbContext.Pets.Where(p => p.BreedId == breed.Id).Any())
            {
                AddViewMessage($@"Nie można usunąć rasy, do której należy już jakieś zwierzę.", ViewMessageType.Alert);
                return RedirectToAction(nameof(Breeds));
            }

            _dbContext.Remove(breed);
            await _dbContext.SaveChangesAsync();

            var speciesText = breed.Species == Species.Cat ? "kota" : "psa";
            var name = breed.Name;
            AddViewMessage($@"Usunięto rasę {speciesText}: ""{name}""", ViewMessageType.Success);

            return RedirectToAction(nameof(Breeds));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditBreed(int id)
        {
            var breed = await _dbContext.Breeds.FindAsync(id);
            if (breed == null)
            {
                return NotFound();
            }
            return View(breed);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditBreed(Breed breed)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Update(breed);
                await _dbContext.SaveChangesAsync();
                AddViewMessage("Pomyślnie zmieniono rasę", ViewMessageType.Success);
                return RedirectToAction(nameof(Breeds));
            }
            return View(breed);
        }


        [HttpGet]
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<IActionResult> DeleteConfirmation(int id)
        {
            var pet = await _dbContext.Pets.FindAsync(id);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<IActionResult> Delete([Bind("id")] int id)
        {
            var pet = await _dbContext.Pets.FindAsync(id);
            if (pet != null)
            {
                _dbContext.Pets.Remove(pet);
            }

            await _dbContext.SaveChangesAsync();

            AddViewMessage("Pomyślnie usunięto zwierzę", ViewMessageType.Success);

            return RedirectToAction(nameof(Database));
        }

        [HttpGet("/Pets/Adopt/{id}")]
        public async Task<IActionResult> Adopt(int id)
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                AddViewMessage("Musisz być zalogowany, żeby adoptować zwierzę", ViewMessageType.Alert);
                return Redirect("/Identity/Account/Login");
            }

            var pet = await _dbContext.Pets.Include(p => p.Breed).SingleOrDefaultAsync(p => p.Id == id);

            return View(pet);
        }

        public async Task<IActionResult> Breeds()
        {
            var breeds = await _dbContext.Breeds.ToListAsync();
            return View(breeds);
        }

        private bool PetExists(int id)
        {
            return _dbContext.Pets.Any(e => e.Id == id);
        }

        [HttpGet("api/breeds/{species}/{query?}")]
        public async Task<IActionResult> ApiGetBreeds(int species, string? query)
        {
            var breeds = await _dbContext.Breeds.Where(
                b => (int)b.Species == species
                && (query == null || b.Name.StartsWith(query))
            ).ToListAsync();

            return Json(breeds);
        }
    }
}
