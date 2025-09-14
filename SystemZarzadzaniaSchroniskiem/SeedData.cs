using SystemZarzadzaniaSchroniskiem.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace SystemZarzadzaniaSchroniskiem
{

    public class JSONBreedConverter : JsonConverter<Breed>
    {
        private Areas.Identity.Data.SchroniskoDbContext _dbContext;

        public JSONBreedConverter(Areas.Identity.Data.SchroniskoDbContext context)
        {
            this._dbContext = context;
        }

        public override Breed? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null)
            {
                throw new ArgumentException("Cannot convert non-string value to Breed");
            }

            return _dbContext.Breeds.SingleOrDefault((b) => b.Name == value);
        }

        public override void Write(Utf8JsonWriter writer, Breed value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Name);
        }
    }

    class PetWithHealthRecord : Pet
    {
        public List<string>? HealthRecords { get; set; }
    }

    public class SeedData
    {
        private static readonly string SEED_DATA_PATH = Path.Combine(AppContext.BaseDirectory, "SeedData");


        public static async Task Initialize(
            IServiceProvider serviceProvider,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            Areas.Identity.Data.SchroniskoDbContext context)
        {
            Console.WriteLine("Initialize method called.");
            string roleName = "Administrator";
            IdentityResult roleResult;

            // Sprawdź, czy rola już istnieje
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Jeśli rola nie istnieje, utwórz ją
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Sprawdź, czy administrator już istnieje
            IdentityUser? admin = await userManager.FindByEmailAsync("admin@admin.com");

            if (admin == null)
            {
                // Utwórz administratora tylko jeśli nie istnieje
                admin = new IdentityUser()
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                };

                // Jeśli administrator nie istnieje, utwórz go
                IdentityResult result = await userManager.CreateAsync(admin, "Admin123$");

                if (result.Succeeded)
                {
                    admin.EmailConfirmed = true;
                    await userManager.UpdateAsync(admin);

                    // Przypisz rolę "Administrator" do administratora
                    await userManager.AddToRoleAsync(admin, roleName);
                }
            }

            var profile = await context.UserProfiles.Where(
                p => p.UserId == admin.Id).SingleOrDefaultAsync();
            profile ??= new UserProfile
            {
                FirstName = "Admin",
                LastName = "Administracki",
                UserId = admin.Id
            };

            context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            await InsertUsers(userManager, roleManager, context);
            await InsertBreeds(context);
            await InsertPets(context, userManager);
            await InsertHealthRecords(context);


            context.Events.RemoveRange(context.Events);
            await context.SaveChangesAsync();

            var adam = await context.UserProfiles.FirstOrDefaultAsync(up => up.FirstName == "Adam");
            if (adam != null)
            {
                var evt = new Event
                {
                    CoordinatorProfileId = adam.Id,
                    CoordinatorProfile = adam,
                    Name = "Warsztaty ze Schroniskiem",
                    StartDate = DateTime.Now.AddDays(2),
                    EndDate = DateTime.Now.AddDays(2).AddHours(4),
                    Description = "Zapraszamy na warsztaty przybliżające pracę naszego schroniska. Dowiecie się, skąd trafiają do nas zwierzęta, jak o nie dbamy i na czym polega nasza działalność. Poznacie też naszych niezwykłych podopiecznych.",
                    Location = "Schronisko, 00-000 Miejscowość"

                };
                await context.Events.AddAsync(evt);
                await context.SaveChangesAsync();

                evt = new Event
                {
                    CoordinatorProfileId = adam.Id,
                    CoordinatorProfile = adam,
                    Name = "Psi Grill",
                    StartDate = DateTime.Now.AddDays(4),
                    EndDate = DateTime.Now.AddDays(4).AddHours(4),
                    Description = "Zapraszamy na hot-dogi.",
                    Location = "Schronisko, 00-000 Miejscowość"
                };
                await context.Events.AddAsync(evt);
                await context.SaveChangesAsync();

                evt = new Event
                {
                    CoordinatorProfileId = adam.Id,
                    CoordinatorProfile = adam,
                    Name = "Psi Grill Dzisiaj",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddHours(4),
                    Description = "Zapraszamy na stare hot-dogi.",
                    Location = "Schronisko, 00-000 Miejscowość"
                };
                await context.Events.AddAsync(evt);
                await context.SaveChangesAsync();

                evt = new Event
                {
                    CoordinatorProfileId = adam.Id,
                    CoordinatorProfile = adam,
                    Name = "Psi Grill Wczoraj",
                    StartDate = DateTime.Now.Subtract(TimeSpan.FromDays(2)),
                    EndDate = DateTime.Now.Subtract(TimeSpan.FromDays(2)).AddHours(2),
                    Description = "Zapraszamy na stare hot-dogi.",
                    Location = "Schronisko, 00-000 Miejscowość"
                };
                await context.Events.AddAsync(evt);
                await context.SaveChangesAsync();
            }


            var jan = await context.UserProfiles.Where(up => up.FirstName == "Jan").SingleOrDefaultAsync();
            var pets = await context.Pets.Take(3).ToListAsync();
            if (jan != null && pets != null)
            {
                foreach (var pet in pets)
                {
                    pet.OwnerProfileId = jan.Id;
                    pet.AdoptionStatus = AdoptionStatus.InTemporaryHome;
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task InsertPets(Areas.Identity.Data.SchroniskoDbContext context, UserManager<IdentityUser> userManager)
        {
            if (context.Pets.Any())
            {
                return;

            }
            // context.Pets.RemoveRange(context.Pets);
            // await context.SaveChangesAsync();

            var petData = await File.ReadAllTextAsync(Path.Combine(SEED_DATA_PATH, "pets.json"));
            var jsonSerializerOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter(), new JSONBreedConverter(context) } };
            var seedPets = JsonSerializer.Deserialize<List<PetWithHealthRecord>>(petData, jsonSerializerOptions);

            if (seedPets == null) return;

            context.Pets.AddRange(seedPets);
            await context.SaveChangesAsync();

            var vets = await userManager.GetUsersInRoleAsync("Veterinarian");
            var vet = vets.FirstOrDefault();

            if (vet == null)
            {
                return;
            }

            var vetProfile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == vet.Id);

            if (vetProfile != null)
            {

                foreach (var pet in seedPets)
                {
                    foreach (var record in pet?.HealthRecords ?? [])
                    {
                        if (pet != null)
                        {
                            await context.HealthRecords.AddAsync(new HealthRecord
                            {
                                Content = record,
                                CreationDate = DateTime.Now,
                                PetId = pet.Id,
                                UserProfileId = vetProfile.Id
                            });
                        }
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task InsertHealthRecords(Areas.Identity.Data.SchroniskoDbContext context)
        {
            await context.SaveChangesAsync();
        }

        private static async Task InsertBreeds(Areas.Identity.Data.SchroniskoDbContext context)
        {
            var breedData = await File.ReadAllTextAsync(Path.Combine(SEED_DATA_PATH, "breeds.json"));
            var jsonSerializerOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
            var seedBreeds = JsonSerializer.Deserialize<List<Breed>>(breedData, jsonSerializerOptions);

            if (seedBreeds == null) return;

            var existingNames = context.Breeds.Select(b => b.Name).ToList();
            var breedsToInsert = seedBreeds.Where(b => !existingNames.Contains(b.Name)).DistinctBy(b => b.Name);
            context.Breeds.AddRange(breedsToInsert);
            await context.SaveChangesAsync();
        }

        private static async Task InsertUsers(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            Areas.Identity.Data.SchroniskoDbContext context)
        {
            var users = new[] {
                (Email: "jan@example.com", Confirmed: true, Role: "Employee", FirstName: "Jan", LastName: "Pracowniczy"),
                (Email: "adam@example.com", Confirmed: true, Role: "Volunteer", FirstName: "Adam", LastName: "Wolontariacki"),
                (Email: "janina@example.com", Confirmed: true, Role: "Adopter", FirstName: "Janina", LastName: "Adopcyjna"),
                (Email: "balbina@example.com", Confirmed: true, Role: "Veterinarian", FirstName: "Balbina", LastName: "Weterynarska"),
                (Email: "ildefons@example.com", Confirmed: true, Role: "Veterinarian", FirstName: "Ildefons", LastName: "Weterynarski"),
                (Email: "witold@example.com", Confirmed: true, Role: "Veterinarian", FirstName: "Witold", LastName: "Weterynarski"),
                (Email: "hubert@example.com", Confirmed: false, Role: "Adopter", FirstName: "Hubert", LastName: "Niepotwierdzalski"),
            };

            foreach (var (Email, Confirmed, Role, FirstName, LastName) in users)
            {
                var user = await userManager.FindByEmailAsync(Email);

                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = Email,
                        Email = Email,
                        EmailConfirmed = Confirmed
                    };
                    await userManager.CreateAsync(user, "Admin123$");
                }

                await userManager.AddToRoleAsync(user, Role);

                user = await userManager.FindByEmailAsync(Email);
                if (user != null)
                {
                    var profile = await context.UserProfiles.Where(p => p.UserId == user.Id).SingleOrDefaultAsync();

                    profile ??= new UserProfile
                    {
                        FirstName = FirstName,
                        UserId = user.Id,
                        LastName = LastName,
                    };
                    context.UserProfiles.Update(profile);
                    await context.SaveChangesAsync();

                    if (profile.FirstName == "Balbina")
                    {
                        context.Timetables.RemoveRange(context.Timetables);
                        await context.SaveChangesAsync();

                        await context.Timetables.AddAsync(new Timetable
                        {
                            StaffUserProfileId = profile.Id,
                            Weekday = Weekday.Monday,
                            StartTime = new TimeOnly(8, 0),
                            EndTime = new TimeOnly(16, 0)
                        });
                        await context.SaveChangesAsync();

                        await context.Timetables.AddAsync(new Timetable
                        {
                            StaffUserProfileId = profile.Id,
                            Weekday = Weekday.Wednesday,
                            StartTime = new TimeOnly(12, 0),
                            EndTime = new TimeOnly(19, 0)
                        });

                        await context.SaveChangesAsync();

                    }
                    if (profile.FirstName == "Jan")
                    {
                        await context.Timetables.AddAsync(new Timetable
                        {
                            StaffUserProfileId = profile.Id,
                            Weekday = Weekday.Friday,
                            StartTime = new TimeOnly(8, 0),
                            EndTime = new TimeOnly(14, 0)
                        });
                        await context.SaveChangesAsync();
                    }
                    if (profile.FirstName == "Ildefons")
                    {
                        await context.Timetables.AddAsync(new Timetable
                        {
                            StaffUserProfileId = profile.Id,
                            Weekday = Weekday.Monday,
                            StartTime = new TimeOnly(8, 0),
                            EndTime = new TimeOnly(14, 0)
                        });
                        await context.SaveChangesAsync();

                        await context.Timetables.AddAsync(new Timetable
                        {
                            StaffUserProfileId = profile.Id,
                            Weekday = Weekday.Tuesday,
                            StartTime = new TimeOnly(10, 0),
                            EndTime = new TimeOnly(17, 0)
                        });
                        await context.SaveChangesAsync();

                        await context.Timetables.AddAsync(new Timetable
                        {
                            StaffUserProfileId = profile.Id,
                            Weekday = Weekday.Wednesday,
                            StartTime = new TimeOnly(12, 0),
                            EndTime = new TimeOnly(19, 0)
                        });

                        await context.SaveChangesAsync();
                    }
                }
            }

        }
    }
}
