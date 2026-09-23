using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class PlinqBenchmarkResult
{
    public string ReportName { get; set; } =
        string.Empty;

    public double LinqMilliseconds { get; set; }

    public double PlinqMilliseconds { get; set; }

    public double Speedup { get; set; }

    public bool ResultsMatch { get; set; }
}
