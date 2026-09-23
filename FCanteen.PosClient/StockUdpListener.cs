using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FCanteen.Data.Contracts;

namespace FCanteen.PosClient;

public class StockUdpListener
{
    private readonly int _udpPort;

    private readonly
        ConcurrentDictionary<int, bool>
        _availability;

    public StockUdpListener(
        int udpPort,
        ConcurrentDictionary<int, bool>
            availability)
    {
        _udpPort =
            udpPort;

        _availability =
            availability;
    }

    public async Task RunAsync(
        CancellationToken cancellationToken)
    {
        using var udpClient =
            new UdpClient();

        udpClient.ExclusiveAddressUse =
            false;

        udpClient.Client.SetSocketOption(
            SocketOptionLevel.Socket,
            SocketOptionName.ReuseAddress,
            true);

        udpClient.Client.Bind(
            new IPEndPoint(
                IPAddress.Any,
                _udpPort));

        Console.WriteLine(
            $"UDP listener started on port " +
            $"{_udpPort}.");

        while (!cancellationToken
                   .IsCancellationRequested)
        {
            try
            {
                var result =
                    await udpClient
                        .ReceiveAsync(
                            cancellationToken);

                var json =
                    Encoding.UTF8
                        .GetString(
                            result.Buffer);

                var message =
                    JsonSerializer
                        .Deserialize
                        <StockStatusMessage>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive =
                                    true
                            });

                if (message is null)
                {
                    continue;
                }

                _availability[
                    message.MenuItemId] =
                    message.IsAvailable;

                Console.WriteLine();

                Console.WriteLine(
                    "******** STOCK UPDATE ********");

                Console.WriteLine(
                    $"{message.MenuItemName} " +
                    $"(ID {message.MenuItemId})");

                Console.WriteLine(
                    message.IsAvailable
                        ? "Status: AVAILABLE"
                        : "Status: OUT OF STOCK");

                Console.WriteLine(
                    "******************************");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"UDP error: {ex.Message}");
            }
        }
    }
}