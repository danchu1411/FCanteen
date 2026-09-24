using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Auditing;

public interface IAuditLogger
{
    Task LogAsync(
        string action,
        string details,
        CancellationToken cancellationToken =
            default);
}
