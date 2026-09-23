using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Notifications.Implementations;

public class ConsoleNotificationService
    : INotificationService
{
    public Task SendAsync(
        string subject,
        string message,
        CancellationToken cancellationToken =
            default)
    {
        cancellationToken
            .ThrowIfCancellationRequested();

        Console.WriteLine();
        Console.WriteLine(
            "========== CONSOLE NOTIFICATION ==========");

        Console.WriteLine(
            $"Subject: {subject}");

        Console.WriteLine(
            $"Message: {message}");

        Console.WriteLine(
            "==========================================");

        return Task.CompletedTask;
    }
}