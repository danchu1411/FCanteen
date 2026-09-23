using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class DailySettlement
{
    public int DailySettlementId { get; set; }

    public DateTime Date { get; set; }

    public string BranchCode { get; set; } = string.Empty;

    public int TotalTickets { get; set; }

    public decimal TotalRevenue { get; set; }

    public long CalculationTimeMs { get; set; }
}
