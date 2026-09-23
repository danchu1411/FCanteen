using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using FCanteen.Analytics.Models;
using FCanteen.Data;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Analytics.Services;

public class PlinqReportService
{
    private readonly string _connectionString;

    public PlinqReportService(
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

    private async Task<SalesFact[]> LoadDataAsync()
    {
        await using var db =
            CreateDbContext();

        Console.WriteLine(
            "Loading sales data from database...");

        var facts =
            await db.TicketLines
                .AsNoTracking()
                .Select(x =>
                    new SalesFact
                    {
                        MenuItemId =
                            x.MenuItemId,

                        MenuItemName =
                            x.MenuItem.Name,

                        Quantity =
                            x.Quantity,

                        UnitPrice =
                            x.UnitPrice,

                        CreatedAt =
                            x.OrderTicket.CreatedAt,

                        BranchCode =
                            x.OrderTicket.BranchCode
                    })
                .ToArrayAsync();

        Console.WriteLine(
            $"Loaded {facts.Length:N0} TicketLines.");

        return facts;
    }

    private static List<TopMenuRevenueResult>
        Top10MenuSequential(
            SalesFact[] facts)
    {
        return facts
            .GroupBy(x =>
                new
                {
                    x.MenuItemId,
                    x.MenuItemName
                })
            .Select(group =>
                new TopMenuRevenueResult
                {
                    MenuItemId =
                        group.Key.MenuItemId,

                    MenuItemName =
                        group.Key.MenuItemName,

                    TotalQuantity =
                        group.Sum(
                            x => (long)x.Quantity),

                    Revenue =
                        group.Sum(
                            x => x.Revenue)
                })
            .OrderByDescending(
                x => x.Revenue)
            .ThenBy(
                x => x.MenuItemId)
            .Take(10)
            .ToList();
    }

    private static List<TopMenuRevenueResult>
        Top10MenuPlinq(
            SalesFact[] facts)
    {
        return facts
            .AsParallel()
            .GroupBy(x =>
                new
                {
                    x.MenuItemId,
                    x.MenuItemName
                })
            .Select(group =>
                new TopMenuRevenueResult
                {
                    MenuItemId =
                        group.Key.MenuItemId,

                    MenuItemName =
                        group.Key.MenuItemName,

                    TotalQuantity =
                        group.Sum(
                            x => (long)x.Quantity),

                    Revenue =
                        group.Sum(
                            x => x.Revenue)
                })
            .OrderByDescending(
                x => x.Revenue)
            .ThenBy(
                x => x.MenuItemId)
            .Take(10)
            .ToList();
    }

    private static List<HourlyRevenueResult>
        HourlyRevenueSequential(
            SalesFact[] facts)
    {
        return Enumerable
            .Range(0, 24)
            .Select(hour =>
                new HourlyRevenueResult
                {
                    Hour =
                        hour,

                    Revenue =
                        facts
                            .Where(x =>
                                x.CreatedAt.Hour ==
                                hour)
                            .Sum(x =>
                                x.Revenue)
                })
            .ToList();
    }

    private static List<HourlyRevenueResult>
        HourlyRevenuePlinq(
            SalesFact[] facts)
    {
        return Enumerable
            .Range(0, 24)
            .AsParallel()
            .AsOrdered()
            .Select(hour =>
                new HourlyRevenueResult
                {
                    Hour =
                        hour,

                    Revenue =
                        facts
                            .Where(x =>
                                x.CreatedAt.Hour ==
                                hour)
                            .Sum(x =>
                                x.Revenue)
                })
            .ToList();
    }

    private static List<MonthlyTopBranchResult>
        MonthlyTopBranchSequential(
            SalesFact[] facts)
    {
        var branchMonthly =
            facts
                .GroupBy(x =>
                    new
                    {
                        Year =
                            x.CreatedAt.Year,

                        Month =
                            x.CreatedAt.Month,

                        x.BranchCode
                    })
                .Select(group =>
                    new MonthlyTopBranchResult
                    {
                        Year =
                            group.Key.Year,

                        Month =
                            group.Key.Month,

                        BranchCode =
                            group.Key.BranchCode,

                        Revenue =
                            group.Sum(
                                x => x.Revenue)
                    })
                .ToList();

        return branchMonthly
            .GroupBy(x =>
                new
                {
                    x.Year,
                    x.Month
                })
            .Select(group =>
                group
                    .OrderByDescending(
                        x => x.Revenue)
                    .ThenBy(
                        x => x.BranchCode)
                    .First())
            .OrderBy(
                x => x.Year)
            .ThenBy(
                x => x.Month)
            .ToList();
    }

    private static List<MonthlyTopBranchResult>
        MonthlyTopBranchPlinq(
            SalesFact[] facts)
    {
        var branchMonthly =
            facts
                .AsParallel()
                .GroupBy(x =>
                    new
                    {
                        Year =
                            x.CreatedAt.Year,

                        Month =
                            x.CreatedAt.Month,

                        x.BranchCode
                    })
                .Select(group =>
                    new MonthlyTopBranchResult
                    {
                        Year =
                            group.Key.Year,

                        Month =
                            group.Key.Month,

                        BranchCode =
                            group.Key.BranchCode,

                        Revenue =
                            group.Sum(
                                x => x.Revenue)
                    })
                .ToArray();

        return branchMonthly
            .AsParallel()
            .GroupBy(x =>
                new
                {
                    x.Year,
                    x.Month
                })
            .Select(group =>
                group
                    .OrderByDescending(
                        x => x.Revenue)
                    .ThenBy(
                        x => x.BranchCode)
                    .First())
            .OrderBy(
                x => x.Year)
            .ThenBy(
                x => x.Month)
            .ToList();
    }

    private static List<LowRevenueMenuResult>
        LowRevenueMenuSequential(
            SalesFact[] facts)
    {
        var totalRevenue =
            facts.Sum(
                x => x.Revenue);

        if (totalRevenue == 0)
        {
            return [];
        }

        var threshold =
            totalRevenue * 0.01m;

        return facts
            .GroupBy(x =>
                new
                {
                    x.MenuItemId,
                    x.MenuItemName
                })
            .Select(group =>
            {
                var revenue =
                    group.Sum(
                        x => x.Revenue);

                return new LowRevenueMenuResult
                {
                    MenuItemId =
                        group.Key.MenuItemId,

                    MenuItemName =
                        group.Key.MenuItemName,

                    Revenue =
                        revenue,

                    RevenuePercentage =
                        revenue /
                        totalRevenue *
                        100m
                };
            })
            .Where(x =>
                x.Revenue < threshold)
            .OrderBy(
                x => x.Revenue)
            .ThenBy(
                x => x.MenuItemId)
            .ToList();
    }

    private static List<LowRevenueMenuResult>
        LowRevenueMenuPlinq(
            SalesFact[] facts)
    {
        var totalRevenue =
            facts
                .AsParallel()
                .Sum(x =>
                    x.Revenue);

        if (totalRevenue == 0)
        {
            return [];
        }

        var threshold =
            totalRevenue * 0.01m;

        return facts
            .AsParallel()
            .GroupBy(x =>
                new
                {
                    x.MenuItemId,
                    x.MenuItemName
                })
            .Select(group =>
            {
                var revenue =
                    group.Sum(
                        x => x.Revenue);

                return new LowRevenueMenuResult
                {
                    MenuItemId =
                        group.Key.MenuItemId,

                    MenuItemName =
                        group.Key.MenuItemName,

                    Revenue =
                        revenue,

                    RevenuePercentage =
                        revenue /
                        totalRevenue *
                        100m
                };
            })
            .Where(x =>
                x.Revenue < threshold)
            .OrderBy(
                x => x.Revenue)
            .ThenBy(
                x => x.MenuItemId)
            .ToList();
    }

    private static (
        List<T> Results,
        double Milliseconds)
        Measure<T>(
            Func<List<T>> action)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var stopwatch =
            Stopwatch.StartNew();

        var results =
            action();

        stopwatch.Stop();

        return (
            results,
            stopwatch.Elapsed.TotalMilliseconds);
    }

