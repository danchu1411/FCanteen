using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class TopMenuRevenueResult
{
    public int MenuItemId { get; set; }

    public string MenuItemName { get; set; } =
        string.Empty;

    public long TotalQuantity { get; set; }

    public decimal Revenue { get; set; }
}
