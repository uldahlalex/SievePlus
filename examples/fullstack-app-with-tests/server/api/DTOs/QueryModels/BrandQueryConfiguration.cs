using dataccess;
using Sieve.Plus.Services;

namespace api.DTOs.QueryModels;

public class BrandQueryConfiguration : ISievePlusQueryConfiguration<BrandQueryModel, Brand>
{
    public void Configure(SievePlusQueryMapper<BrandQueryModel, Brand> mapper)
    {
        mapper.Property(q => q.Id, e => e.Id).CanFilter().CanSort();
        mapper.Property(q => q.Name, e => e.Name).CanFilter().CanSort();
        mapper.Property(q => q.CreatedAt, e => e.CreatedAt).CanFilter().CanSort();
        mapper.Property(q => q.ComputerCount, e => e.Computers.Count).CanFilter().CanSort();
    }
}
