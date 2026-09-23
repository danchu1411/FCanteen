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

        var udpPort =
            configuration
                .GetValue<int>(
                    "Networking:UdpPort");

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

        var commandService =
            new KitchenCommandService(
                connectionString,
                udpPort);

        var tcpTask =
            server.RunAsync(
                cancellationTokenSource.Token);

        var commandTask =
            commandService.RunAsync();

        await Task.WhenAll(
            tcpTask,
            commandTask);
    }
}