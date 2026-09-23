using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class LowRevenueMenuResult
{
    public int MenuItemId { get; set; }

    public string MenuItemName { get; set; } =
        string.Empty;

    public decimal Revenue { get; set; }

    public decimal RevenuePercentage { get; set; }
}
