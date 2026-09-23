using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Services.Models.Discounts;

namespace FCanteen.Services.Discounts.Policies;

public class StaffDiscountPolicy
    : IDiscountPolicy
{
    public string Name =>
        "Lecturer and Staff 15%";

    public int Priority =>
        100;

    public bool CanApply(
        DiscountContext context)
    {
        return context.CustomerType
                   is CustomerType.Lecturer
                   or CustomerType.Staff;
    }

    public decimal CalculateDiscount(
        DiscountContext context)
    {
        return Math.Round(
            context.CurrentTotal * 0.15m,
            0,
            MidpointRounding.AwayFromZero);
    }
}
