using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FCanteen.Data;
using FCanteen.Data.Contracts;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace FCanteen.PosClient;

internal class Program
{
    static async Task Main(string[] args)
    {
        // YC3: tên quầy phải truyền qua command-line argument.
        if (args.Length == 0 ||
            string.IsNullOrWhiteSpace(args[0]))
        {
            Console.WriteLine(
                "Missing counter name.");

            Console.WriteLine(
                "Example: QUAY01");

            return;
        }

        var counterName =
            args[0].Trim().ToUpperInvariant();

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

        var serverHost =
            configuration[
                "Networking:KitchenServerHost"]
            ?? "127.0.0.1";

        var tcpPort =
            configuration
                .GetValue<int>(
                    "Networking:TcpPort");

        var udpPort =
            configuration
                .GetValue<int>(
                    "Networking:UdpPort");

        var availability =
            new ConcurrentDictionary<int, bool>();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var udpListener =
            new StockUdpListener(
                udpPort,
                availability);

        var udpTask =
            Task.Run(
                () =>
                    udpListener.RunAsync(
                        cancellationTokenSource.Token));


        Console.Title =
            $"FCanteen POS - {counterName}";

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "            FCANTEEN POS");

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            $"Counter : {counterName}");

        Console.WriteLine(
            $"Kitchen : {serverHost}:{tcpPort}");

        while (true)
        {
            try
            {
                await CreateOrderAsync(
                    counterName,
                    connectionString,
                    serverHost,
                    tcpPort,
                    availability);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.Write(
                "Create another order? (Y/N): ");

            var answer =
                Console.ReadLine();

            if (!string.Equals(
                    answer,
                    "Y",
                    StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.Clear();

            Console.WriteLine(
                $"FCANTEEN POS - {counterName}");
        }
        cancellationTokenSource.Cancel();

        try
        {
            await udpTask;
        }
        catch (OperationCanceledException)
        {
        }
        Console.WriteLine(
            "POS stopped.");
    }

    private static async Task CreateOrderAsync(
        string counterName,
        string connectionString,
        string serverHost,
        int tcpPort,
        ConcurrentDictionary<int, bool>
            availability)
    {
        await using var db =
            CreateDbContext(
                connectionString);

        // YC3: đọc thực đơn từ database.
        var menuItems =
            await db.MenuItems
                .AsNoTracking()
                .OrderBy(x => x.MenuItemId)
                .ToListAsync();

        foreach (var item in menuItems)
        {
            availability.TryAdd(
                item.MenuItemId,
                item.IsAvailable);
        }

        if (menuItems.Count == 0)
        {
            Console.WriteLine(
                "Menu is empty.");

            return;
        }

        DisplayMenu(
            menuItems,
            availability);

        var requestLines =
            new List<OrderLineRequest>();

        decimal clientTotal = 0;

        while (true)
        {
            Console.WriteLine();

            Console.Write(
                "Choose No. " +
                "(0 = finish order): ");

            var input =
                Console.ReadLine();

            if (!int.TryParse(
                    input,
                    out var menuNumber))
            {
                Console.WriteLine(
                    "Please enter a number.");

                continue;
            }

            // 0 = hoàn tất nhập món.
            if (menuNumber == 0)
            {
                break;
            }

            if (menuNumber < 1 ||
                menuNumber > menuItems.Count)
            {
                Console.WriteLine(
                    "Menu number does not exist.");

                continue;
            }

            // Bảng hiển thị đánh số từ 1.
            var selectedItem =
                menuItems[menuNumber - 1];

            var isAvailable =
                availability.TryGetValue(
                    selectedItem.MenuItemId,
                    out var currentAvailability)
                    ? currentAvailability
                    : selectedItem.IsAvailable;

            if (!isAvailable)
            {
                Console.WriteLine(
                    $"{selectedItem.Name} " +
                    "is currently OUT OF STOCK.");

                continue;
            }

            Console.Write(
                "Quantity: ");

            var quantityInput =
                Console.ReadLine();

            if (!int.TryParse(
                    quantityInput,
                    out var quantity) ||
                quantity <= 0)
            {
                Console.WriteLine(
                    "Quantity must be greater than 0.");

                continue;
            }

            Console.Write(
                "Note (press Enter if none): ");

            var note =
                Console.ReadLine();

            requestLines.Add(
                new OrderLineRequest
                {
                    MenuItemId =
                        selectedItem.MenuItemId,

                    Quantity =
                        quantity,

                    Note =
                        string.IsNullOrWhiteSpace(note)
                            ? null
                            : note.Trim()
                });

            var lineTotal =
                selectedItem.Price *
                quantity;

            clientTotal +=
                lineTotal;

            Console.WriteLine();
            Console.WriteLine(
                $"Added: " +
                $"{selectedItem.Name} " +
                $"x {quantity}");

            Console.WriteLine(
                $"Line total: " +
                $"{lineTotal:N0} VND");

            Console.WriteLine(
                $"Current subtotal: " +
                $"{clientTotal:N0} VND");
        }

        if (requestLines.Count == 0)
        {
            Console.WriteLine(
                "Order cancelled: no item selected.");

            return;
        }

        DisplayOrderSummary(
            menuItems,
            requestLines,
            clientTotal);

        Console.WriteLine();

        Console.Write(
            "Send this order? (Y/N): ");

        var confirm =
            Console.ReadLine();

        if (!string.Equals(
                confirm,
                "Y",
                StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(
                "Order cancelled.");

            return;
        }

        var request =
            new OrderRequest
            {
                CounterName =
                    counterName,

                ClientTotal =
                    clientTotal,

                Lines =
                    requestLines
            };

        await SendOrderAsync(
            request,
            serverHost,
            tcpPort);
    }

    private static void DisplayMenu(
        List<MenuItem> menuItems,
        ConcurrentDictionary<int, bool>
            availability)
    {
        Console.WriteLine();

        Console.WriteLine(
            "==================== MENU ====================");

        Console.WriteLine(
            $"{"No.",-5}" +
            $"{"Name",-24}" +
            $"{"Price",14}" +
            $"{"Unit",-10}" +
            $"{"Status",-12}");

        Console.WriteLine(
            new string('-', 65));

        for (var index = 0;
             index < menuItems.Count;
             index++)
        {
            var item =
                menuItems[index];

            var currentAvailability =
                availability.TryGetValue(
                    item.MenuItemId,
                    out var value)
                    ? value
                    : item.IsAvailable;

            var status =
                currentAvailability
                    ? "Available"
                    : "OUT";

            Console.WriteLine(
                $"{index + 1,-5}" +
                $"{item.Name,-24}" +
                $"{item.Price,14:N0}" +
                $"{item.Unit,-10}" +
                $"{status,-12}");
        }

        Console.WriteLine(
            "==============================================");
    }

    private static void DisplayOrderSummary(
        List<MenuItem> menuItems,
        List<OrderLineRequest> lines,
        decimal clientTotal)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=============== ORDER SUMMARY ===============");

        Console.WriteLine(
            $"{"Item",-24}" +
            $"{"Qty",6}" +
            $"{"Price",14}" +
            $"{"Total",14}");

        Console.WriteLine(
            new string('-', 58));

        foreach (var line in lines)
        {
            var menuItem =
                menuItems.First(
                    x =>
                        x.MenuItemId ==
                        line.MenuItemId);

            var total =
                menuItem.Price *
                line.Quantity;

            Console.WriteLine(
                $"{menuItem.Name,-24}" +
                $"{line.Quantity,6}" +
                $"{menuItem.Price,14:N0}" +
                $"{total,14:N0}");

            if (!string.IsNullOrWhiteSpace(
                    line.Note))
            {
                Console.WriteLine(
                    $"  Note: {line.Note}");
            }
        }

        Console.WriteLine(
            new string('-', 58));

        Console.WriteLine(
            $"CLIENT SUBTOTAL: " +
            $"{clientTotal:N0} VND");

        Console.WriteLine(
            "=============================================");
    }

    private static async Task SendOrderAsync(
        OrderRequest request,
        string serverHost,
        int tcpPort)
    {
        using var tcpClient =
            new TcpClient();

        Console.WriteLine();
        Console.WriteLine(
            $"Connecting to " +
            $"{serverHost}:{tcpPort}...");

        try
        {
            await tcpClient.ConnectAsync(
                serverHost,
                tcpPort);

            Console.WriteLine(
                "Connected to Kitchen Server.");

            await using var stream =
                tcpClient.GetStream();

            using var reader =
                new StreamReader(
                    stream,
                    Encoding.UTF8,
                    false,
                    1024,
                    leaveOpen: true);

            using var writer =
                new StreamWriter(
                    stream,
                    new UTF8Encoding(false),
                    1024,
                    leaveOpen: true)
                {
                    AutoFlush = true
                };

            var requestJson =
                JsonSerializer.Serialize(
                    request);

            // Server YC2 dùng ReadLineAsync(),
            // vì vậy client phải gửi bằng WriteLineAsync().
            await writer.WriteLineAsync(
                requestJson);

            Console.WriteLine(
                "Order sent. " +
                "Waiting for server confirmation...");

            var responseJson =
                await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(
                    responseJson))
            {
                Console.WriteLine(
                    "Server returned an empty response.");

                return;
            }

            var response =
                JsonSerializer
                    .Deserialize<OrderResponse>(
                        responseJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive =
                                true
                        });

            if (response is null)
            {
                Console.WriteLine(
                    "Invalid response from server.");

                return;
            }

            DisplayServerResponse(
                response);
        }
        catch (SocketException ex)
        {
            Console.WriteLine(
                "Cannot connect to Kitchen Server.");

            Console.WriteLine(
                $"Socket error: {ex.Message}");

            Console.WriteLine(
                "Make sure KitchenServer is running " +
                "on port 9500.");
        }
    }

    private static void DisplayServerResponse(
        OrderResponse response)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=========== SERVER CONFIRMATION ===========");

        Console.WriteLine(
            $"Success : {response.Success}");

        Console.WriteLine(
            $"Message : {response.Message}");

        if (response.Success)
        {
            Console.WriteLine(
                $"Ticket  : #{response.TicketId}");

            Console.WriteLine(
                $"TOTAL CALCULATED BY SERVER: " +
                $"{response.TotalAmount:N0} VND");
        }

        Console.WriteLine(
            "===========================================");
    }

    private static FCanteenContext CreateDbContext(
        string connectionString)
    {
        var options =
            new DbContextOptionsBuilder
                <FCanteenContext>()
                .UseSqlServer(
                    connectionString)
                .Options;

        return new FCanteenContext(
            options);
    }
}