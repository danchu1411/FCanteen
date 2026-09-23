using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Contracts;

public class OrderRequest
{
    public string CounterName { get; set; } = string.Empty;

    public decimal ClientTotal { get; set; }

    public List<OrderLineRequest> Lines { get; set; } = [];
}
