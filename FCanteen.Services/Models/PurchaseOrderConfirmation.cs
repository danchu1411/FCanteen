using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Models;

public class PurchaseOrderConfirmation
{
    public string OrderNumber
    {
        get;
        set;
    } = string.Empty;

    public int LineCount
    {
        get;
        set;
    }

    public decimal TotalAmount
    {
        get;
        set;
    }
}
