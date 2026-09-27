using FCanteen.Data.Entities;

namespace FCanteen.Web.Models;

public class IngredientPurchaseIndexViewModel
{
    public IReadOnlyList<Ingredient>
        Ingredients
    {
        get;
        set;
    } = [];

    public int CartLineCount
    {
        get;
        set;
    }
}