using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Notifications;

public interface INotificationService
{
    Task SendAsync(
        string subject,
        string message,
        CancellationToken cancellationToken =
            default);
}
