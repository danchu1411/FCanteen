using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FCanteen.Data;
using FCanteen.Data.Contracts;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.KitchenServer;

public class KitchenTcpServer
{
    private readonly string _connectionString;
    private readonly int _port;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public KitchenTcpServer(
        string connectionString,
        int port)
    {
        _connectionString = connectionString;
        _port = port;
    }

    public async Task RunAsync(
        CancellationToken cancellationToken)
    {
        var listener =
            new TcpListener(
                IPAddress.Any,
                _port);

        listener.Start();

        Console.WriteLine(
            "====================================");

        Console.WriteLine(
            "      FCANTEEN KITCHEN SERVER");

        Console.WriteLine(
            "====================================");

        Console.WriteLine(
            $"Listening on TCP port {_port}...");

        Console.WriteLine(
            "Press Ctrl+C to stop.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client =
                    await listener.AcceptTcpClientAsync(
                        cancellationToken);

                // Mỗi kết nối được xử lý bởi một Task riêng.
                _ = Task.Run(
                    () => HandleClientAsync(client),
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Chương trình đang được dừng bằng Ctrl+C.
        }
        finally
        {
            listener.Stop();

            Console.WriteLine(
                "Kitchen Server stopped.");
        }
    }

    private async Task HandleClientAsync(
        TcpClient client)
    {
        var remoteAddress =
            client.Client.RemoteEndPoint?.ToString()
            ?? "Unknown";

        var localAddress =
            client.Client.LocalEndPoint?.ToString()
            ?? "KitchenServer";

        Console.WriteLine();
        Console.WriteLine(
            $"Client connected: {remoteAddress}");

        try
        {
            using (client)
            await using (var stream =
                         client.GetStream())
            using (var reader =
                   new StreamReader(
                       stream,
                       Encoding.UTF8,
                       false,
                       1024,
                       leaveOpen: true))
            using (var writer =
                   new StreamWriter(
                       stream,
                       new UTF8Encoding(false),
                       1024,
                       leaveOpen: true)
                   {
                       AutoFlush = true
                   })
            {
                var requestJson =
                    await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(
                        requestJson))
                {
                    await SendResponseAsync(
                        writer,
                        remoteAddress,
                        localAddress,
                        new OrderResponse
                        {
                            Success = false,
                            Message =
                                "Empty request."
                        });

                    return;
                }

                await WriteDeviceLogAsync(
                    protocol: "TCP",
                    sourceAddress: remoteAddress,
                    content: $"IN: {requestJson}");

                OrderRequest? request;

                try
                {
                    request =
                        JsonSerializer
                            .Deserialize<OrderRequest>(
                                requestJson,
                                _jsonOptions);
                }
                catch (JsonException)
                {
                    await SendResponseAsync(
                        writer,
                        remoteAddress,
                        localAddress,
                        new OrderResponse
                        {
                            Success = false,
                            Message =
                                "Invalid JSON."
                        });

                    return;
                }

                if (request is null)
                {
                    await SendResponseAsync(
                        writer,
                        remoteAddress,
                        localAddress,
                        new OrderResponse
                        {
                            Success = false,
                            Message =
                                "Invalid order request."
                        });

                    return;
                }

                var response =
                    await CreateOrderAsync(
                        request);

                await SendResponseAsync(
                    writer,
                    remoteAddress,
                    localAddress,
                    response);

                if (response.Success)
                {
                    await PrintWaitingTicketsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error handling {remoteAddress}: " +
                ex.Message);
        }

        Console.WriteLine(
            $"Client disconnected: {remoteAddress}");
    }

    private async Task<OrderResponse>
        CreateOrderAsync(OrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.CounterName))
        {
            return new OrderResponse
            {
                Success = false,
                Message =
                    "Counter name is required."
            };
        }

        if (request.Lines.Count == 0)
        {
            return new OrderResponse
            {
                Success = false,
                Message =
                    "Order contains no items."
            };
        }

        await using var db =
            CreateDbContext();

        var menuItemIds =
            request.Lines
                .Select(x => x.MenuItemId)
                .Distinct()
                .ToList();

        var menuItems =
            await db.MenuItems
                .Where(x =>
                    menuItemIds.Contains(
                        x.MenuItemId))
                .ToDictionaryAsync(
                    x => x.MenuItemId);

