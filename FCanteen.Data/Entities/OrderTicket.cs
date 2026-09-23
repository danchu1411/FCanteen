using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class OrderTicket
{
    public int OrderTicketId { get; set; }

    public string CounterName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = "Waiting";

    public ICollection<TicketLine> TicketLines { get; set; }
        = new List<TicketLine>();
}
