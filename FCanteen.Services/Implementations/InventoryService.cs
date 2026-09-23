using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Implementations;

public class InventoryService
    : IInventoryService
{
    private readonly IIngredientRepository
        _ingredientRepository;

    public InventoryService(
        IIngredientRepository ingredientRepository)
    {
        _ingredientRepository =
            ingredientRepository;
    }

    public Task<IReadOnlyList<Ingredient>>
        GetIngredientsAsync(
            CancellationToken cancellationToken =
                default)
    {
        return _ingredientRepository
            .GetAllAsync(
                cancellationToken);
    }

    public Task<Ingredient?>
        GetIngredientAsync(
            int ingredientId,
            CancellationToken cancellationToken =
                default)
    {
        return _ingredientRepository
            .GetByIdAsync(
                ingredientId,
                cancellationToken);
    }
}