    private static bool ValidateTop10(
        List<TopMenuRevenueResult> a,
        List<TopMenuRevenueResult> b)
    {
        return a
            .Select(x =>
                (
                    x.MenuItemId,
                    x.TotalQuantity,
                    x.Revenue
                ))
            .SequenceEqual(
                b.Select(x =>
                    (
                        x.MenuItemId,
                        x.TotalQuantity,
                        x.Revenue
                    )));
    }

    private static bool ValidateHourly(
        List<HourlyRevenueResult> a,
        List<HourlyRevenueResult> b)
    {
        return a
            .Select(x =>
                (
                    x.Hour,
                    x.Revenue
                ))
            .SequenceEqual(
                b.Select(x =>
                    (
                        x.Hour,
                        x.Revenue
                    )));
    }

    private static bool ValidateMonthlyBranch(
        List<MonthlyTopBranchResult> a,
        List<MonthlyTopBranchResult> b)
    {
        return a
            .Select(x =>
                (
                    x.Year,
                    x.Month,
                    x.BranchCode,
                    x.Revenue
                ))
            .SequenceEqual(
                b.Select(x =>
                    (
                        x.Year,
                        x.Month,
                        x.BranchCode,
                        x.Revenue
                    )));
    }

    private static bool ValidateLowRevenue(
        List<LowRevenueMenuResult> a,
        List<LowRevenueMenuResult> b)
    {
        return a
            .Select(x =>
                (
                    x.MenuItemId,
                    x.Revenue,
                    x.RevenuePercentage
                ))
            .SequenceEqual(
                b.Select(x =>
                    (
                        x.MenuItemId,
                        x.Revenue,
                        x.RevenuePercentage
                    )));
    }

