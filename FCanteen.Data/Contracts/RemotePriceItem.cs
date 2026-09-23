using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Contracts;

public class RemotePriceItem
{
    public int MenuItemId { get; set; }

    public string Name { get; set; } =
        string.Empty;

    public decimal Price { get; set; }
}
