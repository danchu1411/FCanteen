using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Models.Discounts;

public class AppliedDiscountResult
{
    public string PolicyName { get; set; } =
        string.Empty;

    public int Priority { get; set; }

    public decimal AmountBefore { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal AmountAfter { get; set; }
}
