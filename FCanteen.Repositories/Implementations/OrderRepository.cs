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

public class OrderRepository
    : IOrderRepository
{
    private readonly FCanteenContext _db;

    public OrderRepository(
        FCanteenContext db)
    {
        _db = db;
    }

    public async Task<OrderTicket?>
        GetByIdAsync(
            int orderTicketId,
            CancellationToken cancellationToken =
                default)
    {
        return await _db.OrderTickets
            .AsNoTracking()
            .Include(x =>
                x.TicketLines)
            .ThenInclude(x =>
                x.MenuItem)
            .FirstOrDefaultAsync(
                x =>
                    x.OrderTicketId ==
                    orderTicketId,
                cancellationToken);
    }

    public async Task AddAsync(
        OrderTicket order,
        CancellationToken cancellationToken =
            default)
    {
        _db.OrderTickets.Add(
            order);

        await _db.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<int>
        CountByDateAsync(
            DateTime date,
            string branchCode,
            CancellationToken cancellationToken =
                default)
    {
        var start =
            date.Date;

        var end =
            start.AddDays(1);

        return await _db.OrderTickets
            .AsNoTracking()
            .CountAsync(
                x =>
                    x.BranchCode ==
                        branchCode &&
                    x.CreatedAt >= start &&
                    x.CreatedAt < end,
                cancellationToken);
    }

    public async Task<decimal>
        GetRevenueByDateAsync(
            DateTime date,
            string branchCode,
            CancellationToken cancellationToken =
                default)
    {
        var start =
            date.Date;

        var end =
            start.AddDays(1);

        var revenue =
            await _db.OrderTickets
                .AsNoTracking()
                .Where(x =>
                    x.BranchCode ==
                        branchCode &&
                    x.CreatedAt >= start &&
                    x.CreatedAt < end)
                .SumAsync(
                    x =>
                        (decimal?)
                        x.TotalAmount,
                    cancellationToken);

        return revenue ?? 0m;
    }
}
