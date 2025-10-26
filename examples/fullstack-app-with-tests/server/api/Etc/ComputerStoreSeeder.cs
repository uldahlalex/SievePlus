using Bogus;
using dataccess;
using Microsoft.EntityFrameworkCore;

namespace api;

public class ComputerStoreSeeder(MyDbContext ctx) : ISeeder
{
    public async Task Seed()
    {
        // Ensure database is created first
        await ctx.Database.EnsureCreatedAsync();

        // Check if already seeded
        if (await ctx.Computers.AnyAsync())
            return;

        // Create brands
        var brands = new List<Brand>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "Dell", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid().ToString(), Name = "HP", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid().ToString(), Name = "Lenovo", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid().ToString(), Name = "Apple", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid().ToString(), Name = "Asus", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid().ToString(), Name = "Acer", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid().ToString(), Name = "MSI", CreatedAt = DateTime.UtcNow },
        };
        ctx.Brands.AddRange(brands);

        // Create categories
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "Laptop", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid().ToString(), Name = "Desktop", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid().ToString(), Name = "Workstation", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid().ToString(), Name = "Gaming", CreatedAt = DateTime.UtcNow },
        };
        ctx.Categories.AddRange(categories);

        await ctx.SaveChangesAsync();

        // Create computers with Bogus
        var processors = new[] { "Intel i5", "Intel i7", "Intel i9", "AMD Ryzen 5", "AMD Ryzen 7", "AMD Ryzen 9", "Apple M1", "Apple M2", "Apple M3" };
        var graphicsCards = new[] { "Intel UHD", "Intel Iris Xe", "NVIDIA GTX 1650", "NVIDIA RTX 3050", "NVIDIA RTX 3060", "NVIDIA RTX 4060", "NVIDIA RTX 4070", "AMD Radeon RX 6600", "Apple Integrated" };
        var ramOptions = new[] { 8, 16, 32, 64 };
        var storageOptions = new[] { 256, 512, 1024, 2048 };
        var screenSizes = new[] { 13.3m, 14.0m, 15.6m, 16.0m, 17.3m };

        var computerFaker = new Faker<Computer>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(c => c.Name, f => $"{f.PickRandom(brands).Name} {f.Commerce.ProductAdjective()} {f.PickRandom(categories).Name}")
            .RuleFor(c => c.Processor, f => f.PickRandom(processors))
            .RuleFor(c => c.Price, f => Math.Round(f.Random.Decimal(500, 4000), 2))
            .RuleFor(c => c.ScreenSize, f => f.PickRandom(screenSizes))
            .RuleFor(c => c.Ram, f => f.PickRandom(ramOptions))
            .RuleFor(c => c.Storage, f => f.PickRandom(storageOptions))
            .RuleFor(c => c.GraphicsCard, f => f.PickRandom(graphicsCards))
            .RuleFor(c => c.InStock, f => f.Random.Bool(0.8f)) // 80% in stock
            .RuleFor(c => c.Rating, f => Math.Round(f.Random.Double(3.0, 5.0), 1))
            .RuleFor(c => c.Sales, f => f.Random.Int(0, 500))
            .RuleFor(c => c.CreatedAt, f => DateTime.SpecifyKind(f.Date.Past(2), DateTimeKind.Utc))
            .RuleFor(c => c.BrandId, f => f.PickRandom(brands).Id)
            .RuleFor(c => c.CategoryId, f => f.PickRandom(categories).Id);

        var computers = computerFaker.Generate(200);
        ctx.Computers.AddRange(computers);

        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();
    }
}
