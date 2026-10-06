using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data;

public static class OrderStatuses
{
    public const string Waiting = "Waiting";
    public const string Preparing = "Preparing";
    public const string Ready = "Ready";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
}
