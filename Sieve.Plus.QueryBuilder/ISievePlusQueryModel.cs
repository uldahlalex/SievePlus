namespace Sieve.Plus.QueryBuilder;

/// <summary>
/// Marker interface for Sieve.Plus query models.
/// Query models represent the filterable and sortable properties available for an entity.
/// </summary>
/// <remarks>
/// Query models should contain only the properties that are configured in your SievePlusProcessor
/// with CanFilter() and/or CanSort(). This provides compile-time safety for query building.
/// </remarks>
/// <example>
/// <code>
/// public class AuthorQueryModel : ISievePlusQueryModel
/// {
///     public string? Name { get; set; }
///     public DateTime? Createdat { get; set; }
///     public int? BooksCount { get; set; }  // Custom mapped property
/// }
///
/// var query = SieveQueryBuilder&lt;AuthorQueryModel&gt;.Create()
///     .FilterContains(a => a.Name, "Bob")
///     .FilterEquals(a => a.BooksCount, 5)  // Type-safe custom property!
///     .BuildQueryString();
/// </code>
/// </example>
public interface ISievePlusQueryModel
{
}
