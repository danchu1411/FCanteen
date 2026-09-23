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
            .FirstOrDefaultAsync(
                x =>
                    x.MenuItemId ==
                    menuItemId,
                cancellationToken);
    }
}