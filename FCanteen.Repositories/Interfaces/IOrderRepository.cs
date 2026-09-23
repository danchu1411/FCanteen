using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<OrderTicket?>
        GetByIdAsync(
            int orderTicketId,
            CancellationToken cancellationToken =
                default);

    Task AddAsync(
        OrderTicket order,
        CancellationToken cancellationToken =
            default);

    Task<int> CountByDateAsync(
        DateTime date,
        string branchCode,
        CancellationToken cancellationToken =
            default);

    Task<decimal> GetRevenueByDateAsync(
        DateTime date,
        string branchCode,
        CancellationToken cancellationToken =
            default);
}
