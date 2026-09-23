using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Models.Discounts;

public class DiscountRequest
{
    public CustomerType CustomerType { get; set; }

    public DateTime OrderTime { get; set; }

    public List<DiscountOrderItem> Items { get; set; }
        = [];

    public decimal Subtotal =>
        Items.Sum(
            x => x.LineTotal);
}
