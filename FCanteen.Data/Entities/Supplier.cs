using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class Supplier
{
    public int SupplierId { get; set; }

    public string Name { get; set; } =
        string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public ICollection<Ingredient> Ingredients
    {
        get;
        set;
    } = [];
}
