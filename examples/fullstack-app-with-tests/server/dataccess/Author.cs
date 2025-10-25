using System;
using System.Collections.Generic;

namespace dataccess;

public partial class Author
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
