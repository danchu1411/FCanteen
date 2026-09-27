using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using FCanteen.Repositories.Models;

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

    public async Task<PagedResult<MenuItem>>
        SearchAsync(
            string? searchTerm,
            int? categoryId,
            bool? isAvailable,
            string sortBy,
            int page,
            int pageSize,
            CancellationToken cancellationToken =
                default)
    {
        page =
            Math.Max(
                page,
                1);

        pageSize =
            Math.Clamp(
                pageSize,
                1,
                100);

        var query =
            _db.MenuItems
                .AsNoTracking()
                .Include(x =>
                    x.Category)
                .AsQueryable();

        /*
         * SEARCH:
         * tìm cả Name và Code.
         */
        if (!string.IsNullOrWhiteSpace(
            searchTerm))
        {
            var keyword =
                searchTerm.Trim();

            query =
                query.Where(x =>
                    x.Name.Contains(
                        keyword)
                    ||
                    x.Code.Contains(
                        keyword));
        }

        /*
         * FILTER CATEGORY.
         */
        if (categoryId.HasValue)
        {
            query =
                query.Where(x =>
                    x.CategoryId ==
                    categoryId.Value);
        }

        /*
         * FILTER AVAILABILITY.
         *
         * null  = tất cả
         * true  = còn bán
         * false = ngừng bán
         */
        if (isAvailable.HasValue)
        {
            query =
                query.Where(x =>
                    x.IsAvailable ==
                    isAvailable.Value);
        }

        /*
         * Count phải thực hiện
         * SAU filter nhưng TRƯỚC paging.
         */
        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    (double)totalCount /
                    pageSize);

        /*
         * Nếu user nhập thủ công
         * ?page=999 thì đưa về trang cuối.
         */
        if (totalPages > 0 &&
            page > totalPages)
        {
            page =
                totalPages;
        }

        /*
         * SORT.
         */
        query =
            sortBy switch
            {
                "name_desc" =>
                    query
                        .OrderByDescending(
                            x => x.Name)
                        .ThenBy(
                            x => x.MenuItemId),

                "price_asc" =>
                    query
                        .OrderBy(
                            x => x.Price)
                        .ThenBy(
                            x => x.MenuItemId),

                "price_desc" =>
                    query
                        .OrderByDescending(
                            x => x.Price)
                        .ThenBy(
                            x => x.MenuItemId),

                _ =>
                    query
                        .OrderBy(
                            x => x.Name)
                        .ThenBy(
                            x => x.MenuItemId)
            };

        var items =
            await query
                .Skip(
                    (page - 1) *
                    pageSize)
                .Take(
                    pageSize)
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<MenuItem>
        {
            Items =
                items,

            TotalCount =
                totalCount,

            Page =
                page,

            PageSize =
                pageSize
        };
    }

    public async Task ReplaceIngredientsAsync(
    int menuItemId,
    IReadOnlyDictionary<int, decimal> quantities,
    CancellationToken cancellationToken =
        default)
    {
        var existing =
            await _db.MenuItemIngredients
                .Where(x =>
                    x.MenuItemId ==
                    menuItemId)
                .ToListAsync(
                    cancellationToken);

        /*
         * Update hoặc remove những dòng
         * đã tồn tại.
         */
        foreach (var current in existing)
        {
            if (quantities.TryGetValue(
                current.IngredientId,
                out var quantity) &&
                quantity > 0)
            {
                /*
                 * Ingredient vẫn còn trong recipe:
                 * chỉ update quantity,
                 * không tạo entity cùng key mới.
                 */
                current.Quantity =
                    quantity;
            }
            else
            {
                /*
                 * Ingredient đã bị bỏ chọn.
                 */
                _db.MenuItemIngredients
                    .Remove(
                        current);
            }
        }

        /*
         * Chỉ Add những Ingredient chưa có
         * trong recipe cũ.
         */
        var existingIngredientIds =
            existing
                .Select(x =>
                    x.IngredientId)
                .ToHashSet();

        foreach (var pair in quantities)
        {
            if (pair.Value <= 0 ||
                existingIngredientIds.Contains(
                    pair.Key))
            {
                continue;
            }

            _db.MenuItemIngredients.Add(
                new MenuItemIngredient
                {
                    MenuItemId =
                        menuItemId,

                    IngredientId =
                        pair.Key,

                    Quantity =
                        pair.Value
                });
        }

        await _db.SaveChangesAsync(
            cancellationToken);
    }
}