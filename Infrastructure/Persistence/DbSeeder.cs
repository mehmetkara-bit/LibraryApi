using LibraryApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Migrations uygulanmış mı? (SQLite)
        await db.Database.MigrateAsync();

        // Category seed
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "Programming", Description = "Software development books" },
                new Category { Name = "History", Description = "History books" }
            );
            await db.SaveChangesAsync();
        }

        // Author seed
        if (!await db.Authors.AnyAsync())
        {
            db.Authors.AddRange(
                new Author { FirstName = "Robert", LastName = "Martin", Country = "USA" },
                new Author { FirstName = "Martin", LastName = "Fowler", Country = "UK" }
            );
            await db.SaveChangesAsync();
        }
    }
}