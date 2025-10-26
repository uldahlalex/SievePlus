using dataccess;
using Sieve.Plus.Services;

namespace api.DTOs.QueryModels;

public class CategoryQueryConfiguration : ISievePlusQueryConfiguration<CategoryQueryModel, Category>
{
    public void Configure(SievePlusQueryMapper<CategoryQueryModel, Category> mapper)
    {
        mapper.Property(q => q.Id, e => e.Id).CanFilter().CanSort();
        mapper.Property(q => q.Name, e => e.Name).CanFilter().CanSort();
        mapper.Property(q => q.CreatedAt, e => e.CreatedAt).CanFilter().CanSort();
        mapper.Property(q => q.ComputerCount, e => e.Computers.Count).CanFilter().CanSort();
    }
}
