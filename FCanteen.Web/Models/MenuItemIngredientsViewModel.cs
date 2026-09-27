namespace FCanteen.Web.Models;

public class MenuItemIngredientsViewModel
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

    public decimal SellingPrice
    {
        get;
        set;
    }

    public List<MenuItemIngredientRowViewModel>
        Ingredients
    {
        get;
        set;
    } = [];

    public decimal CostPrice =>
        Ingredients.Sum(
            x => x.LineCost);

    public decimal MinimumSellingPrice =>
        CostPrice * 1.20m;
}
