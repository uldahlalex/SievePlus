using System;
using System.Collections.Generic;

namespace dataccess;

public partial class Book
{
    public string Id { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int Pages { get; set; }

    public DateTime Createdat { get; set; }

    public string? Genreid { get; set; }

    public virtual Genre? Genre { get; set; }

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
}
