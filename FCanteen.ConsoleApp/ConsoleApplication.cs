using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Services.Models.Discounts;

using FCanteen.Services.Interfaces;

namespace FCanteen.ConsoleApp;

public class ConsoleApplication
{
    private readonly IOrderService
        _orderService;

    private readonly IReportService
        _reportService;

    private readonly IInventoryService
        _inventoryService;

    public ConsoleApplication(
        IOrderService orderService,
        IReportService reportService,
        IInventoryService inventoryService)
    {
        _orderService =
            orderService;

        _reportService =
            reportService;

        _inventoryService =
            inventoryService;
    }

    public async Task RunAsync(
        CancellationToken cancellationToken =
            default)
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine(
                "====================================");

            Console.WriteLine(
                "       FCANTEEN LAB 03 - DI");

            Console.WriteLine(
                "====================================");

            Console.WriteLine(
                "1. Show available menu");

            Console.WriteLine(
                "2. Show ingredients");

            Console.WriteLine(
                "3. Show daily report");

            Console.WriteLine(
                "4. Find order by ID");

            Console.WriteLine(
                 "5. YC2 - Test discount policies");

            Console.WriteLine(
                "0. Exit");

            Console.WriteLine();

            Console.Write(
                "Choose: ");

            var choice =
                Console.ReadLine();

            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await ShowMenuAsync(
                        cancellationToken);
                    break;

                case "2":
                    await ShowIngredientsAsync(
                        cancellationToken);
                    break;

                case "3":
                    await ShowDailyReportAsync(
                        cancellationToken);
                    break;

                case "4":
                    await ShowOrderAsync(
                        cancellationToken);
                    break;

