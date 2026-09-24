using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;

namespace FCanteen.Services.Contexts;

public static class StaffAmbientContext
{
    private static readonly AsyncLocal<Staff?>
        CurrentStaffHolder = new();

    public static Staff? Current
    {
        get =>
            CurrentStaffHolder.Value;

        set =>
            CurrentStaffHolder.Value = value;
    }

    public static void Clear()
    {
        CurrentStaffHolder.Value = null;
    }
}
