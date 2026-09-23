using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FCanteen.Data;
using FCanteen.Data.Contracts;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.KitchenServer;

public class PriceSyncService
{
    private readonly string _connectionString;

    private readonly HttpClient _httpClient =
        new();

    public PriceSyncService(
        string connectionString)
    {
        _connectionString =
            connectionString;
    }

    public async Task SyncAsync(
        string url)
    {
        if (string.IsNullOrWhiteSpace(
                url))
        {
            Console.WriteLine(
                "Price sync URL is empty.");

            return;
        }

        // ===== URI =====

        var uri =
            new Uri(url);

        Console.WriteLine();
        Console.WriteLine(
            "========== URI INFORMATION ==========");

        Console.WriteLine(
            $"Scheme : {uri.Scheme}");

        Console.WriteLine(
            $"Host   : {uri.Host}");

        Console.WriteLine(
            $"Port   : {uri.Port}");

        // ===== DNS =====

        Console.WriteLine();
        Console.WriteLine(
            "Resolving DNS...");

        var ipAddresses =
            await Dns
                .GetHostAddressesAsync(
                    uri.Host);

        foreach (var ipAddress
                 in ipAddresses)
        {
            Console.WriteLine(
                $"IP     : {ipAddress}");
        }

        await WriteLogAsync(
            "DNS",
            uri.Host,
            string.Join(
                ", ",
                ipAddresses.Select(
                    x => x.ToString())));

        // ===== HTTP =====

        Console.WriteLine();
        Console.WriteLine(
            $"HTTP GET {uri}");

        var stopwatch =
            Stopwatch.StartNew();

        try
        {
            using var response =
                await _httpClient
                    .GetAsync(uri);

            var json =
                await response.Content
                    .ReadAsStringAsync();

            stopwatch.Stop();

            Console.WriteLine(
                $"Status : " +
                $"{(int)response.StatusCode} " +
                $"{response.StatusCode}");

            Console.WriteLine(
                $"Time   : " +
                $"{stopwatch.ElapsedMilliseconds} ms");

            await WriteLogAsync(
                "HTTP",
                uri.Host,
                $"GET {uri}; " +
                $"Status={(int)response.StatusCode} " +
                $"{response.StatusCode}; " +
                $"ResponseTime=" +
                $"{stopwatch.ElapsedMilliseconds}ms");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    "HTTP request failed.");

                return;
            }

            var remoteItems =
                JsonSerializer
                    .Deserialize
                    <List<RemotePriceItem>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive =
                                true
                        });

            if (remoteItems is null ||
                remoteItems.Count == 0)
            {
                Console.WriteLine(
                    "Remote JSON is empty.");

                return;
            }

            await UpdatePricesAsync(
                remoteItems);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await WriteLogAsync(
                "HTTP",
                uri.Host,
                $"GET {uri}; " +
                $"ERROR={ex.Message}; " +
                $"ResponseTime=" +
                $"{stopwatch.ElapsedMilliseconds}ms");

            Console.WriteLine(
                $"HTTP error: {ex.Message}");
        }
    }

    private async Task UpdatePricesAsync(
        List<RemotePriceItem> remoteItems)
    {
        await using var db =
            CreateDbContext();

        var itemIds =
            remoteItems
                .Select(x => x.MenuItemId)
                .Distinct()
                .ToList();

        var localItems =
            await db.MenuItems
                .Where(x =>
                    itemIds.Contains(
                        x.MenuItemId))
                .ToDictionaryAsync(
                    x => x.MenuItemId);

        var changedCount = 0;

        Console.WriteLine();
        Console.WriteLine(
            "========== PRICE CHANGES ==========");

        foreach (var remoteItem
                 in remoteItems)
        {
            if (!localItems.TryGetValue(
                    remoteItem.MenuItemId,
                    out var localItem))
            {
                Console.WriteLine(
                    $"ID {remoteItem.MenuItemId}: " +
                    "not found locally.");

                continue;
            }

            if (localItem.Price ==
                remoteItem.Price)
            {
                continue;
            }

            Console.WriteLine(
                $"{localItem.Name}: " +
                $"{localItem.Price:N0} -> " +
                $"{remoteItem.Price:N0}");

            localItem.Price =
                remoteItem.Price;

            changedCount++;
        }

        await db.SaveChangesAsync();

        Console.WriteLine(
            $"Updated {changedCount} item(s).");

        Console.WriteLine(
            "===================================");
    }

    private async Task WriteLogAsync(
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