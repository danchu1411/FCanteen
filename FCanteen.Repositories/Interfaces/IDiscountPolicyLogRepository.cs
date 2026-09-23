using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces;

public interface IDiscountPolicyLogRepository
{
    Task AddRangeAsync(
        IEnumerable<DiscountPolicyLog> logs,
        CancellationToken cancellationToken =
            default);
}
