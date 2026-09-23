using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class Staff
{
    public int StaffId { get; set; }

    public string StaffCode { get; set; } =
        string.Empty;

    public string FullName { get; set; } =
        string.Empty;

    public string Role { get; set; } =
        string.Empty;

    public string BranchCode { get; set; } =
        string.Empty;
}
