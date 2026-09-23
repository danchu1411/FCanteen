using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RaceConditionResult
{
    public string VersionName { get; set; } =
        string.Empty;

    public int OrderCount { get; set; }

    public decimal InitialStock { get; set; }

    public decimal TotalConsumed { get; set; }

    public decimal ExpectedStock { get; set; }

    public decimal ActualStock { get; set; }

    public double ElapsedMilliseconds { get; set; }

    public decimal Difference =>
        ActualStock - ExpectedStock;

    public bool IsCorrect =>
        ActualStock == ExpectedStock;
}
