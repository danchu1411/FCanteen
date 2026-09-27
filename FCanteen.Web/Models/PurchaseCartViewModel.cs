namespace FCanteen.Web.Models;

public class PurchaseCartViewModel
{
    public IReadOnlyList<PurchaseCartItem>
        Items
    {
        get;
        set;
    } = [];

    public int LineCount =>
        Items.Count;

    public decimal TotalAmount =>
        Items.Sum(
            x => x.LineTotal);
}
