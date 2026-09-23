using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Models;

public class DailySummaryResult
{
    public DateTime Date { get; set; }

    public string BranchCode { get; set; } =
        string.Empty;

    public int TotalTickets { get; set; }

    public decimal TotalRevenue { get; set; }
}
