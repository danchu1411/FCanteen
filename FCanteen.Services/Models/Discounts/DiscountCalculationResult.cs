using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Models.Discounts;

public class DiscountCalculationResult
{
    public decimal Subtotal { get; set; }

    public decimal TotalDiscount { get; set; }

    public decimal FinalTotal { get; set; }

    public List<AppliedDiscountResult>
        AppliedPolicies
    { get; set; }
        = [];
}
