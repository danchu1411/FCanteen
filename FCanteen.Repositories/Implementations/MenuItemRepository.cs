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

public class MenuItemRepository
    : IMenuItemRepository
{
    private readonly FCanteenContext _db;

    public MenuItemRepository(
        FCanteenContext db)
    {
        _db = db;
    }

    public async Task<
        IReadOnlyList<MenuItem>>
        GetAllAsync(
            CancellationToken cancellationToken =
                default)
    {
        return await _db.MenuItems
            .AsNoTracking()
            .Include(x => x.Category)
            .OrderBy(x => x.MenuItemId)
            .ToListAsync(
                cancellationToken);
    }

    public async Task<
        IReadOnlyList<MenuItem>>
        GetAvailableAsync(
            CancellationToken cancellationToken =
                default)
    {
        return await _db.MenuItems
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x =>
                x.IsAvailable)
            .OrderBy(x =>
                x.MenuItemId)
            .ToListAsync(
                cancellationToken);
    }

    public async Task<MenuItem?>
        GetByIdAsync(
            int menuItemId,
            CancellationToken cancellationToken =
                default)
    {
        return await _db.MenuItems
            .AsNoTracking()
            .Include(x =>
                x.Category)
            .Include(x =>
                x.MenuItemIngredients)
            .ThenInclude(x =>
                x.Ingredient)
            .FirstOrDefaultAsync(
                x =>
                    x.MenuItemId ==
                    menuItemId,
                cancellationToken);
    }

    public async Task AddAsync(
        MenuItem menuItem,
        CancellationToken cancellationToken =
            default)
    {
        _db.MenuItems.Add(
            menuItem);

        await _db.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UpdateAsync(
        MenuItem menuItem,
        CancellationToken cancellationToken =
            default)
    {
        _db.MenuItems.Update(
            menuItem);

        await _db.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        int menuItemId,
        CancellationToken cancellationToken =
            default)
    {
        var menuItem =
            await _db.MenuItems
                .FirstOrDefaultAsync(
                    x =>
                        x.MenuItemId ==
                        menuItemId,
                    cancellationToken);

        if (menuItem is null)
        {
            return false;
        }

        _db.MenuItems.Remove(
            menuItem);

        await _db.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool>
        CodeExistsAsync(
            string code,
            int? excludeMenuItemId = null,
            CancellationToken cancellationToken =
                default)
    {
        var query =
            _db.MenuItems
                .AsNoTracking()
                .Where(x =>
                    x.Code == code);

        if (excludeMenuItemId.HasValue)
        {
            query =
                query.Where(x =>
                    x.MenuItemId !=
                    excludeMenuItemId.Value);
        }

        return await query.AnyAsync(
            cancellationToken);
    }

    public async Task<decimal>
        CalculateCostAsync(
            int menuItemId,
            CancellationToken cancellationToken =
                default)
    {
        var cost =
            await _db.MenuItemIngredients
                .Where(x =>
                    x.MenuItemId ==
                    menuItemId)
                .Select(x =>
                    (decimal?)(
                        x.Quantity *
                        x.Ingredient.UnitCost))
                .SumAsync(
                    cancellationToken);

        return cost ?? 0m;
    }

    public async Task<bool>
    HasTicketLinesAsync(
        int menuItemId,
        CancellationToken cancellationToken =
            default)
    {
        return await _db.TicketLines
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.MenuItemId ==
                    menuItemId,
                cancellationToken);
    }
}