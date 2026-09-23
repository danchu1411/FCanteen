using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Services.Models.Discounts;

namespace FCanteen.Services.Discounts;

public interface IDiscountPolicy
{
    string Name { get; }

    int Priority { get; }

    bool CanApply(
        DiscountContext context);

    decimal CalculateDiscount(
        DiscountContext context);
}
