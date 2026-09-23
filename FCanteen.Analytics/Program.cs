using FCanteen.Data;
using FCanteen.Data.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FCanteen.Analytics.Services;

var configuration =
    new ConfigurationBuilder()
        .SetBasePath(
            AppContext.BaseDirectory)
        .AddJsonFile(
            "appsettings.json",
            optional: false)
        .Build();

var connectionString =
    configuration
        .GetConnectionString("FCanteen")
    ?? throw new InvalidOperationException(
        "Connection string 'FCanteen' was not found.");

var options =
    new DbContextOptionsBuilder
        <FCanteenContext>()
        .UseSqlServer(
            connectionString)
        .Options;

while (true)
{
    Console.Clear();

    Console.WriteLine(
        "======================================");

    Console.WriteLine(
        "       FCANTEEN LAB 02 ANALYTICS");

    Console.WriteLine(
        "======================================");

    Console.WriteLine(
        "1. YC1 - Run large data seeder");

    Console.WriteLine(
        "2. YC2 - Menu efficiency benchmark");

    Console.WriteLine(
        "3. YC3 - LINQ vs PLINQ reports");

    Console.WriteLine(
        "4. YC4 - Async EF Core daily report");

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
            {
                await using var db =
                    new FCanteenContext(
                        options);

                await Lab02DataSeeder
                    .SeedAsync(db);

                break;
            }

        case "2":
            {
                var service =
                    new MenuEfficiencyService(
                        connectionString);

                await service
                    .RunBenchmarkAsync();

                break;
            }

        case "3":
            {
                var service =
                    new PlinqReportService(
                        connectionString);

                await service
                    .RunReportsAsync();

                break;
            }

        case "4":
            {
                Console.Write(
                    "Branch code " +
                    "(default BR01): ");

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
                    "High-value threshold " +
                    "(default 200000): ");

                var thresholdInput =
                    Console.ReadLine();

                decimal threshold =
                    200_000m;

                if (!string.IsNullOrWhiteSpace(
                        thresholdInput) &&
                    decimal.TryParse(
                        thresholdInput,
                        out var parsedThreshold) &&
                    parsedThreshold >= 0)
                {
                    threshold =
                        parsedThreshold;
                }

                var service =
                    new AsyncDailyReportService(
                        connectionString);

                /*
                 * Cancellation token từ caller.
                 */
                using var userCts =
                    new CancellationTokenSource();

                ConsoleCancelEventHandler handler =
                    (_, eventArgs) =>
                    {
                        eventArgs.Cancel = true;

                        userCts.Cancel();
                    };

                Console.CancelKeyPress +=
                    handler;

                try
                {
                    await service.RunYc4Async(
                        branchCode,
                        threshold,
                        userCts.Token);
                }
                finally
                {
                    Console.CancelKeyPress -=
                        handler;
                }

                break;
            }

        case "0":
            return;

        default:
            Console.WriteLine(
                "Invalid choice.");
            break;
    }

    Console.WriteLine();

    Console.WriteLine(
        "Press Enter to return to menu.");

    Console.ReadLine();
}