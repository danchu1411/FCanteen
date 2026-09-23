using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class DiscountPolicyLog
{
    public int DiscountPolicyLogId { get; set; }

    public string PolicyName { get; set; } =
        string.Empty;

    public int Priority { get; set; }

    public string CustomerType { get; set; } =
        string.Empty;

    public DateTime OrderTime { get; set; }

    public decimal AmountBefore { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal AmountAfter { get; set; }

    public DateTime AppliedAt { get; set; }
}