        decimal serverTotal = 0;

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
            {
                return new OrderResponse
                {
                    Success = false,
                    Message =
                        $"Invalid quantity for " +
                        $"MenuItemId={line.MenuItemId}."
                };
            }

            if (!menuItems.TryGetValue(
                    line.MenuItemId,
                    out var menuItem))
            {
                return new OrderResponse
                {
                    Success = false,
                    Message =
                        $"Menu item " +
                        $"{line.MenuItemId} " +
                        "does not exist."
                };
            }

            if (!menuItem.IsAvailable)
            {
                return new OrderResponse
                {
                    Success = false,
                    Message =
                        $"{menuItem.Name} " +
                        "is out of stock."
                };
            }

            // QUAN TRỌNG:
            // server tự lấy Price từ database.
            serverTotal +=
                menuItem.Price *
                line.Quantity;
        }

        var ticket =
            new OrderTicket
            {
                CounterName =
                    request.CounterName,

                TotalAmount =
                    serverTotal,

                CreatedAt =
                    DateTime.Now,

                Status =
                    "Waiting"
            };

        foreach (var requestLine
                 in request.Lines)
        {
            var menuItem =
                menuItems[
                    requestLine.MenuItemId];

            ticket.TicketLines.Add(
                new TicketLine
                {
                    MenuItemId =
                        menuItem.MenuItemId,

                    Quantity =
                        requestLine.Quantity,

                    // Snapshot giá lúc bán.
                    UnitPrice =
                        menuItem.Price,

                    Note =
                        requestLine.Note
                });
        }

        await using var transaction =
            await db.Database
                .BeginTransactionAsync();

        try
        {
            db.OrderTickets.Add(ticket);

            await db.SaveChangesAsync();

            await transaction.CommitAsync();

            Console.WriteLine();
            Console.WriteLine(
                "Order accepted:");

            Console.WriteLine(
                $"  Ticket ID    : " +
                $"{ticket.OrderTicketId}");

            Console.WriteLine(
                $"  Counter      : " +
                $"{ticket.CounterName}");

            Console.WriteLine(
                $"  Client total : " +
                $"{request.ClientTotal:N0}");

            Console.WriteLine(
                $"  Server total : " +
                $"{serverTotal:N0}");

            return new OrderResponse
            {
                Success = true,
                Message =
                    "Order accepted.",

                TicketId =
                    ticket.OrderTicketId,

                TotalAmount =
                    serverTotal
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            Console.WriteLine(
                "Database error: " +
                ex.Message);

            return new OrderResponse
            {
                Success = false,
                Message =
                    "Server failed to save order."
            };
        }
    }

    private async Task SendResponseAsync(
        StreamWriter writer,
        string remoteAddress,
        string localAddress,
        OrderResponse response)
    {
        var responseJson =
            JsonSerializer.Serialize(
                response);

        await writer.WriteLineAsync(
            responseJson);

        await WriteDeviceLogAsync(
            protocol: "TCP",
            sourceAddress: localAddress,
            content:
                $"OUT to {remoteAddress}: " +
                responseJson);
    }

    private async Task PrintWaitingTicketsAsync()
    {
        await using var db =
            CreateDbContext();

        var tickets =
            await db.OrderTickets
                .Where(x =>
                    x.Status == "Waiting")
                .OrderBy(x =>
                    x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

        Console.WriteLine();
        Console.WriteLine(
            "=========== WAITING TICKETS ===========");

        Console.WriteLine(
            $"{"ID",-6}" +
            $"{"Counter",-15}" +
            $"{"Total",15}" +
            $"{"Created",12}");

        foreach (var ticket in tickets)
        {
            Console.WriteLine(
                $"{ticket.OrderTicketId,-6}" +
                $"{ticket.CounterName,-15}" +
                $"{ticket.TotalAmount,15:N0}" +
                $"{ticket.CreatedAt,12:HH:mm:ss}");
        }

        Console.WriteLine(
            "=======================================");
    }

    private async Task WriteDeviceLogAsync(
        string protocol,
        string sourceAddress,
        string content)
    {
        await using var db =
            CreateDbContext();

        if (content.Length > 4000)
        {
            content =
                content[..4000];
        }

        db.DeviceLogs.Add(
            new DeviceLog
            {
                Protocol =
                    protocol,

                SourceAddress =
                    sourceAddress,

                Content =
                    content,

                CreatedAt =
                    DateTime.Now
            });

        await db.SaveChangesAsync();
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