using FCanteen.Data;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Blazor.Services;

public class OrderStatusService
{
    private readonly IDbContextFactory<FCanteenContext> _dbFactory;

    public OrderStatusService(
        IDbContextFactory<FCanteenContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<bool> ChangeStatusAsync(
        int orderId,
        string newStatus,
        string changedBy,
        CancellationToken cancellationToken = default)
    {
        if (newStatus is not (
            OrderStatuses.Waiting or
            OrderStatuses.Preparing or
            OrderStatuses.Ready or
            OrderStatuses.Delivered or
            OrderStatuses.Cancelled))
        {
            throw new ArgumentException(
                "Trạng thái đơn không hợp lệ.",
                nameof(newStatus));
        }

        await using var db =
            await _dbFactory.CreateDbContextAsync(
                cancellationToken);

        var order =
            await db.OrderTickets
                .FirstOrDefaultAsync(
                    x => x.OrderTicketId == orderId,
                    cancellationToken);

        if (order is null)
        {
            return false;
        }

        var oldStatus = order.Status;

        if (oldStatus == newStatus)
        {
            return false;
        }

        order.Status = newStatus;

        db.OrderStatusHistories.Add(
            new OrderStatusHistory
            {
                OrderTicketId = order.OrderTicketId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = changedBy,
                ChangedAt = DateTime.Now
            });

        await db.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