    public async Task RunReportsAsync()
    {
        Console.WriteLine();
        Console.WriteLine(
            "========== YC3 LINQ vs PLINQ ==========");

        var facts =
            await LoadDataAsync();

        if (facts.Length == 0)
        {
            Console.WriteLine(
                "No sales data found.");

            return;
        }

        Console.WriteLine();

        Console.WriteLine(
            $"Logical CPU cores: " +
            $"{Environment.ProcessorCount}");

        WarmUp(facts);

        var benchmarks =
            new List<PlinqBenchmarkResult>();

        // ======================================
        // REPORT 1
        // ======================================

        Console.WriteLine();
        Console.WriteLine(
            "REPORT 1 - TOP 10 MENU BY REVENUE");

        var top10Linq =
            Measure(
                () =>
                    Top10MenuSequential(
                        facts));

        var top10Plinq =
            Measure(
                () =>
                    Top10MenuPlinq(
                        facts));

        var top10Match =
            ValidateTop10(
                top10Linq.Results,
                top10Plinq.Results);

        benchmarks.Add(
            CreateBenchmark(
                "Top 10 menu by revenue",
                top10Linq.Milliseconds,
                top10Plinq.Milliseconds,
                top10Match));

        PrintTop10(
            top10Linq.Results);

        // ======================================
        // REPORT 2
        // ======================================

        Console.WriteLine();
        Console.WriteLine(
            "REPORT 2 - REVENUE BY HOUR");

        var hourlyLinq =
            Measure(
                () =>
                    HourlyRevenueSequential(
                        facts));

        var hourlyPlinq =
            Measure(
                () =>
                    HourlyRevenuePlinq(
                        facts));

        var hourlyMatch =
            ValidateHourly(
                hourlyLinq.Results,
                hourlyPlinq.Results);

        benchmarks.Add(
            CreateBenchmark(
                "Revenue by hour (AsOrdered)",
                hourlyLinq.Milliseconds,
                hourlyPlinq.Milliseconds,
                hourlyMatch));

        PrintHourly(
            hourlyLinq.Results);

        // ======================================
        // REPORT 3
        // ======================================

        Console.WriteLine();
        Console.WriteLine(
            "REPORT 3 - TOP BRANCH BY MONTH");

        var branchLinq =
            Measure(
                () =>
                    MonthlyTopBranchSequential(
                        facts));

        var branchPlinq =
            Measure(
                () =>
                    MonthlyTopBranchPlinq(
                        facts));

        var branchMatch =
            ValidateMonthlyBranch(
                branchLinq.Results,
                branchPlinq.Results);

        benchmarks.Add(
            CreateBenchmark(
                "Top branch by month",
                branchLinq.Milliseconds,
                branchPlinq.Milliseconds,
                branchMatch));

        PrintMonthlyBranches(
            branchLinq.Results);

        // ======================================
        // REPORT 4
        // ======================================

        Console.WriteLine();
        Console.WriteLine(
            "REPORT 4 - MENU UNDER 1% REVENUE");

        var lowLinq =
            Measure(
                () =>
                    LowRevenueMenuSequential(
                        facts));

        var lowPlinq =
            Measure(
                () =>
                    LowRevenueMenuPlinq(
                        facts));

        var lowMatch =
            ValidateLowRevenue(
                lowLinq.Results,
                lowPlinq.Results);

        benchmarks.Add(
            CreateBenchmark(
                "Menu under 1% revenue",
                lowLinq.Milliseconds,
                lowPlinq.Milliseconds,
                lowMatch));

        PrintLowRevenue(
            lowLinq.Results);

        // ======================================
        // SUMMARY
        // ======================================

        PrintBenchmarkSummary(
            benchmarks);
    }

