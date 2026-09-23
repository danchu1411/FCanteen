using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Models.Discounts;

public class DiscountContext
{
    public CustomerType CustomerType { get; set; }

    public DateTime OrderTime { get; set; }

    public IReadOnlyList<DiscountOrderItem>
        Items
    { get; set; }
        = [];

    public decimal OriginalSubtotal { get; set; }

    public decimal CurrentTotal { get; set; }
}
