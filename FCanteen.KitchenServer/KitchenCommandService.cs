using FCanteen.Data;
using FCanteen.Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.KitchenServer;

public class KitchenCommandService
{
    private readonly string _connectionString;
    private readonly StockUdpBroadcaster _udpBroadcaster;
    private readonly PriceSyncService
       _priceSyncService;

    private readonly string
        _priceSyncUrl;

    public KitchenCommandService(
        string connectionString,
        int udpPort,
        string priceSyncUrl)
    {
        _connectionString =
            connectionString;

        _priceSyncUrl =
            priceSyncUrl;

        _udpBroadcaster =
            new StockUdpBroadcaster(
                connectionString,
                udpPort);

        _priceSyncService =
            new PriceSyncService(
                connectionString);
    }

    public async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Kitchen commands:");

        Console.WriteLine(
            "  out <id>  - Mark item OUT OF STOCK");

        Console.WriteLine(
            "  in <id>   - Mark item AVAILABLE");

        Console.WriteLine(
            "  sync      - Synchronize menu prices");

        while (true)
        {
            Console.WriteLine();

            Console.Write("> ");

            var input =
                Console.ReadLine();

            if (string.IsNullOrWhiteSpace(
                    input))
            {
                continue;
            }

            if (input.Equals(
                "sync",
                StringComparison.OrdinalIgnoreCase))
            {
                await _priceSyncService
                    .SyncAsync(
                        _priceSyncUrl);

                continue;
            }

            var parts =
                input.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                Console.WriteLine(
                    "Example: out 5");

                continue;
            }

            if (!int.TryParse(
                    parts[1],
                    out var menuItemId))
            {
                Console.WriteLine(
                    "Menu item ID must be a number.");

                continue;
            }

            if (parts[0].Equals(
                    "out",
                    StringComparison.OrdinalIgnoreCase))
            {
                await ChangeAvailabilityAsync(
                    menuItemId,
                    false);
            }
            else if (parts[0].Equals(
                         "in",
                         StringComparison.OrdinalIgnoreCase))
            {
                await ChangeAvailabilityAsync(
                    menuItemId,
                    true);
            }
            else
            {
                Console.WriteLine(
                    "Unknown command.");
            }
        }
    }

    private async Task ChangeAvailabilityAsync(
        int menuItemId,
        bool isAvailable)
    {
        await using var db =
            CreateDbContext();

        var menuItem =
            await db.MenuItems
                .FirstOrDefaultAsync(
                    x =>
                        x.MenuItemId ==
                        menuItemId);

        if (menuItem is null)
        {
            Console.WriteLine(
                $"Menu item ID {menuItemId} " +
                "does not exist.");

            return;
        }

        if (menuItem.IsAvailable ==
            isAvailable)
        {
            Console.WriteLine(
                $"{menuItem.Name} is already " +
                (isAvailable
                    ? "AVAILABLE."
                    : "OUT OF STOCK."));

            return;
        }

        menuItem.IsAvailable =
            isAvailable;

        await db.SaveChangesAsync();

        var message =
            new StockStatusMessage
            {
                MenuItemId =
                    menuItem.MenuItemId,

                MenuItemName =
                    menuItem.Name,

                IsAvailable =
                    menuItem.IsAvailable,

                SentAt =
                    DateTime.Now
            };

        await _udpBroadcaster
            .BroadcastAsync(message);

        Console.WriteLine(
            $"{menuItem.Name} -> " +
            (isAvailable
                ? "AVAILABLE"
                : "OUT OF STOCK"));
    }

    private FCanteenContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder
                <FCanteenContext>()
                .UseSqlServer(
                    _connectionString)
                .Options;

        return new FCanteenContext(
            options);
    }
}