using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class HighValueOrderResult
{
    public int OrderTicketId { get; set; }

    public string BranchCode { get; set; } =
        string.Empty;

    public string CounterName { get; set; } =
        string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}
