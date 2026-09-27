using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace FCanteen.Data.Seeders;

public static class Lab04DemoDataSeeder
{
    public static async Task SeedAsync(
        FCanteenContext db,
        CancellationToken cancellationToken =
            default)
    {
        var rice =
            await GetOrCreateIngredientAsync(
                db,
                "Gạo",
                "kg",
                20_000m,
                1,
                cancellationToken);

        var chicken =
            await GetOrCreateIngredientAsync(
                db,
                "Thịt gà",
                "kg",
                80_000m,
                1,
                cancellationToken);

        var vegetable =
            await GetOrCreateIngredientAsync(
                db,
                "Rau xanh",
                "kg",
                30_000m,
                2,
                cancellationToken);

        await db.SaveChangesAsync(
            cancellationToken);

        var hasRecipe =
            await db.MenuItemIngredients
                .AnyAsync(
                    x =>
                        x.MenuItemId == 1,
                    cancellationToken);

        if (!hasRecipe)
        {
            db.MenuItemIngredients.AddRange(
                new MenuItemIngredient
                {
                    MenuItemId = 1,
                    IngredientId =
                        rice.IngredientId,
                    Quantity = 0.20m
                },
                new MenuItemIngredient
                {
                    MenuItemId = 1,
                    IngredientId =
                        chicken.IngredientId,
                    Quantity = 0.25m
                },
                new MenuItemIngredient
                {
                    MenuItemId = 1,
                    IngredientId =
                        vegetable.IngredientId,
                    Quantity = 0.05m
                });

            await db.SaveChangesAsync(
                cancellationToken);
        }
    }

    private static async Task<Ingredient>
        GetOrCreateIngredientAsync(
            FCanteenContext db,
            string name,
            string unit,
            decimal unitCost,
            int supplierId,
            CancellationToken cancellationToken)
    {
        var ingredient =
            await db.Ingredients
                .FirstOrDefaultAsync(
                    x =>
                        x.Name == name,
                    cancellationToken);

        if (ingredient is not null)
        {
            ingredient.UnitCost =
                unitCost;

            ingredient.SupplierId =
                supplierId;

            return ingredient;
        }

        ingredient =
            new Ingredient
            {
                Name = name,
                Unit = unit,
                StockQuantity = 100m,
                AlertThreshold = 10m,
                UnitCost = unitCost,
                SupplierId = supplierId
            };

        db.Ingredients.Add(
            ingredient);

        return ingredient;
    }
}
