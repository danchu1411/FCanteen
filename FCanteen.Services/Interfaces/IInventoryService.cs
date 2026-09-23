using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;

namespace FCanteen.Services.Interfaces;

public interface IInventoryService
{
    Task<IReadOnlyList<Ingredient>>
        GetIngredientsAsync(
            CancellationToken cancellationToken =
                default);

    Task<Ingredient?>
        GetIngredientAsync(
            int ingredientId,
            CancellationToken cancellationToken =
                default);
}
