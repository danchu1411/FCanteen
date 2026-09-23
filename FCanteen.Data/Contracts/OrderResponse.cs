using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Contracts;

public class OrderResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? TicketId { get; set; }

    public decimal TotalAmount { get; set; }
}
