using FCanteen.Blazor.Models;
using FCanteen.Data;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Blazor.Services;

public class MenuAdminService
{
    private readonly IDbContextFactory<FCanteenContext> _dbFactory;

    public MenuAdminService(
        IDbContextFactory<FCanteenContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<MenuItem>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync(
                cancellationToken);

        return await db.MenuItems
            .AsNoTracking()
            .Include(x => x.Category)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync(
                cancellationToken);

        return await db.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuItem?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync(
                cancellationToken);

        return await db.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.MenuItemId == id,
                cancellationToken);
    }

    public async Task<(bool Success, string? Error)>
        SaveAsync(
            MenuItemFormModel model,
            CancellationToken cancellationToken = default)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync(
                cancellationToken);

        var code = model.Code.Trim();

        var duplicateCode =
            await db.MenuItems.AnyAsync(
                x =>
                    x.Code == code &&
                    x.MenuItemId != model.MenuItemId,
                cancellationToken);

        if (duplicateCode)
        {
            return (
                false,
                "Mã món đã tồn tại.");
        }

        if (model.MenuItemId == 0)
        {
            var item = new MenuItem
            {
                Code = code,
                Name = model.Name.Trim(),
                Price = model.Price,
                Unit = model.Unit.Trim(),
                IsAvailable = model.IsAvailable,
                CategoryId = model.CategoryId
            };

            db.MenuItems.Add(item);
        }
        else
        {
            var item =
                await db.MenuItems.FirstOrDefaultAsync(
                    x =>
                        x.MenuItemId ==
                        model.MenuItemId,
                    cancellationToken);

            if (item is null)
            {
                return (
                    false,
                    "Không tìm thấy món ăn.");
            }

            item.Code = code;
            item.Name = model.Name.Trim();
            item.Price = model.Price;
            item.Unit = model.Unit.Trim();
            item.IsAvailable = model.IsAvailable;
            item.CategoryId = model.CategoryId;
        }

        await db.SaveChangesAsync(
            cancellationToken);

        return (true, null);
    }

    public async Task<bool> ToggleAvailabilityAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync(
                cancellationToken);

        var item =
            await db.MenuItems.FirstOrDefaultAsync(
                x => x.MenuItemId == id,
                cancellationToken);

        if (item is null)
        {
            return false;
        }

        item.IsAvailable =
            !item.IsAvailable;

        await db.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<(bool Success, string? Error)>
        DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync(
                cancellationToken);

        var item =
            await db.MenuItems.FirstOrDefaultAsync(
                x => x.MenuItemId == id,
                cancellationToken);

        if (item is null)
        {
            return (
                false,
                "Không tìm thấy món ăn.");
        }

        var usedInOrder =
            await db.TicketLines.AnyAsync(
                x => x.MenuItemId == id,
                cancellationToken);

        if (usedInOrder)
        {
            return (
                false,
                "Không thể xoá vì món đã xuất hiện trong đơn hàng.");
        }

        db.MenuItems.Remove(item);

        await db.SaveChangesAsync(
            cancellationToken);

        return (true, null);
    }
}