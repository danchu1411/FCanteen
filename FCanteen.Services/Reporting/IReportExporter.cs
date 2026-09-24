using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Reporting;

public interface IReportExporter
{
    Task ExportAsync(
        string reportName,
        string content,
        CancellationToken cancellationToken =
            default);
}