using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class IngredientOrderWorkItem
{
    public int OrderNumber { get; set; }

    public decimal QuantityToConsume { get; set; }
}
