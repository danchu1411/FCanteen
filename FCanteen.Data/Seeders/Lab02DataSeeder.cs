using System.Diagnostics;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Data.Seeders;

public static class Lab02DataSeeder
{
    private const int OrderCount = 50_000;
    private const int LinesPerOrder = 4;

    // 50,000 x 4 = 200,000 TicketLine
    private const int BatchSize = 1_000;

    private static readonly string[] BranchCodes =
    [
        "BR01",
        "BR02",
        "BR03"
    ];

    private static readonly string[] CounterNames =
    [
        "QUAY01",
        "QUAY02",
        "QUAY03"
    ];

    public static async Task SeedAsync(
        FCanteenContext db,
        CancellationToken cancellationToken = default)
    {
        var currentOrderCount =
            await db.OrderTickets
                .CountAsync(cancellationToken);

        var currentLineCount =
            await db.TicketLines
                .CountAsync(cancellationToken);

        Console.WriteLine(
            $"Current orders     : {currentOrderCount:N0}");

        Console.WriteLine(
            $"Current ticketlines: {currentLineCount:N0}");

        // Tránh seed lại khi dataset đã đủ lớn.
        if (currentOrderCount >= 50_000 &&
            currentLineCount >= 200_000)
        {
            Console.WriteLine(
                "Large Lab02 dataset already exists. Seeder skipped.");

            return;
        }

        var menuItems =
            await db.MenuItems
                .AsNoTracking()
                .OrderBy(x => x.MenuItemId)
                .ToListAsync(cancellationToken);

        if (menuItems.Count < LinesPerOrder)
        {
            throw new InvalidOperationException(
                "Not enough MenuItems to generate TicketLines.");
        }

        Console.WriteLine();
        Console.WriteLine(
            "========== LAB02 LARGE DATA SEEDER ==========");

        Console.WriteLine(
            $"Orders to generate : {OrderCount:N0}");

        Console.WriteLine(
            $"Lines per order     : {LinesPerOrder}");

        Console.WriteLine(
            $"Expected lines      : " +
            $"{OrderCount * LinesPerOrder:N0}");

        Console.WriteLine(
            $"Batch size          : {BatchSize:N0}");

        Console.WriteLine(
            "Branches            : BR01, BR02, BR03");

        Console.WriteLine(
            "Peak time           : 11:00 - 12:59");

        var stopwatch =
            Stopwatch.StartNew();

        var random =
            new Random(20260915);

        var originalAutoDetectChanges =
            db.ChangeTracker
                .AutoDetectChangesEnabled;

        // BẮT BUỘC THEO ĐỀ.
        db.ChangeTracker
            .AutoDetectChangesEnabled = false;

        try
        {
            for (var batchStart = 0;
                 batchStart < OrderCount;
                 batchStart += BatchSize)
            {
                cancellationToken
                    .ThrowIfCancellationRequested();

                var currentBatchSize =
                    Math.Min(
                        BatchSize,
                        OrderCount - batchStart);

                var orders =
                    new List<OrderTicket>(
                        currentBatchSize);

                for (var batchIndex = 0;
                     batchIndex < currentBatchSize;
                     batchIndex++)
                {
                    var globalIndex =
                        batchStart +
                        batchIndex;

                    var branchCode =
                        BranchCodes[
                            globalIndex %
                            BranchCodes.Length];

                    var createdAt =
                        GenerateCreatedAt(
                            random);

                    var ticket =
                        new OrderTicket
                        {
                            CounterName =
                                CounterNames[
                                    random.Next(
                                        CounterNames.Length)],

                            BranchCode =
                                branchCode,

                            CreatedAt =
                                createdAt,

                            Status =
                                "Completed"
                        };

                    var selectedIndexes =
                        GetUniqueMenuIndexes(
                            random,
                            menuItems.Count,
                            LinesPerOrder);

                    decimal totalAmount = 0;

                    foreach (var menuIndex
                             in selectedIndexes)
                    {
                        var menuItem =
                            menuItems[menuIndex];

                        var quantity =
                            random.Next(1, 4);

                        var line =
                            new TicketLine
                            {
                                MenuItemId =
                                    menuItem.MenuItemId,

                                Quantity =
                                    quantity,

                                UnitPrice =
                                    menuItem.Price,

                                Note =
                                    null
                            };

                        ticket.TicketLines.Add(
                            line);

                        totalAmount +=
                            menuItem.Price *
                            quantity;
                    }

                    ticket.TotalAmount =
                        totalAmount;

                    orders.Add(ticket);
                }

                // Add theo lô.
                await db.OrderTickets
                    .AddRangeAsync(
                        orders,
                        cancellationToken);

                await db.SaveChangesAsync(
                    cancellationToken);

                // Giải phóng các entity đã track của batch vừa lưu.
                db.ChangeTracker.Clear();

                var generated =
                    batchStart +
                    currentBatchSize;

                Console.WriteLine(
                    $"Generated " +
                    $"{generated,6:N0} / " +
                    $"{OrderCount:N0} orders " +
                    $"({generated * LinesPerOrder:N0} lines)");
            }
        }
        finally
        {
            db.ChangeTracker
                .AutoDetectChangesEnabled =
                originalAutoDetectChanges;
        }

        stopwatch.Stop();

        Console.WriteLine();
        Console.WriteLine(
            "============= SEED COMPLETE =============");

        Console.WriteLine(
            $"Generated orders: {OrderCount:N0}");

        Console.WriteLine(
            $"Generated lines : " +
            $"{OrderCount * LinesPerOrder:N0}");

        Console.WriteLine(
            $"Elapsed time    : " +
            $"{stopwatch.ElapsedMilliseconds:N0} ms");

        Console.WriteLine(
            $"Elapsed seconds : " +
            $"{stopwatch.Elapsed.TotalSeconds:N2} s");

        Console.WriteLine(
            "=========================================");
    }

    private static DateTime GenerateCreatedAt(
        Random random)
    {
        var endDate =
            DateTime.Today.AddDays(-1);

        var startDate =
            endDate.AddMonths(-6);

        var totalDays =
            (endDate - startDate).Days;

        // Chọn ngày đều trên khoảng 6 tháng.
        var date =
            startDate.AddDays(
                random.Next(
                    totalDays + 1));

        int hour;

        // Đề chỉ nói "tập trung 11h-13h",
        // không quy định tỷ lệ.
        // Ta chọn 70% order thuộc giờ cao điểm.
        var isPeakHour =
            random.NextDouble() < 0.70;

        if (isPeakHour)
        {
            // Random.Next(11, 13)
            // => 11 hoặc 12
            // tức 11:00 đến 12:59.
            hour =
                random.Next(11, 13);
        }
        else
        {
            // Các giờ hoạt động ngoài peak.
            do
            {
                hour =
                    random.Next(7, 21);
            }
            while (hour is 11 or 12);
        }

        var minute =
            random.Next(0, 60);

        var second =
            random.Next(0, 60);

        return date.Date
            .AddHours(hour)
            .AddMinutes(minute)
            .AddSeconds(second);
    }

    private static List<int>
        GetUniqueMenuIndexes(
            Random random,
            int menuCount,
            int count)
    {
        var indexes =
            new HashSet<int>();

        while (indexes.Count < count)
        {
            indexes.Add(
                random.Next(menuCount));
        }

        return indexes.ToList();
    }
}