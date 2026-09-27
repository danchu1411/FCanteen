using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces;

public interface IMenuItemRepository
{
    Task<IReadOnlyList<MenuItem>>
        GetAllAsync(
            CancellationToken cancellationToken =
                default);

    Task<IReadOnlyList<MenuItem>>
        GetAvailableAsync(
            CancellationToken cancellationToken =
                default);

    Task<MenuItem?>
        GetByIdAsync(
            int menuItemId,
            CancellationToken cancellationToken =
                default);

    Task AddAsync(
        MenuItem menuItem,
        CancellationToken cancellationToken =
            default);

    Task UpdateAsync(
        MenuItem menuItem,
        CancellationToken cancellationToken =
            default);

    Task<bool> DeleteAsync(
        int menuItemId,
        CancellationToken cancellationToken =
            default);

    Task<bool> CodeExistsAsync(
        string code,
        int? excludeMenuItemId = null,
        CancellationToken cancellationToken =
            default);

    Task<decimal> CalculateCostAsync(
        int menuItemId,
        CancellationToken cancellationToken =
            default);
}
