using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Interfaces;
using FCanteen.Services.Models;

namespace FCanteen.Services.Implementations;

public class ReportService
    : IReportService
{
    private readonly IOrderRepository
        _orderRepository;

    public ReportService(
        IOrderRepository orderRepository)
    {
        _orderRepository =
            orderRepository;
    }

    public async Task<DailySummaryResult>
        GetDailySummaryAsync(
            DateTime date,
            string branchCode,
            CancellationToken cancellationToken =
                default)
    {
        var totalTickets =
            await _orderRepository
                .CountByDateAsync(
                    date,
                    branchCode,
                    cancellationToken);

        var revenue =
            await _orderRepository
                .GetRevenueByDateAsync(
                    date,
                    branchCode,
                    cancellationToken);

        return new DailySummaryResult
        {
            Date =
                date.Date,

            BranchCode =
                branchCode,

            TotalTickets =
                totalTickets,

            TotalRevenue =
                revenue
        };
    }
}
