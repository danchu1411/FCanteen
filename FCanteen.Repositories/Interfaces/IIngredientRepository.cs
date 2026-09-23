using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces;

public interface IIngredientRepository
{
    Task<IReadOnlyList<Ingredient>>
        GetAllAsync(
            CancellationToken cancellationToken =
                default);

    Task<Ingredient?>
        GetByIdAsync(
            int ingredientId,
            CancellationToken cancellationToken =
                default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken =
            default);
}
