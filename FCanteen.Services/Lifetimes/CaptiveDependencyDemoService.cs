using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Services.Lifetimes;

public class CaptiveDependencyDemoService
{
    private readonly FCanteenContext _db;

    public Guid InstanceId { get; } =
        Guid.NewGuid();

    public CaptiveDependencyDemoService(
        FCanteenContext db)
    {
        _db = db;
    }

    public Task<int> GetMenuItemCountAsync(
        CancellationToken cancellationToken =
            default)
    {
        return _db.MenuItems.CountAsync(
            cancellationToken);
    }
}
