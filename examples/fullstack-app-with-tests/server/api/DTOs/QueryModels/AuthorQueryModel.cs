using Sieve.Plus.Models;

namespace api.DTOs.QueryModels;

/// <summary>
/// Query model for Author entity.
/// This defines EXACTLY what properties can be filtered and sorted when querying authors.
/// </summary>
/// <remarks>
/// Property types should match the entity types exactly for proper mapping.
/// The query model defines what's queryable, NOT what's required in a request.
/// </remarks>
public class AuthorQueryModel : ISievePlusQueryModel
{
    // Basic properties (match entity types exactly)
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime Createdat { get; set; }

    // Date parts
    public int CreatedYear { get; set; }
    public int CreatedMonth { get; set; }
    public DateTime CreatedDate { get; set; }
}