                case "5":
                    await TestDiscountPoliciesAsync(
                        cancellationToken);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine(
                        "Invalid choice.");
                    break;
            }

            Console.WriteLine();

            Console.WriteLine(
                "Press Enter to continue.");

            Console.ReadLine();
        }
    }

    private async Task ShowMenuAsync(
        CancellationToken cancellationToken)
    {
        var menu =
            await _orderService
                .GetAvailableMenuAsync(
                    cancellationToken);

        Console.WriteLine(
            "========== AVAILABLE MENU ==========");

        foreach (var item in menu)
        {
            Console.WriteLine(
                $"{item.MenuItemId,3} | " +
                $"{item.Name,-25} | " +
                $"{item.Price,12:N0} | " +
                $"{item.Unit}");
        }
    }

    private async Task ShowIngredientsAsync(
        CancellationToken cancellationToken)
    {
        var ingredients =
            await _inventoryService
                .GetIngredientsAsync(
                    cancellationToken);

        Console.WriteLine(
            "========== INGREDIENTS ==========");

        if (ingredients.Count == 0)
        {
            Console.WriteLine(
                "No ingredients found.");

            return;
        }

        foreach (var item in ingredients)
        {
            Console.WriteLine(
                $"{item.IngredientId,3} | " +
                $"{item.Name,-20} | " +
                $"{item.StockQuantity,12:N2} " +
                $"{item.Unit}");
        }
    }

    private async Task ShowDailyReportAsync(
        CancellationToken cancellationToken)
    {
        Console.Write(
            "Branch code (default BR01): ");

        var branchInput =
            Console.ReadLine();

        var branchCode =
            string.IsNullOrWhiteSpace(
                branchInput)
                ? "BR01"
                : branchInput
                    .Trim()
                    .ToUpperInvariant();

        Console.Write(
            "Date yyyy-MM-dd " +
            "(Enter = today): ");

        var dateInput =
            Console.ReadLine();

        var date =
            DateTime.Today;

        if (!string.IsNullOrWhiteSpace(
                dateInput) &&
            DateTime.TryParse(
                dateInput,
                out var parsedDate))
        {
            date =
                parsedDate.Date;
        }

        var report =
            await _reportService
                .GetDailySummaryAsync(
                    date,
                    branchCode,
                    cancellationToken);

        Console.WriteLine();

        Console.WriteLine(
            $"Date          : " +
            $"{report.Date:yyyy-MM-dd}");

        Console.WriteLine(
            $"Branch        : " +
            $"{report.BranchCode}");

        Console.WriteLine(
            $"Total tickets : " +
            $"{report.TotalTickets:N0}");

        Console.WriteLine(
            $"Total revenue : " +
            $"{report.TotalRevenue:N0}");
    }

    private async Task ShowOrderAsync(
        CancellationToken cancellationToken)
    {
        Console.Write(
            "Order ID: ");

        if (!int.TryParse(
                Console.ReadLine(),
                out var orderId))
        {
            Console.WriteLine(
                "Invalid order ID.");

            return;
        }

        var order =
            await _orderService
                .GetOrderAsync(
                    orderId,
                    cancellationToken);

        if (order is null)
        {
            Console.WriteLine(
                "Order not found.");

            return;
        }

        Console.WriteLine(
            $"Ticket : " +
            $"{order.OrderTicketId}");

        Console.WriteLine(
            $"Branch : " +
            $"{order.BranchCode}");

        Console.WriteLine(
            $"Counter: " +
            $"{order.CounterName}");

        Console.WriteLine(
            $"Total  : " +
            $"{order.TotalAmount:N0}");

        Console.WriteLine(
            $"Time   : " +
            $"{order.CreatedAt:yyyy-MM-dd HH:mm:ss}");
    }

    private async Task TestDiscountPoliciesAsync(
        CancellationToken cancellationToken)
    {
        Console.WriteLine(
            "========== DISCOUNT POLICY DEMO ==========");

        Console.WriteLine();

        Console.WriteLine(
            "Customer type:");

        Console.WriteLine(
            "1. Student");

        Console.WriteLine(
            "2. Lecturer");

        Console.WriteLine(
            "3. Staff");

        Console.WriteLine(
            "4. Regular");

        Console.Write(
            "Choose: ");

        var customerType =
            Console.ReadLine() switch
            {
                "1" =>
                    CustomerType.Student,

                "2" =>
                    CustomerType.Lecturer,

                "3" =>
                    CustomerType.Staff,

                _ =>
                    CustomerType.Regular
            };

        Console.Write(
            "Include drink for combo? (Y/N): ");

        var includeDrink =
            string.Equals(
                Console.ReadLine(),
                "Y",
                StringComparison.OrdinalIgnoreCase);

        Console.Write(
            "Order hour 0-23 " +
            "(Enter = current hour): ");

        var hourInput =
            Console.ReadLine();

        var orderTime =
            DateTime.Now;

        if (int.TryParse(
                hourInput,
                out var hour) &&
            hour is >= 0 and <= 23)
        {
            orderTime =
                DateTime.Today
                    .AddHours(hour);
        }

        var items =
            new List<DiscountOrderItem>
            {
            new()
            {
                Name =
                    "Main Dish Demo",

                UnitPrice =
                    70_000m,

                Quantity =
                    1,

                Category =
                    DiscountItemCategory
                        .MainDish
            }
            };

        if (includeDrink)
        {
            items.Add(
                new DiscountOrderItem
                {
                    Name =
                        "Drink Demo",

                    UnitPrice =
                        30_000m,

                    Quantity =
                        1,

                    Category =
                        DiscountItemCategory
                            .Drink
                });
        }

        var request =
            new DiscountRequest
            {
                CustomerType =
                    customerType,

                OrderTime =
                    orderTime,

                Items =
                    items
            };

        var result =
            await _orderService
                .CalculateDiscountAsync(
                    request,
                    cancellationToken);

        Console.WriteLine();
        Console.WriteLine(
            $"Customer       : " +
            $"{customerType}");

        Console.WriteLine(
            $"Order time     : " +
            $"{orderTime:HH:mm}");

        Console.WriteLine(
            $"Subtotal       : " +
            $"{result.Subtotal:N0} VND");

        Console.WriteLine();

        Console.WriteLine(
            "Applied policies:");

        if (result.AppliedPolicies.Count == 0)
        {
            Console.WriteLine(
                "None.");
        }
        else
        {
            foreach (var policy
                     in result.AppliedPolicies)
            {
                Console.WriteLine(
                    $"Priority " +
                    $"{policy.Priority,3} | " +
                    $"{policy.PolicyName,-30} | " +
                    $"{policy.AmountBefore,10:N0}" +
                    $" -> -" +
                    $"{policy.DiscountAmount,8:N0}" +
                    $" -> " +
                    $"{policy.AmountAfter,10:N0}");
            }
        }

        Console.WriteLine();

        Console.WriteLine(
            $"Total discount : " +
            $"{result.TotalDiscount:N0} VND");

        Console.WriteLine(
            $"Final total    : " +
            $"{result.FinalTotal:N0} VND");

        Console.WriteLine(
            "==========================================");
    }
}
