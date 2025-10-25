using dataccess;
using Sieve.Plus.Services;

namespace api.DTOs.QueryModels;

/// <summary>
/// Configuration that maps BookQueryModel properties to Book entity properties.
/// </summary>
public class BookQueryConfiguration : ISievePlusQueryConfiguration<BookQueryModel, Book>
{
    public void Configure(SievePlusQueryMapper<BookQueryModel, Book> mapper)
    {
        // Simple 1:1 property mappings
        mapper.Property<string>(q => q.Id, e => e.Id)
            .CanFilter()
            .CanSort();

        mapper.Property<string>(q => q.Title, e => e.Title)
            .CanFilter()
            .CanSort();

        mapper.Property<int>(q => q.Pages, e => e.Pages)
            .CanFilter()
            .CanSort();

        mapper.Property<DateTime>(q => q.Createdat, e => e.Createdat)
            .CanFilter()
            .CanSort();

        // Navigation properties
        mapper.Property<string>(q => q.GenreName, e => e.Genre.Name)
            .CanFilter()
            .CanSort();

        mapper.Property<string>(q => q.GenreId, e => e.Genreid)
            .CanFilter()
            .CanSort();

        // Date part extraction
        mapper.Property<int>(q => q.PublishedYear, e => e.Createdat.Year)
            .CanFilter()
            .CanSort();

        mapper.Property<int>(q => q.PublishedMonth, e => e.Createdat.Month)
            .CanFilter()
            .CanSort();

        mapper.Property<DateTime>(q => q.PublishedDate, e => e.Createdat.Date)
            .CanFilter()
            .CanSort();

        // Calculated property
        mapper.Property<int>(q => q.PageRangeStart, e => e.Pages / 100 * 100)
            .CanFilter()
            .CanSort();

        // Custom filter (boolean expression)
        mapper.CustomFilter(q => q.IsLongBook, e => e.Pages > 500);
    }
}
