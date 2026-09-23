using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Contracts;

public class StockStatusMessage
{
    public int MenuItemId { get; set; }

    public string MenuItemName { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    public DateTime SentAt { get; set; }
}
