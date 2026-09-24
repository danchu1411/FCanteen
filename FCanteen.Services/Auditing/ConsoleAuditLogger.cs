using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Services.Contexts;

namespace FCanteen.Services.Auditing;

public class ConsoleAuditLogger
    : IAuditLogger
{
    public Task LogAsync(
        string action,
        string details,
        CancellationToken cancellationToken =
            default)
    {
        cancellationToken
            .ThrowIfCancellationRequested();

        var staff =
            StaffAmbientContext.Current;

        Console.WriteLine();
        Console.WriteLine(
            "============== AUDIT LOG ==============");

        Console.WriteLine(
            $"Action : {action}");

        if (staff is null)
        {
            Console.WriteLine(
                "Staff  : NOT SET");
        }
        else
        {
            Console.WriteLine(
                $"Staff  : " +
                $"{staff.StaffCode} - " +
                $"{staff.FullName}");

            Console.WriteLine(
                $"Role   : {staff.Role}");

            Console.WriteLine(
                $"Branch : {staff.BranchCode}");
        }

        Console.WriteLine(
            $"Details: {details}");

        Console.WriteLine(
            $"Time   : " +
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        Console.WriteLine(
            "=======================================");

        return Task.CompletedTask;
    }
}