using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Contracts;

public class OrderLineRequest
{
    public int MenuItemId { get; set; }

    public int Quantity { get; set; }

    public string? Note { get; set; }
}
