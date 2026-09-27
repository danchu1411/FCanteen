namespace FCanteen.Web.Models;

public class PurchaseCartItem
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

    public string SupplierName
    {
        get;
        set;
    } = string.Empty;

    public decimal UnitCost
    {
        get;
        set;
    }

    public decimal Quantity
    {
        get;
        set;
    }

    public decimal LineTotal =>
        UnitCost *
        Quantity;
}
