using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;

namespace FCanteen.Services.Interfaces;

public interface IOrderService
{
    Task<OrderTicket?>
        GetOrderAsync(
            int orderTicketId,
            CancellationToken cancellationToken =
                default);

    Task<IReadOnlyList<MenuItem>>
        GetAvailableMenuAsync(
            CancellationToken cancellationToken =
                default);
}
