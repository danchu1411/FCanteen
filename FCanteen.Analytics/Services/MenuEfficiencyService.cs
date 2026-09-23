using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using FCanteen.Analytics.Models;
using FCanteen.Data;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Analytics.Services;

public class MenuEfficiencyService
{
    private readonly string _connectionString;

    public MenuEfficiencyService(
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

    private async Task<(
        MenuItem[] MenuItems,
        TicketLineFact[] Lines)>
        LoadDataAsync()
    {
        await using var db =
            CreateDbContext();

        Console.WriteLine(
            "Loading MenuItems and TicketLines from database...");

        var menuItems =
            await db.MenuItems
                .AsNoTracking()
                .OrderBy(x => x.MenuItemId)
                .ToArrayAsync();

        var lines =
            await db.TicketLines
                .AsNoTracking()
                .Select(x =>
                    new TicketLineFact
                    {
                        MenuItemId =
                            x.MenuItemId,

                        Quantity =
                            x.Quantity,

                        UnitPrice =
                            x.UnitPrice,

                        CreatedAt =
                            x.OrderTicket.CreatedAt
                    })
                .ToArrayAsync();

        Console.WriteLine(
            $"Loaded MenuItems : {menuItems.Length:N0}");

        Console.WriteLine(
            $"Loaded TicketLines: {lines.Length:N0}");

        return (
            menuItems,
            lines);
    }

    private static MenuEfficiencyResult
        CalculateMenuEfficiency(
            MenuItem menuItem,
            TicketLineFact[] allLines)
    {
        long totalQuantity = 0;

        decimal totalRevenue = 0;

        decimal peakRevenue = 0;

        double cpuWork = 0;

        foreach (var line in allLines)
        {
            if (line.MenuItemId !=
                menuItem.MenuItemId)
            {
                continue;
            }

            totalQuantity +=
                line.Quantity;

            var lineRevenue =
                line.UnitPrice *
                line.Quantity;

            totalRevenue +=
                lineRevenue;

            var hour =
                line.CreatedAt.Hour;

            var isPeakHour =
                hour is 11 or 12;

            if (isPeakHour)
            {
                peakRevenue +=
                    lineRevenue;
            }

            /*
             * Cố tình thêm CPU work.
             *
             * Đây không phải I/O.
             * Mục đích là để phép tính đủ nặng
             * cho benchmark Parallel rõ ràng hơn.
             */
            var numericRevenue =
                (double)lineRevenue;

            for (var iteration = 1;
                 iteration <= 30;
                 iteration++)
            {
                cpuWork +=
                    Math.Sqrt(
                        numericRevenue +
                        iteration)
                    *
                    Math.Log(
                        iteration + 1);
            }
        }

        var peakRatio =
            totalRevenue == 0
                ? 0
                : (double)
                  (peakRevenue /
                   totalRevenue);

        /*
         * EfficiencyScore:
         *
         * - doanh thu cao -> score cao
         * - tỷ trọng giờ cao điểm cao -> score cao
         * - số lượng bán cao -> score cao
         *
         * cpuWork được đưa vào với trọng số rất nhỏ,
         * chủ yếu để phép tính CPU đủ nặng.
         */
        var efficiencyScore =
            ((double)totalRevenue / 1_000_000d)
            *
            (1d + peakRatio)
            +
            Math.Log10(
                totalQuantity + 1)
            +
            cpuWork * 0.000000000001d;

        return new MenuEfficiencyResult
        {
            MenuItemId =
                menuItem.MenuItemId,

            MenuItemName =
                menuItem.Name,

            TotalQuantity =
                totalQuantity,

            TotalRevenue =
                totalRevenue,

            PeakRevenue =
                peakRevenue,

            PeakRatio =
                peakRatio,

            EfficiencyScore =
                efficiencyScore
        };
    }

    private static List<MenuEfficiencyResult>
        RunSequential(
            MenuItem[] menuItems,
            TicketLineFact[] allLines)
    {
        var results =
            new List<MenuEfficiencyResult>(
                menuItems.Length);

        foreach (var menuItem in menuItems)
        {
            var result =
                CalculateMenuEfficiency(
                    menuItem,
                    allLines);

            results.Add(result);
        }

        return results;
    }

    private static List<MenuEfficiencyResult>
        RunParallelForEach(
            MenuItem[] menuItems,
            TicketLineFact[] allLines)
    {
        var results =
            new MenuEfficiencyResult[
                menuItems.Length];

        Parallel.ForEach(
            Enumerable.Range(
                0,
                menuItems.Length),
            index =>
            {
                results[index] =
                    CalculateMenuEfficiency(
                        menuItems[index],
                        allLines);
            });

        return results.ToList();
    }

    private static List<MenuEfficiencyResult>
        RunParallelFor(
            MenuItem[] menuItems,
            TicketLineFact[] allLines,
            int maxDegree)
    {
        var results =
            new MenuEfficiencyResult[
                menuItems.Length];

        var options =
            new ParallelOptions
            {
                MaxDegreeOfParallelism =
                    maxDegree
            };

        Parallel.For(
            0,
            menuItems.Length,
            options,
            index =>
            {
                results[index] =
                    CalculateMenuEfficiency(
                        menuItems[index],
                        allLines);
            });

        return results.ToList();
    }

    private static MenuEfficiencyBenchmark
        Measure(
            string methodName,
            Func<List<MenuEfficiencyResult>>
                calculation,
            int? maxDegree = null)
    {
        GC.Collect();

        GC.WaitForPendingFinalizers();

        GC.Collect();

        var stopwatch =
            Stopwatch.StartNew();

        var results =
            calculation();

        stopwatch.Stop();

        return new MenuEfficiencyBenchmark
        {
            MethodName =
                methodName,

            MaxDegreeOfParallelism =
                maxDegree,

            ElapsedMilliseconds =
                stopwatch.ElapsedMilliseconds,

            Results =
                results
        };
    }

    public async Task RunBenchmarkAsync()
    {
        Console.WriteLine();
        Console.WriteLine(
            "========== YC2 MENU EFFICIENCY ==========");

        var (
            menuItems,
            allLines) =
            await LoadDataAsync();

        if (menuItems.Length == 0 ||
            allLines.Length == 0)
        {
            Console.WriteLine(
                "Dataset is empty.");

            return;
        }

        var logicalCores =
            Environment.ProcessorCount;

        Console.WriteLine();
        Console.WriteLine(
            $"Logical CPU cores: {logicalCores}");

        Console.WriteLine(
            "Warming up CPU calculation...");

        // Không đo warm-up.
        _ = CalculateMenuEfficiency(
            menuItems[0],
            allLines);

        Console.WriteLine(
            "Warm-up complete.");

        // phần benchmark sẽ thêm phía dưới
        Console.WriteLine();
        Console.WriteLine(
            "Running sequential foreach...");

        var sequential =
            Measure(
                "foreach",
                () =>
                    RunSequential(
                        menuItems,
                        allLines));
        Console.WriteLine(
    "Running Parallel.ForEach...");

        var parallelForEach =
            Measure(
                "Parallel.ForEach",
                () =>
                    RunParallelForEach(
                        menuItems,
                        allLines));

        Console.WriteLine(
    "Running Parallel.For (Degree=2)...");

        var parallelFor2 =
            Measure(
                "Parallel.For",
                () =>
                    RunParallelFor(
                        menuItems,
                        allLines,
                        2),
                2);

        Console.WriteLine(
    "Running Parallel.For (Degree=4)...");

        var parallelFor4 =
            Measure(
                "Parallel.For",
                () =>
                    RunParallelFor(
                        menuItems,
                        allLines,
                        4),
                4);

        Console.WriteLine(
    $"Running Parallel.For " +
    $"(Degree={logicalCores})...");

        var parallelForCpu =
            Measure(
                "Parallel.For",
                () =>
                    RunParallelFor(
                        menuItems,
                        allLines,
                        logicalCores),
                logicalCores);

        var benchmarks =
    new List<MenuEfficiencyBenchmark>
    {
        sequential,
        parallelForEach,
        parallelFor2,
        parallelFor4,
        parallelForCpu
    };

        var sequentialTime =
    Math.Max(
        1,
        sequential.ElapsedMilliseconds);

        foreach (var benchmark
                 in benchmarks)
        {
            benchmark.Speedup =
                (double)sequentialTime /
                Math.Max(
                    1,
                    benchmark.ElapsedMilliseconds);
        }

        PrintBenchmarkTable(
        benchmarks,
        logicalCores);

        PrintMenuResults(
        sequential.Results);
            ValidateResults(

        sequential,
        benchmarks);

        var fastestBenchmark =
            benchmarks
                .OrderBy(
                    x => x.ElapsedMilliseconds)
                .First();

        Console.WriteLine();

        Console.WriteLine(
            $"Selected calculation time for " +
            $"DailySettlement: " +
            $"{fastestBenchmark.ElapsedMilliseconds:N0} ms");

        await SaveDailySettlementsAsync(
            allLines,
            fastestBenchmark
                .ElapsedMilliseconds);
    }

    private static void PrintBenchmarkTable(
    List<MenuEfficiencyBenchmark>
        benchmarks,
    int logicalCores)
    {
        Console.WriteLine();
        Console.WriteLine(
            "================ BENCHMARK =================");

        Console.WriteLine(
            $"Logical CPU cores: {logicalCores}");

        Console.WriteLine();

        Console.WriteLine(
            $"{"Method",-28}" +
            $"{"Degree",10}" +
            $"{"Time(ms)",14}" +
            $"{"Speedup",12}");

        Console.WriteLine(
            new string('-', 64));

        foreach (var benchmark
                 in benchmarks)
        {
            var degreeText =
                benchmark.MaxDegreeOfParallelism
                    ?.ToString()
                ?? "Auto";

            Console.WriteLine(
                $"{benchmark.MethodName,-28}" +
                $"{degreeText,10}" +
                $"{benchmark.ElapsedMilliseconds,14:N0}" +
                $"{benchmark.Speedup,11:F2}x");
        }

        Console.WriteLine(
            "============================================");
    }

    private static void PrintMenuResults(
    List<MenuEfficiencyResult> results)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=========== MENU EFFICIENCY ===========");

        Console.WriteLine(
            $"{"ID",-5}" +
            $"{"Menu",-22}" +
            $"{"Qty",12}" +
            $"{"Revenue",18}" +
            $"{"Peak%",10}" +
            $"{"Score",12}");

        Console.WriteLine(
            new string('-', 79));

        foreach (var result
                 in results.OrderBy(
                     x => x.MenuItemId))
        {
            Console.WriteLine(
                $"{result.MenuItemId,-5}" +
                $"{result.MenuItemName,-22}" +
                $"{result.TotalQuantity,12:N0}" +
                $"{result.TotalRevenue,18:N0}" +
                $"{result.PeakRatio,9:P1}" +
                $"{result.EfficiencyScore,12:F2}");
        }

        Console.WriteLine(
            "=======================================");
    }

