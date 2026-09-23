using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using FCanteen.Analytics.Models;
using FCanteen.Data.Entities;

namespace FCanteen.Analytics.Services;

public class RaceConditionService
{
    private const int OrderCount =
        50_000;

    private const decimal InitialStock =
        100_000m;

    private const decimal ConsumptionPerOrder =
        1m;

    /*
     * Chỉ dùng để làm race condition
     * dễ quan sát hơn khi demo.
     *
     * Không phải logic nghiệp vụ thật.
     */
    private const int SpinWaitIterations =
        200;

    private static IngredientOrderWorkItem[]
        CreateOrders()
    {
        return Enumerable
            .Range(1, OrderCount)
            .Select(orderNumber =>
                new IngredientOrderWorkItem
                {
                    OrderNumber =
                        orderNumber,

                    QuantityToConsume =
                        ConsumptionPerOrder
                })
            .ToArray();
    }

    private static Ingredient
        CreateSimulationIngredient()
    {
        return new Ingredient
        {
            Name =
                "Gao - YC5 Simulation",

            Unit =
                "unit",

            StockQuantity =
                InitialStock,

            AlertThreshold =
                10_000m
        };
    }

    private static RaceConditionResult
        RunWithoutSynchronization(
        Ingredient ingredient,
        IngredientOrderWorkItem[] orders)
    {
        var stopwatch =
            Stopwatch.StartNew();

        var parallelOptions =
            new ParallelOptions
            {
                /*
                 * Đảm bảo có đủ worker để
                 * race condition dễ xuất hiện.
                 */
                MaxDegreeOfParallelism =
                    Math.Max(
                        4,
                        Environment.ProcessorCount)
            };

        Parallel.ForEach(
            orders,
            parallelOptions,
            order =>
            {
                /*
                 * READ
                 */
                var currentStock =
                    ingredient.StockQuantity;

                /*
                 * Cố tình mở rộng khoảng thời gian
                 * giữa READ và WRITE để race
                 * condition dễ quan sát khi demo.
                 */
                Thread.SpinWait(
                    SpinWaitIterations);

                /*
                 * WRITE
                 *
                 * Không lock.
                 */
                ingredient.StockQuantity =
                    currentStock -
                    order.QuantityToConsume;
            });

        stopwatch.Stop();

        var totalConsumed =
            orders.Sum(
                x => x.QuantityToConsume);

        var expectedStock =
            InitialStock -
            totalConsumed;

        return new RaceConditionResult
        {
            VersionName =
                "WITHOUT SYNCHRONIZATION",

            OrderCount =
                orders.Length,

            InitialStock =
                InitialStock,

            TotalConsumed =
                totalConsumed,

            ExpectedStock =
                expectedStock,

            ActualStock =
                ingredient.StockQuantity,

            ElapsedMilliseconds =
                stopwatch.Elapsed
                    .TotalMilliseconds
        };
    }

    private static RaceConditionResult
        RunWithLock(
        Ingredient ingredient,
        IngredientOrderWorkItem[] orders)
    {
        var syncRoot =
            new object();

        var stopwatch =
            Stopwatch.StartNew();

        var parallelOptions =
            new ParallelOptions
            {
                MaxDegreeOfParallelism =
                    Math.Max(
                        4,
                        Environment.ProcessorCount)
            };

        Parallel.ForEach(
            orders,
            parallelOptions,
            order =>
            {
                lock (syncRoot)
                {
                    var currentStock =
                        ingredient.StockQuantity;

                    Thread.SpinWait(
                        SpinWaitIterations);

                    ingredient.StockQuantity =
                        currentStock -
                        order.QuantityToConsume;
                }
            });

        stopwatch.Stop();

        var totalConsumed =
            orders.Sum(
                x => x.QuantityToConsume);

        var expectedStock =
            InitialStock -
            totalConsumed;

        return new RaceConditionResult
        {
            VersionName =
                "WITH LOCK",

            OrderCount =
                orders.Length,

            InitialStock =
                InitialStock,

            TotalConsumed =
                totalConsumed,

            ExpectedStock =
                expectedStock,

            ActualStock =
                ingredient.StockQuantity,

            ElapsedMilliseconds =
                stopwatch.Elapsed
                    .TotalMilliseconds
        };
    }

