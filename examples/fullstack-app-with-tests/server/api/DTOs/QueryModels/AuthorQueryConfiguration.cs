using dataccess;
using Sieve.Plus.Services;

namespace api.DTOs.QueryModels;

/// <summary>
/// Configuration that maps AuthorQueryModel properties to Author entity properties.
/// </summary>
public class AuthorQueryConfiguration : ISievePlusQueryConfiguration<AuthorQueryModel, Author>
{
    public void Configure(SievePlusQueryMapper<AuthorQueryModel, Author> mapper)
    {
        // Simple 1:1 property mappings
        mapper.Property<string>(q => q.Id, e => e.Id)
            .CanFilter()
            .CanSort();

        mapper.Property<string>(q => q.Name, e => e.Name)
            .CanFilter()
            .CanSort();

        mapper.Property<DateTime>(q => q.Createdat, e => e.Createdat)
            .CanFilter()
            .CanSort();

        // Date part extraction
        mapper.Property<int>(q => q.CreatedYear, e => e.Createdat.Year)
            .CanFilter()
            .CanSort();

        mapper.Property<int>(q => q.CreatedMonth, e => e.Createdat.Month)
            .CanFilter()
            .CanSort();

        mapper.Property<DateTime>(q => q.CreatedDate, e => e.Createdat.Date)
            .CanFilter()
            .CanSort();
    }
}