    private static void ValidateResults(
    MenuEfficiencyBenchmark sequential,
    List<MenuEfficiencyBenchmark>
        benchmarks)
    {
        var baseline =
            sequential.Results
                .OrderBy(x => x.MenuItemId)
                .ToArray();

        foreach (var benchmark
                 in benchmarks.Skip(1))
        {
            var candidate =
                benchmark.Results
                    .OrderBy(x => x.MenuItemId)
                    .ToArray();

            var valid =
                baseline.Length ==
                candidate.Length;

            if (valid)
            {
                for (var index = 0;
                     index < baseline.Length;
                     index++)
                {
                    if (baseline[index]
                            .MenuItemId
                        !=
                        candidate[index]
                            .MenuItemId)
                    {
                        valid = false;
                        break;
                    }

                    var difference =
                        Math.Abs(
                            baseline[index]
                                .EfficiencyScore
                            -
                            candidate[index]
                                .EfficiencyScore);

                    if (difference >
                        0.000001)
                    {
                        valid = false;
                        break;
                    }
                }
            }

            Console.WriteLine(
                $"{benchmark.MethodName} " +
                $"{benchmark.MaxDegreeOfParallelism}: " +
                (valid
                    ? "RESULT OK"
                    : "RESULT MISMATCH"));
        }
    }

