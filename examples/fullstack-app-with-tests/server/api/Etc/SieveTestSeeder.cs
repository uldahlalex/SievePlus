using Bogus;
using dataccess;

namespace api;

public class SieveTestSeeder(MyDbContext ctx) : ISeeder
{
    public async Task Seed()
    {
        await ctx.Database.EnsureCreatedAsync();
        // Clear existing data
        ctx.Books.RemoveRange(ctx.Books);
        ctx.Authors.RemoveRange(ctx.Authors);
        ctx.Genres.RemoveRange(ctx.Genres);
        await ctx.SaveChangesAsync();

        // Set a deterministic seed for reproducibility
        Randomizer.Seed = new Random(12345);

        // Create genres (50 genres with varied, realistic names)
        var genreFaker = new Faker<Genre>()
            .RuleFor(g => g.Id, f => Guid.NewGuid().ToString())
            .RuleFor(g => g.Name, f => f.PickRandom(
                "Science Fiction", "Fantasy", "Mystery", "Thriller", "Romance",
                "Horror", "Historical Fiction", "Biography", "Autobiography", "Self-Help",
                "Business", "Philosophy", "Poetry", "Drama", "Adventure",
                "Crime", "Western", "Dystopian", "Paranormal", "Contemporary",
                "Classic", "Young Adult", "Children's Literature", "Graphic Novel", "Memoir",
                "True Crime", "Travel", "Cookbook", "Art & Photography", "Science",
                "History", "Politics", "Religion & Spirituality", "Psychology", "Sociology",
                "Economics", "Technology", "Health & Fitness", "Parenting", "Education",
                "Literary Fiction", "Urban Fantasy", "Space Opera", "Cyberpunk", "Steampunk",
                "Military Fiction", "Legal Thriller", "Medical Thriller", "Spy Fiction", "Satire"
            ))
            .RuleFor(g => g.Createdat, f => f.Date.Past(5).ToUniversalTime());

        var genres = genreFaker.Generate(50);
        ctx.Genres.AddRange(genres);
        await ctx.SaveChangesAsync();

        // Create authors (500 authors with realistic names)
        var authorFaker = new Faker<Author>()
            .RuleFor(a => a.Id, f => Guid.NewGuid().ToString())
            .RuleFor(a => a.Name, f => f.Name.FullName())
            .RuleFor(a => a.Createdat, f => f.Date.Past(10).ToUniversalTime());

        var authors = authorFaker.Generate(500);
        ctx.Authors.AddRange(authors);
        await ctx.SaveChangesAsync();

        // Create books (5000 books with highly varied, realistic titles)
        // This ensures obscure queries will likely return results
        var bookFaker = new Faker<Book>()
            .RuleFor(b => b.Id, f => Guid.NewGuid().ToString())
            .RuleFor(b => b.Title, f => GenerateRealisticBookTitle(f))
            .RuleFor(b => b.Pages, f => f.Random.Number(50, 1200))
            .RuleFor(b => b.Createdat, f => f.Date.Past(20).ToUniversalTime())
            .RuleFor(b => b.Genreid, f => f.PickRandom(genres).Id);

        var books = bookFaker.Generate(5000);
        ctx.Books.AddRange(books);
        await ctx.SaveChangesAsync();

        // Create author-book relationships (many-to-many)
        // Each book will have 1-4 authors randomly assigned
        var random = new Random(12345);
        foreach (var book in books)
        {
            var numAuthors = random.Next(1, 5); // 1 to 4 authors
            var selectedAuthors = authors.OrderBy(x => random.Next()).Take(numAuthors);

            foreach (var author in selectedAuthors)
            {
                book.Authors.Add(author);
            }
        }
        await ctx.SaveChangesAsync();

        // Stop tracking
        ctx.ChangeTracker.Clear();
    }

    private static string GenerateRealisticBookTitle(Faker f)
    {
        // Generate diverse, realistic book titles with varied structures
        var titleType = f.Random.Number(0, 9);

        return titleType switch
        {
            0 => $"The {Capitalize(f.Random.Word())} of {f.Name.FirstName()}", // "The Mystery of Alice"
            1 => CapitalizeWords(f.Random.Words()), // Two random words
            2 => $"{f.Commerce.ProductAdjective()} {Capitalize(f.Random.Word())}", // "Incredible Journey"
            3 => $"The {f.Commerce.ProductAdjective()} {Capitalize(f.Random.Word())}", // "The Silent Night"
            4 => $"{f.Name.FirstName()}'s {Capitalize(f.Random.Word())}", // "Sarah's Quest"
            5 => $"{Capitalize(f.Random.Word())} in {f.Address.City()}", // "Adventure in Tokyo"
            6 => $"The {Capitalize(f.Random.Word())} and the {Capitalize(f.Random.Word())}", // "The Lion and the Mouse"
            7 => CapitalizeWords(f.Random.Words(3)), // Three random words
            8 => $"A {Capitalize(f.Random.Word())} to {Capitalize(f.Random.Word())}", // "A Journey to Remember"
            _ => $"{Capitalize(f.Hacker.Adjective())} {Capitalize(f.Random.Word())}" // "Digital Fortress"
        };
    }

    private static string Capitalize(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;
        return char.ToUpper(word[0]) + word.Substring(1).ToLower();
    }

    private static string CapitalizeWords(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var words = text.Split(' ');
        return string.Join(" ", words.Select(Capitalize));
    }
}
