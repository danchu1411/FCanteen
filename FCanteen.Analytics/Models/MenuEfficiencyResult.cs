using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class MenuEfficiencyResult
{
    public int MenuItemId { get; set; }

    public string MenuItemName { get; set; } =
        string.Empty;

    public long TotalQuantity { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal PeakRevenue { get; set; }

    public double PeakRatio { get; set; }

    public double EfficiencyScore { get; set; }
}
