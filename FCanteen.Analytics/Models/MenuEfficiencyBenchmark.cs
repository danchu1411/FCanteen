using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics.Models;

public class MenuEfficiencyBenchmark
{
    public string MethodName { get; set; } =
        string.Empty;

    public int? MaxDegreeOfParallelism { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public double Speedup { get; set; }

    public List<MenuEfficiencyResult> Results { get; set; }
        = [];
}
