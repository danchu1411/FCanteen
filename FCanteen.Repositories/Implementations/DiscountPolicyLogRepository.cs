using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;

namespace FCanteen.Repositories.Implementations;

public class DiscountPolicyLogRepository
    : IDiscountPolicyLogRepository
{
    private readonly FCanteenContext _db;

    public DiscountPolicyLogRepository(
        FCanteenContext db)
    {
        _db = db;
    }

    public async Task AddRangeAsync(
        IEnumerable<DiscountPolicyLog> logs,
        CancellationToken cancellationToken =
            default)
    {
        await _db.DiscountPolicyLogs
            .AddRangeAsync(
                logs,
                cancellationToken);

        await _db.SaveChangesAsync(
            cancellationToken);
    }
}
