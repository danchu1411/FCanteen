using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class DeviceLog
{
    public int DeviceLogId { get; set; }

    public string Protocol { get; set; } = string.Empty;

    public string SourceAddress { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
