using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Services.Models;

namespace FCanteen.Services.Interfaces;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderConfirmation>
        ConfirmAsync(
            IReadOnlyList<
                PurchaseOrderLineRequest> lines,
            CancellationToken cancellationToken =
                default);
}
