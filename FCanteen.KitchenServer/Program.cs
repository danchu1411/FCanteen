using Microsoft.Extensions.Configuration;

namespace FCanteen.KitchenServer;

internal class Program
{
    static async Task Main(string[] args)
    {
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
                .GetConnectionString(
                    "FCanteen")
            ?? throw new
                InvalidOperationException(
                    "Connection string " +
                    "'FCanteen' was not found.");

        var tcpPort =
            configuration
                .GetValue<int>(
                    "Networking:TcpPort");

        using var cancellationTokenSource =
            new CancellationTokenSource();

        Console.CancelKeyPress +=
            (_, eventArgs) =>
            {
                eventArgs.Cancel = true;

                cancellationTokenSource.Cancel();
            };

        var server =
            new KitchenTcpServer(
                connectionString,
                tcpPort);

        await server.RunAsync(
            cancellationTokenSource.Token);
    }
}