    private static void PrintResult(
        RaceConditionResult result)
    {
        Console.WriteLine();

        Console.WriteLine(
            $"========== {result.VersionName} ==========");

        Console.WriteLine(
            $"Orders          : " +
            $"{result.OrderCount:N0}");

        Console.WriteLine(
            $"Initial stock   : " +
            $"{result.InitialStock:N0}");

        Console.WriteLine(
            $"Total consumed  : " +
            $"{result.TotalConsumed:N0}");

        Console.WriteLine(
            $"Expected stock  : " +
            $"{result.ExpectedStock:N0}");

        Console.WriteLine(
            $"Actual stock    : " +
            $"{result.ActualStock:N0}");

        Console.WriteLine(
            $"Difference      : " +
            $"{result.Difference:N0}");

        Console.WriteLine(
            $"Elapsed         : " +
            $"{result.ElapsedMilliseconds:F2} ms");

        Console.WriteLine(
            $"Result          : " +
            (result.IsCorrect
                ? "CORRECT"
                : "WRONG - RACE CONDITION"));

        Console.WriteLine(
            new string('=', 48));
    }

    public void Run()
    {
        Console.WriteLine();
        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            " YC5 - RACE CONDITION: INGREDIENT STOCK");

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            $"Logical CPU cores : " +
            $"{Environment.ProcessorCount}");

        Console.WriteLine(
            $"Parallel workers  : " +
            $"{Math.Max(4, Environment.ProcessorCount)}");

        Console.WriteLine(
            $"Orders            : " +
            $"{OrderCount:N0}");

        Console.WriteLine(
            $"Consume / order   : " +
            $"{ConsumptionPerOrder:N0}");

        Console.WriteLine(
            $"Initial stock     : " +
            $"{InitialStock:N0}");

        var expected =
            InitialStock -
            OrderCount *
            ConsumptionPerOrder;

        Console.WriteLine(
            $"Expected stock    : " +
            $"{expected:N0}");

        /*
         * Tạo cùng một workload cho cả hai version.
         */
        var orders =
            CreateOrders();

        /*
         * Hai Ingredient riêng để cả hai bắt đầu
         * với cùng InitialStock.
         */
        var unsafeIngredient =
            CreateSimulationIngredient();

        var safeIngredient =
            CreateSimulationIngredient();

        Console.WriteLine();
        Console.WriteLine(
            "Running WITHOUT synchronization...");

        var unsafeResult =
            RunWithoutSynchronization(
                unsafeIngredient,
                orders);

        PrintResult(
            unsafeResult);

        Console.WriteLine();
        Console.WriteLine(
            "Running WITH lock...");

        var safeResult =
            RunWithLock(
                safeIngredient,
                orders);

        PrintResult(
            safeResult);

        PrintComparison(
            unsafeResult,
            safeResult);
    }

    private static void PrintComparison(
    RaceConditionResult unsafeResult,
    RaceConditionResult safeResult)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=============== COMPARISON ===============");

        Console.WriteLine(
            $"{"Version",-28}" +
            $"{"Expected",14}" +
            $"{"Actual",14}" +
            $"{"Status",16}");

        Console.WriteLine(
            new string('-', 72));

        Console.WriteLine(
            $"{"Without synchronization",-28}" +
            $"{unsafeResult.ExpectedStock,14:N0}" +
            $"{unsafeResult.ActualStock,14:N0}" +
            $"{(unsafeResult.IsCorrect ? "CORRECT" : "WRONG"),16}");

        Console.WriteLine(
            $"{"With lock",-28}" +
            $"{safeResult.ExpectedStock,14:N0}" +
            $"{safeResult.ActualStock,14:N0}" +
            $"{(safeResult.IsCorrect ? "CORRECT" : "WRONG"),16}");

        Console.WriteLine(
            new string('-', 72));

        Console.WriteLine();

        if (!unsafeResult.IsCorrect &&
            safeResult.IsCorrect)
        {
            Console.WriteLine(
                "YC5 RESULT: Race condition reproduced " +
                "and fixed successfully.");
        }
        else if (unsafeResult.IsCorrect)
        {
            Console.WriteLine(
                "Unsafe result happened to be correct " +
                "in this run.");

            Console.WriteLine(
                "Run YC5 again because race conditions " +
                "are nondeterministic.");
        }
        else
        {
            Console.WriteLine(
                "Safe version is incorrect. " +
                "Check the lock implementation.");
        }

        Console.WriteLine(
            "==========================================");
    }
}
