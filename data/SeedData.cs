using LibraryApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Data;

public static class SeedData
{
    public static async Task InitializeAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        // Step 1 — Create Roles
        string[] roles = { "Admin", "Member" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Step 2 — Create Default Admin
        var adminEmail = configuration["SuperAdmin:Email"] ?? "superadmin@libraryapp.com";
        var adminPassword = configuration["SuperAdmin:Password"] ?? "SuperAdmin@123";
        var adminFirstName = configuration["SuperAdmin:FirstName"] ?? "Super";
        var adminLastName = configuration["SuperAdmin:LastName"] ?? "Admin";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = adminFirstName,
                LastName = adminLastName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        // Step 3 — Seed Sample Books
        if (!await context.Books.AnyAsync())
        {
            var books = new List<Book>
            {
                new Book
                {
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    ISBN = "978-0132350884",
                    Genre = "Technology",
                    Description = "A handbook of agile software craftsmanship",
                    TotalCopies = 5,
                    CopiesAvailable = 5,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "The Pragmatic Programmer",
                    Author = "Andrew Hunt",
                    ISBN = "978-0201616224",
                    Genre = "Technology",
                    Description = "From journeyman to master",
                    TotalCopies = 3,
                    CopiesAvailable = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "Things Fall Apart",
                    Author = "Chinua Achebe",
                    ISBN = "978-0385474542",
                    Genre = "Fiction",
                    Description = "A story of pre-colonial Nigeria",
                    TotalCopies = 4,
                    CopiesAvailable = 4,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();
        }
    }
}