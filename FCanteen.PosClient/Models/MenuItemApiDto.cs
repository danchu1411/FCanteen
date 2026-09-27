using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.PosClient.Models;

public class MenuItemApiDto
{
    public int MenuItemId
    {
        get;
        set;
    }

    public string Code
    {
        get;
        set;
    } = string.Empty;

    public string Name
    {
        get;
        set;
    } = string.Empty;

    public decimal Price
    {
        get;
        set;
    }

    public string Unit
    {
        get;
        set;
    } = string.Empty;

    public bool IsAvailable
    {
        get;
        set;
    }
}
