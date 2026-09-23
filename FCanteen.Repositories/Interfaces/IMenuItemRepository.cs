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
}
