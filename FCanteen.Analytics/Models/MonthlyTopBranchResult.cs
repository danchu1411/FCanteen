using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class MonthlyTopBranchResult
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string BranchCode { get; set; } =
        string.Empty;

    public decimal Revenue { get; set; }
}