    private static void WarmUp(
    SalesFact[] facts)
    {
        var sampleSize =
            Math.Min(
                10_000,
                facts.Length);

        var sample =
            facts
                .Take(sampleSize)
                .ToArray();

        // Warm-up LINQ
        _ = sample.Sum(
            x => x.Quantity);

        // Warm-up PLINQ
        _ = sample
            .AsParallel()
            .Sum(x => x.Quantity);

        Console.WriteLine(
            "LINQ/PLINQ warm-up complete.");
    }

    private static PlinqBenchmarkResult
    CreateBenchmark(
        string reportName,
        double linqMilliseconds,
        double plinqMilliseconds,
        bool resultsMatch)
    {
        var safePlinqTime =
            Math.Max(
                0.0001,
                plinqMilliseconds);

        return new PlinqBenchmarkResult
        {
            ReportName =
                reportName,

            LinqMilliseconds =
                linqMilliseconds,

            PlinqMilliseconds =
                plinqMilliseconds,

            Speedup =
                linqMilliseconds /
                safePlinqTime,

            ResultsMatch =
                resultsMatch
        };
    }

    private static void PrintTop10(
    List<TopMenuRevenueResult> results)
    {
        Console.WriteLine();

        Console.WriteLine(
            $"{"Rank",-6}" +
            $"{"Menu",-24}" +
            $"{"Qty",12}" +
            $"{"Revenue",18}");

        Console.WriteLine(
            new string('-', 60));

        for (var index = 0;
             index < results.Count;
             index++)
        {
            var item =
                results[index];

            Console.WriteLine(
                $"{index + 1,-6}" +
                $"{item.MenuItemName,-24}" +
                $"{item.TotalQuantity,12:N0}" +
                $"{item.Revenue,18:N0}");
        }
    }

    private static void PrintHourly(
    List<HourlyRevenueResult> results)
    {
        Console.WriteLine();

        Console.WriteLine(
            $"{"Hour",-10}" +
            $"{"Revenue",20}");

        Console.WriteLine(
            new string('-', 30));

        foreach (var item in results)
        {
            Console.WriteLine(
                $"{item.Hour:00}:00" +
                $"{item.Revenue,20:N0}");
        }
    }

    private static void PrintMonthlyBranches(
    List<MonthlyTopBranchResult> results)
    {
        Console.WriteLine();

        Console.WriteLine(
            $"{"Month",-12}" +
            $"{"Branch",-12}" +
            $"{"Revenue",20}");

        Console.WriteLine(
            new string('-', 44));

        foreach (var item in results)
        {
            Console.WriteLine(
                $"{item.Year}-{item.Month:00,-7}" +
                $"{item.BranchCode,-12}" +
                $"{item.Revenue,20:N0}");
        }
    }

    private static void PrintLowRevenue(
    List<LowRevenueMenuResult> results)
    {
        Console.WriteLine();

        if (results.Count == 0)
        {
            Console.WriteLine(
                "No menu item is below 1% " +
                "of total revenue.");

            return;
        }

        Console.WriteLine(
            $"{"Menu",-24}" +
            $"{"Revenue",18}" +
            $"{"Share",12}" +
            $"{"Suggestion",-15}");

        Console.WriteLine(
            new string('-', 69));

        foreach (var item in results)
        {
            Console.WriteLine(
                $"{item.MenuItemName,-24}" +
                $"{item.Revenue,18:N0}" +
                $"{item.RevenuePercentage,11:F2}%" +
                $"{"REMOVE",-15}");
        }
    }

    private static void PrintBenchmarkSummary(
    List<PlinqBenchmarkResult> benchmarks)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=============== YC3 BENCHMARK ===============");

        Console.WriteLine(
            $"{"Report",-30}" +
            $"{"LINQ(ms)",12}" +
            $"{"PLINQ(ms)",12}" +
            $"{"Speedup",10}" +
            $"{"Match",10}");

        Console.WriteLine(
            new string('-', 74));

        foreach (var result
                 in benchmarks)
        {
            Console.WriteLine(
                $"{result.ReportName,-30}" +
                $"{result.LinqMilliseconds,12:F2}" +
                $"{result.PlinqMilliseconds,12:F2}" +
                $"{result.Speedup,9:F2}x" +
                $"{(result.ResultsMatch ? "OK" : "FAIL"),10}");
        }

        Console.WriteLine(
            "=============================================");
    }
}
