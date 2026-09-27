
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Models;

public class PurchaseOrderLineRequest
{
    public int IngredientId
    {
        get;
        set;
    }

    public decimal Quantity
    {
        get;
        set;
    }
}
