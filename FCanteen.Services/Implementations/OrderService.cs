using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Discounts;
using FCanteen.Services.Interfaces;
using FCanteen.Services.Models.Discounts;
using FCanteen.Services.Notifications;

using System.Text;
using FCanteen.Services.Auditing;
using FCanteen.Services.Contexts;
using FCanteen.Services.Reporting;

namespace FCanteen.Services.Implementations;

public class OrderService
    : IOrderService
{
    private readonly IOrderRepository
        _orderRepository;

    private readonly IMenuItemRepository
        _menuItemRepository;

    private readonly IReadOnlyList<IDiscountPolicy>
        _discountPolicies;

    private readonly IDiscountPolicyLogRepository
        _discountPolicyLogRepository;

    private readonly INotificationService
        _notificationService;

    public IAuditLogger? AuditLogger
    {
        get;
        set;
    }

    public OrderService(
        IOrderRepository orderRepository,
        IMenuItemRepository menuItemRepository,
        IEnumerable<IDiscountPolicy> discountPolicies,
        IDiscountPolicyLogRepository
            discountPolicyLogRepository,
        INotificationService notificationService)
    {
        _orderRepository =
            orderRepository;

        _menuItemRepository =
            menuItemRepository;

        _discountPolicies =
            discountPolicies
                .OrderBy(
                    x => x.Priority)
                .ToArray();

        _discountPolicyLogRepository =
            discountPolicyLogRepository;

        _notificationService =
            notificationService;
    }

    public Task<OrderTicket?>
        GetOrderAsync(
            int orderTicketId,
            CancellationToken cancellationToken =
                default)
    {
        return _orderRepository
            .GetByIdAsync(
                orderTicketId,
                cancellationToken);
    }

    public Task<IReadOnlyList<MenuItem>>
        GetAvailableMenuAsync(
            CancellationToken cancellationToken =
                default)
    {
        return _menuItemRepository
            .GetAvailableAsync(
                cancellationToken);
    }

    public async Task<DiscountCalculationResult>
        CalculateDiscountAsync(
            DiscountRequest request,
            CancellationToken cancellationToken =
                default)
    {
        if (request.Items.Count == 0)
        {
            return new DiscountCalculationResult
            {
                Subtotal = 0,
                TotalDiscount = 0,
                FinalTotal = 0
            };
        }

        var subtotal =
            request.Subtotal;

        var context =
            new DiscountContext
            {
                CustomerType =
                    request.CustomerType,

                OrderTime =
                    request.OrderTime,

                Items =
                    request.Items,

                OriginalSubtotal =
                    subtotal,

                CurrentTotal =
                    subtotal
            };

        var appliedPolicies =
            new List<AppliedDiscountResult>();

        var logs =
            new List<DiscountPolicyLog>();

        foreach (var policy
                 in _discountPolicies)
        {
            if (!policy.CanApply(
                    context))
            {
                continue;
            }

            var amountBefore =
                context.CurrentTotal;

            var calculatedDiscount =
                policy.CalculateDiscount(
                    context);

            /*
             * Policy không được làm hóa đơn âm.
             */
            var discountAmount =
                Math.Clamp(
                    calculatedDiscount,
                    0m,
                    amountBefore);

            if (discountAmount <= 0)
            {
                continue;
            }

            context.CurrentTotal -=
                discountAmount;

            appliedPolicies.Add(
                new AppliedDiscountResult
                {
                    PolicyName =
                        policy.Name,

                    Priority =
                        policy.Priority,

                    AmountBefore =
                        amountBefore,

                    DiscountAmount =
                        discountAmount,

                    AmountAfter =
                        context.CurrentTotal
                });

            logs.Add(
                new DiscountPolicyLog
                {
                    PolicyName =
                        policy.Name,

                    Priority =
                        policy.Priority,

                    CustomerType =
                        request.CustomerType
                            .ToString(),

                    OrderTime =
                        request.OrderTime,

                    AmountBefore =
                        amountBefore,

                    DiscountAmount =
                        discountAmount,

                    AmountAfter =
                        context.CurrentTotal,

                    AppliedAt =
                        DateTime.Now
                });
        }

        if (logs.Count > 0)
        {
            await _discountPolicyLogRepository
                .AddRangeAsync(
                    logs,
                    cancellationToken);
        }

        var result =
        new DiscountCalculationResult
        {
            Subtotal =
                subtotal,

            TotalDiscount =
                subtotal -
                context.CurrentTotal,

            FinalTotal =
                context.CurrentTotal,

            AppliedPolicies =
                appliedPolicies
        };

        await _notificationService
            .SendAsync(
                "FCanteen Order Discount Result",
                $"Customer={request.CustomerType}; " +
                $"Subtotal={result.Subtotal:N0} VND; " +
                $"Discount={result.TotalDiscount:N0} VND; " +
                $"Final={result.FinalTotal:N0} VND",
                cancellationToken);

        return result;
    }

    public async Task<bool>
    ExportOrderReportAsync(
        int orderTicketId,
        IReportExporter reportExporter,
        CancellationToken cancellationToken =
            default)
    {
        ArgumentNullException.ThrowIfNull(
            reportExporter);

        var order =
            await _orderRepository
                .GetByIdAsync(
                    orderTicketId,
                    cancellationToken);

        if (order is null)
        {
            return false;
        }

        /*
         * AMBIENT CONTEXT:
         * Không truyền Staff vào method.
         */
        var currentStaff =
            StaffAmbientContext.Current;

        var staffDescription =
            currentStaff is null
                ? "NOT SET"
                : $"{currentStaff.StaffCode} - " +
                  $"{currentStaff.FullName} " +
                  $"({currentStaff.Role}, " +
                  $"{currentStaff.BranchCode})";

        var report =
            new StringBuilder();

        report.AppendLine(
            $"Order Ticket ID : " +
            $"{order.OrderTicketId}");

        report.AppendLine(
            $"Branch          : " +
            $"{order.BranchCode}");

        report.AppendLine(
            $"Counter         : " +
            $"{order.CounterName}");

        report.AppendLine(
            $"Created At      : " +
            $"{order.CreatedAt:yyyy-MM-dd HH:mm:ss}");

        report.AppendLine(
            $"Total Amount    : " +
            $"{order.TotalAmount:N0} VND");

        report.AppendLine(
            $"Current Staff   : " +
            $"{staffDescription}");

        report.AppendLine();

        report.AppendLine(
            "Ticket Lines:");

        foreach (var line
                 in order.TicketLines)
        {
            report.AppendLine(
                $"- MenuItemId={line.MenuItemId}; " +
                $"Qty={line.Quantity}; " +
                $"UnitPrice={line.UnitPrice:N0}; " +
                $"LineTotal=" +
                $"{line.UnitPrice * line.Quantity:N0}");
        }

        /*
         * METHOD INJECTION:
         * Service chỉ được truyền vào method này.
         */
        await reportExporter
            .ExportAsync(
                $"Order #{order.OrderTicketId}",
                report.ToString(),
                cancellationToken);

        /*
         * PROPERTY INJECTION:
         * Logger optional nên dùng ?.
         */
        if (AuditLogger is not null)
        {
            await AuditLogger
                .LogAsync(
                    "EXPORT_ORDER_REPORT",
                    $"Exported order " +
                    $"#{order.OrderTicketId}",
                    cancellationToken);
        }

        return true;
    }
}
