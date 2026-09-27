using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;

namespace FCanteen.Repositories.Implementations;

public class DeviceLogRepository
    : IDeviceLogRepository
{
    private readonly FCanteenContext _db;

    public DeviceLogRepository(
        FCanteenContext db)
    {
        _db =
            db;
    }

    public async Task AddAsync(
        DeviceLog deviceLog,
        CancellationToken cancellationToken =
            default)
    {
        _db.DeviceLogs.Add(
            deviceLog);

        await _db.SaveChangesAsync(
            cancellationToken);
    }
}
