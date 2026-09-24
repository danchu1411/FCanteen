using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Services.Reporting;

public class ConsoleReportExporter
    : IReportExporter
{
    public Task ExportAsync(
        string reportName,
        string content,
        CancellationToken cancellationToken =
            default)
    {
        cancellationToken
            .ThrowIfCancellationRequested();

        Console.WriteLine();
        Console.WriteLine(
            "==========================================");

        Console.WriteLine(
            $" REPORT EXPORT: {reportName}");

        Console.WriteLine(
            "==========================================");

        Console.WriteLine(
            content);

        Console.WriteLine(
            "==========================================");

        return Task.CompletedTask;
    }
}
