using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text;

namespace FCanteen.Services.Notifications.Implementations;

public class FileNotificationService
    : INotificationService
{
    private readonly string _filePath;

    public FileNotificationService()
    {
        _filePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "notifications.log");
    }

    public async Task SendAsync(
        string subject,
        string message,
        CancellationToken cancellationToken =
            default)
    {
        var content =
            new StringBuilder()
                .AppendLine(
                    "========== FILE NOTIFICATION ==========")
                .AppendLine(
                    $"Time   : {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                .AppendLine(
                    $"Subject: {subject}")
                .AppendLine(
                    $"Message: {message}")
                .AppendLine(
                    "=======================================")
                .AppendLine()
                .ToString();

        await File.AppendAllTextAsync(
            _filePath,
            content,
            cancellationToken);

        Console.WriteLine();
        Console.WriteLine(
            "Notification was written to file:");

        Console.WriteLine(
            _filePath);
    }
}
