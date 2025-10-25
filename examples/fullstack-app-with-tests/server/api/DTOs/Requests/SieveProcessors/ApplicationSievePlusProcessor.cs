using System.Linq;
using api.DTOs.QueryModels;
using dataccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sieve.Plus.Models;
using Sieve.Plus.Services;

namespace api.Services;

/// <summary>
/// Custom Sieve.Plus processor with both traditional fluent API configuration
/// and new explicit query model configurations
/// </summary>
public class ApplicationSievePlusProcessor : SievePlusProcessor
{
    public ApplicationSievePlusProcessor(IOptions<SievePlusOptions> options) : base(options)
    {
    }

    /// <summary>
    /// Configure explicit query models for type-safe querying.
    /// Query models define exactly what properties can be filtered/sorted.
    /// </summary>
    protected override void ConfigureQueryModels(SievePlusQueryModelRegistry registry)
    {
        // Register query model configurations
        registry.AddConfiguration<BookQueryConfiguration>();
        registry.AddConfiguration<AuthorQueryConfiguration>();

        // Alternative: Scan entire assembly for all configurations
        // registry.AddConfigurationsFromAssembly(typeof(BookQueryConfiguration).Assembly);
    }

    protected override SievePlusPropertyMapper MapProperties(SievePlusPropertyMapper mapper)
    {
        // ========== AUTHOR PROPERTIES ==========

        // Basic properties
        mapper.Property<Author>(a => a.Id)
            .CanFilter()
            .CanSort();

        mapper.Property<Author>(a => a.Name)
            .CanFilter()
            .CanSort();

        mapper.Property<Author>(a => a.Createdat)
            .CanFilter()
            .CanSort();

        // Date part extraction - useful for filtering/sorting by year, month, etc.
        mapper.Property<Author>(a => a.Createdat.Year)
            .CanFilter()
            .CanSort()
            .HasName("CreatedYear");

        mapper.Property<Author>(a => a.Createdat.Month)
            .CanFilter()
            .CanSort()
            .HasName("CreatedMonth");

        mapper.Property<Author>(a => a.Createdat.Date)
            .CanFilter()
            .CanSort()
            .HasName("CreatedDate"); // Date without time component

        // ========== BOOK PROPERTIES ==========

        // Basic properties
        mapper.Property<Book>(b => b.Id)
            .CanFilter()
            .CanSort();

        mapper.Property<Book>(b => b.Title)
            .CanFilter()
            .CanSort();

        mapper.Property<Book>(b => b.Pages)
            .CanFilter()
            .CanSort();

        mapper.Property<Book>(b => b.Createdat)
            .CanFilter()
            .CanSort();

        // Navigational property - access Genre properties from Book queries
        mapper.Property<Book>(b => b.Genre.Name)
            .CanFilter()
            .CanSort()
            .HasName(SieveConstants.GenreName);

        mapper.Property<Book>(b => b.Genreid)
            .CanFilter()
            .CanSort()
            .HasName(SieveConstants.GenreId);

        // Date parts for books
        mapper.Property<Book>(b => b.Createdat.Year)
            .CanFilter()
            .CanSort()
            .HasName("PublishedYear");

        mapper.Property<Book>(b => b.Createdat.Month)
            .CanFilter()
            .CanSort()
            .HasName("PublishedMonth");

        mapper.Property<Book>(b => b.Createdat.Date)
            .CanFilter()
            .CanSort()
            .HasName("PublishedDate");

        // Calculated property - page range categorization (e.g., 0-99, 100-199, 200-299, etc.)
        mapper.Property<Book>(b => b.Pages / 100 * 100)
            .CanFilter()
            .CanSort()
            .HasName("PageRangeStart");

        // Boolean expressions - useful for categorization
        mapper.Property<Book>(b => b.Pages > 500)
            .CanFilter()
            .HasName("IsLongBook"); // Use: ?Filters=IsLongBook==true

        // ========== GENRE PROPERTIES ==========

        // Basic properties
        mapper.Property<Genre>(g => g.Id)
            .CanFilter()
            .CanSort();

        mapper.Property<Genre>(g => g.Name)
            .CanFilter()
            .CanSort();

        mapper.Property<Genre>(g => g.Createdat)
            .CanFilter()
            .CanSort();

        // Date parts for genres
        mapper.Property<Genre>(g => g.Createdat.Year)
            .CanFilter()
            .CanSort()
            .HasName("CreatedYear");

        mapper.Property<Genre>(g => g.Createdat.Month)
            .CanFilter()
            .CanSort()
            .HasName("CreatedMonth");

        return mapper;
    }
}
