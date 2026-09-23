using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Services.Models.Discounts;

namespace FCanteen.Services.Discounts.Policies;

public class ComboDiscountPolicy
    : IDiscountPolicy
{
    public string Name =>
        "Main dish + drink -5,000";

    public int Priority =>
        200;

    public bool CanApply(
        DiscountContext context)
    {
        var hasMainDish =
            context.Items.Any(
                x =>
                    x.Category ==
                    DiscountItemCategory.MainDish);

        var hasDrink =
            context.Items.Any(
                x =>
                    x.Category ==
                    DiscountItemCategory.Drink);

        return hasMainDish &&
               hasDrink;
    }

    public decimal CalculateDiscount(
        DiscountContext context)
    {
        return Math.Min(
            5_000m,
            context.CurrentTotal);
    }
}
