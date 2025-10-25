using Sieve.Plus.Models;

namespace Sieve.Plus.Services
{
    /// <summary>
    /// Interface for configuring query model mappings to entities.
    /// Similar to Entity Framework's IEntityTypeConfiguration, this allows explicit mapping
    /// from query models (what users can filter/sort) to entity models (database schema).
    /// </summary>
    /// <typeparam name="TQueryModel">The query model that defines filterable/sortable properties</typeparam>
    /// <typeparam name="TEntity">The entity model (EF entity)</typeparam>
    /// <example>
    /// <code>
    /// public class BookQueryModel : ISievePlusQueryModel
    /// {
    ///     public string? Title { get; set; }
    ///     public bool? IsLongBook { get; set; }
    /// }
    ///
    /// public class BookQueryConfiguration : ISievePlusQueryConfiguration&lt;BookQueryModel, Book&gt;
    /// {
    ///     public void Configure(SievePlusQueryMapper&lt;BookQueryModel, Book&gt; mapper)
    ///     {
    ///         mapper.Property(q => q.Title, e => e.Title).CanFilter().CanSort();
    ///         mapper.CustomFilter(q => q.IsLongBook, e => e.Pages > 500);
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface ISievePlusQueryConfiguration<TQueryModel, TEntity>
        where TQueryModel : ISievePlusQueryModel
    {
        /// <summary>
        /// Configures the mapping between query model properties and entity properties.
        /// </summary>
        /// <param name="mapper">The mapper used to configure property mappings</param>
        void Configure(SievePlusQueryMapper<TQueryModel, TEntity> mapper);
    }
}
