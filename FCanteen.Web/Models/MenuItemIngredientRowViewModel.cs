namespace FCanteen.Web.Models;

public class MenuItemIngredientRowViewModel
{
    public int IngredientId
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    } = string.Empty;

    public string Unit
    {
        get;
        set;
    } = string.Empty;

    public decimal UnitCost
    {
        get;
        set;
    }

    public bool IsSelected
    {
        get;
        set;
    }

    public decimal Quantity
    {
        get;
        set;
    }

    public decimal LineCost =>
        IsSelected
            ? Quantity * UnitCost
            : 0m;
}
