using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class SalesFact
{
    public int MenuItemId { get; set; }

    public string MenuItemName { get; set; } =
        string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; }

    public string BranchCode { get; set; } =
        string.Empty;

    public decimal Revenue =>
        UnitPrice * Quantity;
}
