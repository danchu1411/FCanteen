using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;
using FCanteen.Services.Models.Discounts;
using FCanteen.Services.Reporting;

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

    Task<DiscountCalculationResult>
        CalculateDiscountAsync(
            DiscountRequest request,
            CancellationToken cancellationToken =
                default);

    Task<bool> ExportOrderReportAsync(
        int orderTicketId,
        IReportExporter reportExporter,
        CancellationToken cancellationToken =
            default);
}
