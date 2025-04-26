using SystemZarzadzaniaSchroniskiem.Models;
using Microsoft.AspNetCore.Identity;
using SystemZarzadzaniaSchroniskiem.Areas.Identity.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace SystemZarzadzaniaSchroniskiem
{

    public class JSONBreedConverter : JsonConverter<Breed>
    {
        private SystemZarzadzaniaSchroniskiemDbContext _dbContext;

        public JSONBreedConverter(SystemZarzadzaniaSchroniskiemDbContext context)
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
    public class SeedData
    {
        private static readonly string SEED_DATA_PATH = Path.Combine(AppContext.BaseDirectory, "SeedData");


        public static async Task Initialize(
            IServiceProvider serviceProvider,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SystemZarzadzaniaSchroniskiemDbContext context)
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

            var customer = context.Userek?.FirstOrDefault() ?? new Userek();
            customer.UserId = admin.Id;
            customer.FirstName = "Admin";
            customer.LastName = "Administracki";
            customer.Email = admin.Email!;
            context.Userek?.Update(customer);
            await context.SaveChangesAsync();

            await InsertBreeds(context);
            await InsertPets(context);

        }

        private static async Task InsertPets(SystemZarzadzaniaSchroniskiemDbContext context)
        {
            context.Pets.RemoveRange(context.Pets);
            await context.SaveChangesAsync();

            var petData = await File.ReadAllTextAsync(Path.Combine(SEED_DATA_PATH, "pets.json"));
            var jsonSerializerOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter(), new JSONBreedConverter(context) } };
            var seedPets = JsonSerializer.Deserialize<List<Pet>>(petData, jsonSerializerOptions);

            if (seedPets == null) return;

            context.Pets.AddRange(seedPets);
            await context.SaveChangesAsync();
        }

        private static async Task InsertBreeds(SystemZarzadzaniaSchroniskiemDbContext context)
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
    }
}
