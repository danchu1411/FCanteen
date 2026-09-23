using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Services.Models;

namespace FCanteen.Services.Interfaces;

public interface IReportService
{
    Task<DailySummaryResult>
        GetDailySummaryAsync(
            DateTime date,
            string branchCode,
            CancellationToken cancellationToken =
                default);
}
