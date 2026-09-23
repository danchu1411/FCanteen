using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Models.Discounts;

public class DiscountOrderItem
{
    public string Name { get; set; } =
        string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public DiscountItemCategory Category
    {
        get;
        set;
    }

    public decimal LineTotal =>
        UnitPrice * Quantity;
}