using Sieve.Plus.Models;

namespace api.DTOs.QueryModels;

/// <summary>
/// Query model for Book entity.
/// This defines EXACTLY what properties can be filtered and sorted when querying books.
/// </summary>
/// <remarks>
/// Property types should match the entity types exactly for proper mapping.
/// The query model defines what's queryable, NOT what's required in a request.
/// </remarks>
public class BookQueryModel : ISievePlusQueryModel
{
    // Basic properties (match entity types exactly)
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int Pages { get; set; }
    public DateTime Createdat { get; set; }

    // Navigation properties
    public string GenreName { get; set; } = null!;
    public string GenreId { get; set; } = null!;

    // Date parts
    public int PublishedYear { get; set; }
    public int PublishedMonth { get; set; }
    public DateTime PublishedDate { get; set; }

    // Calculated/custom properties
    public int PageRangeStart { get; set; }  // Pages / 100 * 100
    public bool? IsLongBook { get; set; }      // Custom filter: Pages > 500
}
