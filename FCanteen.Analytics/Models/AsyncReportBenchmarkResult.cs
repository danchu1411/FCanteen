using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class AsyncReportBenchmarkResult
{
    public DailyReportResult SequentialReport { get; set; }
        = new();

    public DailyReportResult ConcurrentReport { get; set; }
        = new();

    public double SequentialMilliseconds { get; set; }

    public double ConcurrentMilliseconds { get; set; }

    public double Speedup { get; set; }

    public bool ResultsMatch { get; set; }
}