    private async Task SaveDailySettlementsAsync(
    TicketLineFact[] allLines,
    long calculationTimeMs)
    {
        if (allLines.Length == 0)
        {
            return;
        }

        var latestDate =
            allLines
                .Max(x => x.CreatedAt)
                .Date;

        await using var db =
            CreateDbContext();

        var existing =
            await db.DailySettlements
                .Where(x =>
                    x.Date ==
                    latestDate)
                .ToListAsync();

        if (existing.Count > 0)
        {
            db.DailySettlements
                .RemoveRange(existing);

            await db.SaveChangesAsync();
        }

        var settlements =
            await db.OrderTickets
                .AsNoTracking()
                .Where(x =>
                    x.CreatedAt.Date ==
                    latestDate)
                .GroupBy(x =>
                    x.BranchCode)
                .Select(group =>
                    new DailySettlement
                    {
                        Date =
                            latestDate,

                        BranchCode =
                            group.Key,

                        TotalTickets =
                            group.Count(),

                        TotalRevenue =
                            group.Sum(
                                x => x.TotalAmount),

                        CalculationTimeMs =
                            calculationTimeMs
                    })
                .ToListAsync();

        db.DailySettlements
            .AddRange(settlements);

        await db.SaveChangesAsync();

        Console.WriteLine();
        Console.WriteLine(
            $"Saved {settlements.Count} " +
            $"DailySettlement row(s) " +
            $"for {latestDate:yyyy-MM-dd}.");
    }
}
