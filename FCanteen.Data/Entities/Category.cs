using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } =
        string.Empty;

    public ICollection<MenuItem> MenuItems
    {
        get;
        set;
    } = [];
}
