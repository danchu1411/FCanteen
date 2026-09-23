using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Notifications.Implementations;

public class FakeEmailNotificationService
    : INotificationService
{
    public async Task SendAsync(
        string subject,
        string message,
        CancellationToken cancellationToken =
            default)
    {
        /*
         * Giả lập thời gian gửi email.
         */
        await Task.Delay(
            150,
            cancellationToken);

        Console.WriteLine();
        Console.WriteLine(
            "============ FAKE EMAIL ============");

        Console.WriteLine(
            "To     : customer@fcanteen.local");

        Console.WriteLine(
            $"Subject: {subject}");

        Console.WriteLine(
            $"Body   : {message}");

        Console.WriteLine(
            "Status : SENT (SIMULATED)");

        Console.WriteLine(
            "====================================");
    }
}
