using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.ConsoleApp.Models;

public class LifetimeResult
{
    public string ScopeName { get; set; } =
        string.Empty;

    public Guid Transient1 { get; set; }

    public Guid Transient2 { get; set; }

    public Guid Scoped1 { get; set; }

    public Guid Scoped2 { get; set; }

    public Guid Singleton1 { get; set; }

    public Guid Singleton2 { get; set; }
}
