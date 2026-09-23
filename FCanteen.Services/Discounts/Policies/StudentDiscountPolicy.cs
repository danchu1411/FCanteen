using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Services.Models.Discounts;

namespace FCanteen.Services.Discounts.Policies;

public class StudentDiscountPolicy
    : IDiscountPolicy
{
    public string Name =>
        "Student 10%";

    public int Priority =>
        100;

    public bool CanApply(
        DiscountContext context)
    {
        return context.CustomerType ==
               CustomerType.Student;
    }

    public decimal CalculateDiscount(
        DiscountContext context)
    {
        return Math.Round(
            context.CurrentTotal * 0.10m,
            0,
            MidpointRounding.AwayFromZero);
    }
}
