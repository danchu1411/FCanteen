using System.Diagnostics;
using System.Runtime.CompilerServices;
using FCanteen.Analytics.Models;
using FCanteen.Data;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Analytics.Services;

public class AsyncDailyReportService
{
    private readonly string _connectionString;

    public AsyncDailyReportService(
        string connectionString)
    {
        _connectionString =
            connectionString;
    }

    private FCanteenContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<FCanteenContext>()
                .UseSqlServer(_connectionString)
                .Options;

        return new FCanteenContext(options);
    }

    private async Task<int> GetTotalTicketsAsync(
        DateTime date,
        string branchCode,
        CancellationToken cancellationToken)
    {
        await using var db =
            CreateDbContext();

        var start =
            date.Date;

        var end =
            start.AddDays(1);

        return await db.OrderTickets
            .AsNoTracking()
            .CountAsync(
                x =>
                    x.BranchCode == branchCode &&
                    x.CreatedAt >= start &&
                    x.CreatedAt < end,
                cancellationToken);
    }

    private async Task<decimal> GetTotalRevenueAsync(
        DateTime date,
        string branchCode,
        CancellationToken cancellationToken)
    {
        await using var db =
            CreateDbContext();

        var start =
            date.Date;

        var end =
            start.AddDays(1);

        var revenue =
            await db.OrderTickets
                .AsNoTracking()
                .Where(x =>
                    x.BranchCode == branchCode &&
                    x.CreatedAt >= start &&
                    x.CreatedAt < end)
                .SumAsync(
                    x => (decimal?)x.TotalAmount,
                    cancellationToken);

        return revenue ?? 0m;
    }

    private async Task<TopMenuDailyResult?>
    GetTopMenuAsync(
        DateTime date,
        string branchCode,
        CancellationToken cancellationToken)
    {
        await using var db =
            CreateDbContext();

        var start =
            date.Date;

        var end =
            start.AddDays(1);

        return await db.TicketLines
            .AsNoTracking()
            .Where(x =>
                x.OrderTicket.BranchCode ==
                    branchCode &&
                x.OrderTicket.CreatedAt >=
                    start &&
                x.OrderTicket.CreatedAt <
                    end)
            .GroupBy(x =>
                new
                {
                    x.MenuItemId,
                    x.MenuItem.Name
                })
            .Select(group =>
                new TopMenuDailyResult
                {
                    MenuItemId =
                        group.Key.MenuItemId,

                    MenuItemName =
                        group.Key.Name,

                    Revenue =
                        group.Sum(x =>
                            x.UnitPrice *
                            x.Quantity)
                })
            .OrderByDescending(x =>
                x.Revenue)
            .ThenBy(x =>
                x.MenuItemId)
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    private async Task<PeakHourResult?>
        GetPeakHourAsync(
            DateTime date,
            string branchCode,
            CancellationToken cancellationToken)
    {
        await using var db =
            CreateDbContext();

        var start =
            date.Date;

        var end =
            start.AddDays(1);

        return await db.OrderTickets
            .AsNoTracking()
            .Where(x =>
                x.BranchCode == branchCode &&
                x.CreatedAt >= start &&
                x.CreatedAt < end)
            .GroupBy(x =>
                x.CreatedAt.Hour)
            .Select(group =>
                new PeakHourResult
                {
                    Hour =
                        group.Key,

                    TotalTickets =
                        group.Count(),

                    Revenue =
                        group.Sum(x =>
                            x.TotalAmount)
                })
            .OrderByDescending(x =>
                x.Revenue)
            .ThenBy(x =>
                x.Hour)
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task<DailyReportResult>
        GenerateDailyReportSequentialAsync(
            DateTime date,
            string branchCode,
            CancellationToken cancellationToken)
    {
        var totalTickets =
            await GetTotalTicketsAsync(
                date,
                branchCode,
                cancellationToken);

        var totalRevenue =
            await GetTotalRevenueAsync(
                date,
                branchCode,
                cancellationToken);

        var topMenu =
            await GetTopMenuAsync(
                date,
                branchCode,
                cancellationToken);

        var peakHour =
            await GetPeakHourAsync(
                date,
                branchCode,
                cancellationToken);

        return new DailyReportResult
        {
            Date =
                date.Date,

            BranchCode =
                branchCode,

            TotalTickets =
                totalTickets,

            TotalRevenue =
                totalRevenue,

            TopMenu =
                topMenu,

            PeakHour =
                peakHour
        };
    }

    public async Task<DailyReportResult>
    GenerateDailyReportAsync(
        DateTime date,
        string branchCode,
        CancellationToken cancellationToken)
    {
        /*
         * QUAN TRỌNG:
         * Không await từng task ngay.
         *
         * Khởi động cả 4 trước.
         */

        var totalTicketsTask =
            GetTotalTicketsAsync(
                date,
                branchCode,
                cancellationToken);

        var totalRevenueTask =
            GetTotalRevenueAsync(
                date,
                branchCode,
                cancellationToken);

        var topMenuTask =
            GetTopMenuAsync(
                date,
                branchCode,
                cancellationToken);

        var peakHourTask =
            GetPeakHourAsync(
                date,
                branchCode,
                cancellationToken);

        /*
         * Chờ toàn bộ 4 query hoàn tất.
         */
        await Task.WhenAll(
            totalTicketsTask,
            totalRevenueTask,
            topMenuTask,
            peakHourTask);

        return new DailyReportResult
        {
            Date =
                date.Date,

            BranchCode =
                branchCode,

            TotalTickets =
                await totalTicketsTask,

            TotalRevenue =
                await totalRevenueTask,

            TopMenu =
                await topMenuTask,

            PeakHour =
                await peakHourTask
        };
    }

    public async Task<AsyncReportBenchmarkResult>
        CompareAsync(
            DateTime date,
            string branchCode,
            CancellationToken cancellationToken)
    {
        Console.WriteLine();

        Console.WriteLine(
            "Running SEQUENTIAL awaits...");

        var sequentialWatch =
            Stopwatch.StartNew();

        var sequential =
            await GenerateDailyReportSequentialAsync(
                date,
                branchCode,
                cancellationToken);

        sequentialWatch.Stop();

        Console.WriteLine(
            "Running CONCURRENT Task.WhenAll...");

        var concurrentWatch =
            Stopwatch.StartNew();

        var concurrent =
            await GenerateDailyReportAsync(
                date,
                branchCode,
                cancellationToken);

        concurrentWatch.Stop();

        var sequentialMs =
            sequentialWatch
                .Elapsed
                .TotalMilliseconds;

        var concurrentMs =
            concurrentWatch
                .Elapsed
                .TotalMilliseconds;

        var speedup =
            sequentialMs /
            Math.Max(
                0.0001,
                concurrentMs);

        return new AsyncReportBenchmarkResult
        {
            SequentialReport =
                sequential,

            ConcurrentReport =
                concurrent,

            SequentialMilliseconds =
                sequentialMs,

            ConcurrentMilliseconds =
                concurrentMs,

            Speedup =
                speedup,

            ResultsMatch =
                ReportsMatch(
                    sequential,
                    concurrent)
        };
    }

    private static bool ReportsMatch(
    DailyReportResult first,
    DailyReportResult second)
    {
        if (first.TotalTickets !=
            second.TotalTickets)
        {
            return false;
        }

        if (first.TotalRevenue !=
            second.TotalRevenue)
        {
            return false;
        }

        if (first.TopMenu?.MenuItemId !=
            second.TopMenu?.MenuItemId)
        {
            return false;
        }

        if (first.TopMenu?.Revenue !=
            second.TopMenu?.Revenue)
        {
            return false;
        }

        if (first.PeakHour?.Hour !=
            second.PeakHour?.Hour)
        {
            return false;
        }

        if (first.PeakHour?.Revenue !=
            second.PeakHour?.Revenue)
        {
            return false;
        }

        return true;
    }

    private async Task<DateTime?>
        GetLatestOrderDateAsync(
            string branchCode,
            CancellationToken cancellationToken)
    {
        await using var db =
            CreateDbContext();

        var latest =
            await db.OrderTickets
                .AsNoTracking()
                .Where(x =>
                    x.BranchCode ==
                    branchCode)
                .MaxAsync(
                    x =>
                        (DateTime?)
                        x.CreatedAt,
                    cancellationToken);

        return latest?.Date;
    }

    public async IAsyncEnumerable
    <HighValueOrderResult>
    StreamHighValueOrdersAsync(
        string branchCode,
        decimal minimumAmount,
        [EnumeratorCancellation]
        CancellationToken cancellationToken =
            default)
    {
        await using var db =
            CreateDbContext();

        var query =
            db.OrderTickets
                .AsNoTracking()
                .Where(x =>
                    x.BranchCode ==
                        branchCode &&
                    x.TotalAmount >=
                        minimumAmount)
                .OrderByDescending(x =>
                    x.TotalAmount)
                .Select(x =>
                    new HighValueOrderResult
                    {
                        OrderTicketId =
                            x.OrderTicketId,

                        BranchCode =
                            x.BranchCode,

                        CounterName =
                            x.CounterName,

                        TotalAmount =
                            x.TotalAmount,

                        CreatedAt =
                            x.CreatedAt
                    })
                .AsAsyncEnumerable();

        await foreach (
            var order in query
                .WithCancellation(
                    cancellationToken))
        {
            yield return order;
        }
    }

    private async Task PrintHighValueOrdersAsync(
    string branchCode,
    decimal minimumAmount,
    CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine(
            "====== IAsyncEnumerable HIGH-VALUE ORDERS ======");

        Console.WriteLine(
            $"Branch    : {branchCode}");

        Console.WriteLine(
            $"Threshold : {minimumAmount:N0} VND");

        Console.WriteLine();

        Console.WriteLine(
            $"{"Ticket",-10}" +
            $"{"Counter",-12}" +
            $"{"Amount",18}" +
            $"{"Created",22}");

        Console.WriteLine(
            new string('-', 62));

        var totalCount = 0;

        const int maxRowsToPrint = 20;

        await foreach (
            var order in
                StreamHighValueOrdersAsync(
                    branchCode,
                    minimumAmount,
                    cancellationToken))
        {
            totalCount++;

            /*
             * Duyệt toàn bộ stream để chứng minh
             * không load List, nhưng chỉ print
             * 20 dòng đầu tránh console quá dài.
             */
            if (totalCount <=
                maxRowsToPrint)
            {
                Console.WriteLine(
                    $"{order.OrderTicketId,-10}" +
                    $"{order.CounterName,-12}" +
                    $"{order.TotalAmount,18:N0}" +
                    $"{order.CreatedAt,22:yyyy-MM-dd HH:mm}");
            }
        }

        Console.WriteLine(
            new string('-', 62));

        Console.WriteLine(
            $"Streamed {totalCount:N0} " +
            $"high-value order(s).");

        if (totalCount >
            maxRowsToPrint)
        {
            Console.WriteLine(
                $"Only first " +
                $"{maxRowsToPrint} rows " +
                $"were printed.");
        }
    }

    private static void PrintDailyReport(
    DailyReportResult report)
    {
        Console.WriteLine();

        Console.WriteLine(
            "========== DAILY REPORT ==========");

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
            $"{report.TotalRevenue:N0} VND");

        if (report.TopMenu is not null)
        {
            Console.WriteLine(
                $"Top menu      : " +
                $"{report.TopMenu.MenuItemName}");

            Console.WriteLine(
                $"Top menu rev  : " +
                $"{report.TopMenu.Revenue:N0} VND");
        }

        if (report.PeakHour is not null)
        {
            Console.WriteLine(
                $"Peak hour     : " +
                $"{report.PeakHour.Hour:00}:00");

            Console.WriteLine(
                $"Peak revenue  : " +
                $"{report.PeakHour.Revenue:N0} VND");

            Console.WriteLine(
                $"Peak tickets  : " +
                $"{report.PeakHour.TotalTickets:N0}");
        }

        Console.WriteLine(
            "==================================");
    }

    private static void PrintBenchmark(
    AsyncReportBenchmarkResult result)
    {
        Console.WriteLine();

        Console.WriteLine(
            "=========== YC4 ASYNC BENCHMARK ===========");

        Console.WriteLine(
            $"{"Method",-30}" +
            $"{"Time(ms)",15}");

        Console.WriteLine(
            new string('-', 45));

        Console.WriteLine(
            $"{"Sequential await",-30}" +
            $"{result.SequentialMilliseconds,15:F2}");

        Console.WriteLine(
            $"{"Task.WhenAll",-30}" +
            $"{result.ConcurrentMilliseconds,15:F2}");

        Console.WriteLine(
            new string('-', 45));

        Console.WriteLine(
            $"Speedup       : " +
            $"{result.Speedup:F2}x");

        Console.WriteLine(
            $"Results Match : " +
            $"{(result.ResultsMatch ? "OK" : "FAIL")}");

        Console.WriteLine(
            "===========================================");
    }

    public async Task RunYc4Async(
    string branchCode,
    decimal highValueThreshold,
    CancellationToken cancellationToken)
    {
        /*
         * Nhận CancellationToken từ bên ngoài
         * và kết hợp với timeout 30 giây.
         */
        using var timeoutCts =
            CancellationTokenSource
                .CreateLinkedTokenSource(
                    cancellationToken);

        timeoutCts.CancelAfter(
            TimeSpan.FromSeconds(30));

        var token =
            timeoutCts.Token;

        try
        {
            Console.WriteLine();
            Console.WriteLine(
                "======================================");

            Console.WriteLine(
                " YC4 - ASYNCHRONOUS EF CORE");

            Console.WriteLine(
                "======================================");

            Console.WriteLine(
                "Automatic timeout: 30 seconds");

            Console.WriteLine(
                $"Branch: {branchCode}");

            var latestDate =
                await GetLatestOrderDateAsync(
                    branchCode,
                    token);

            if (latestDate is null)
            {
                Console.WriteLine(
                    $"No orders found for " +
                    $"{branchCode}.");

                return;
            }

            Console.WriteLine(
                $"Report date: " +
                $"{latestDate:yyyy-MM-dd}");

            /*
             * Sequential vs Task.WhenAll.
             */
            var benchmark =
                await CompareAsync(
                    latestDate.Value,
                    branchCode,
                    token);

            PrintDailyReport(
                benchmark.ConcurrentReport);

            PrintBenchmark(
                benchmark);

            /*
             * IAsyncEnumerable.
             */
            await PrintHighValueOrdersAsync(
                branchCode,
                highValueThreshold,
                token);

            Console.WriteLine();
            Console.WriteLine(
                "YC4 completed successfully.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine();
            Console.WriteLine(
                "========== OPERATION CANCELLED ==========");

            if (cancellationToken
                .IsCancellationRequested)
            {
                Console.WriteLine(
                    "Reason: Cancelled by caller/user.");
            }
            else
            {
                Console.WriteLine(
                    "Reason: The operation exceeded " +
                    "the 30-second timeout.");
            }

            Console.WriteLine(
                "Cancellation was handled gracefully.");

            Console.WriteLine(
                "=========================================");
        }
    }
}