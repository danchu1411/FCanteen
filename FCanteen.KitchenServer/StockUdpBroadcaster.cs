using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FCanteen.Data;
using FCanteen.Data.Contracts;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.KitchenServer;

public class StockUdpBroadcaster
{
    private readonly string _connectionString;
    private readonly int _udpPort;

    public StockUdpBroadcaster(
        string connectionString,
        int udpPort)
    {
        _connectionString = connectionString;
        _udpPort = udpPort;
    }

    public async Task BroadcastAsync(
        StockStatusMessage message)
    {
        using var udpClient =
            new UdpClient();

        udpClient.EnableBroadcast = true;

        var json =
            JsonSerializer.Serialize(message);

        var bytes =
            Encoding.UTF8.GetBytes(json);

        var endpoint =
            new IPEndPoint(
                IPAddress.Broadcast,
                _udpPort);

        await udpClient.SendAsync(
            bytes,
            endpoint);

        Console.WriteLine();
        Console.WriteLine(
            $"UDP broadcast: {json}");

        await WriteDeviceLogAsync(
            json);
    }

    private async Task WriteDeviceLogAsync(
        string content)
    {
        await using var db =
            CreateDbContext();

        db.DeviceLogs.Add(
            new DeviceLog
            {
                Protocol = "UDP",
                SourceAddress = "KitchenServer",
                Content = content,
                CreatedAt = DateTime.Now
            });

        await db.SaveChangesAsync();
    }

    private FCanteenContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<FCanteenContext>()
                .UseSqlServer(_connectionString)
                .Options;

        return new FCanteenContext(
            options);
    }
}