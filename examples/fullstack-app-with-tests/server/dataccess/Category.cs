using System;
using System.Collections.Generic;

namespace dataccess;

public partial class Category
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Computer> Computers { get; set; } = new List<Computer>();
}
