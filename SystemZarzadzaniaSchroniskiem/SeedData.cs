using System.Text.RegularExpressions;
using SystemZarzadzaniaSchroniskiem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SystemZarzadzaniaSchroniskiem.Areas.Identity.Data;


public class SeedData
{
    private static readonly string SEED_DATA_PATH = Path.Combine(AppContext.BaseDirectory, "SeedData");
 

    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SystemZarzadzaniaSchroniskiemDbContext context)
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
        IdentityUser admin = await userManager.FindByEmailAsync("admin@admin.com");

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
        customer.Email = admin.Email;
        context.Userek?.Update(customer);
        await context.SaveChangesAsync();
    }
}
