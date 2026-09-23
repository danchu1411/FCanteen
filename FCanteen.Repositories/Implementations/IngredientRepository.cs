using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Repositories.Implementations;

public class IngredientRepository
    : IIngredientRepository
{
    private readonly FCanteenContext _db;

    public IngredientRepository(
        FCanteenContext db)
    {
        _db = db;
    }

    public async Task<
        IReadOnlyList<Ingredient>>
        GetAllAsync(
            CancellationToken cancellationToken =
                default)
    {
        return await _db.Ingredients
            .AsNoTracking()
            .OrderBy(x =>
                x.IngredientId)
            .ToListAsync(
                cancellationToken);
    }

    public async Task<Ingredient?>
        GetByIdAsync(
            int ingredientId,
            CancellationToken cancellationToken =
                default)
    {
        return await _db.Ingredients
            .FirstOrDefaultAsync(
                x =>
                    x.IngredientId ==
                    ingredientId,
                cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken =
            default)
    {
        await _db.SaveChangesAsync(
            cancellationToken);
    }
}