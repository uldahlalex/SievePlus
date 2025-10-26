using System;

namespace dataccess;

public partial class Computer
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Processor { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal ScreenSize { get; set; }
    public int Ram { get; set; } // In GB
    public int Storage { get; set; } // In GB
    public string GraphicsCard { get; set; } = null!;
    public bool InStock { get; set; }
    public double Rating { get; set; }
    public int Sales { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? BrandId { get; set; }
    public virtual Brand? Brand { get; set; }

    public string? CategoryId { get; set; }
    public virtual Category? Category { get; set; }
}
