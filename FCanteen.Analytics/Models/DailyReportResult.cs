using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class DailyReportResult
{
    public DateTime Date { get; set; }

    public string BranchCode { get; set; } =
        string.Empty;

    public int TotalTickets { get; set; }

    public decimal TotalRevenue { get; set; }

    public TopMenuDailyResult? TopMenu { get; set; }

    public PeakHourResult? PeakHour { get; set; }
}
