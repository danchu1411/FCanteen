using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class Ingredient
{
    public int IngredientId { get; set; }

    public string Name { get; set; } =
        string.Empty;

    public string Unit { get; set; } =
        string.Empty;

    public decimal StockQuantity { get; set; }

    public decimal AlertThreshold { get; set; }

    /*
     * Giá vốn trên một đơn vị nguyên liệu.
     *
     * Cần cho YC2/YC5 Lab04.
     */
    public decimal UnitCost { get; set; }

    /*
     * Một nguyên liệu có thể được cung cấp
     * bởi một Supplier.
     */
    public int? SupplierId { get; set; }

    public Supplier? Supplier { get; set; }

    public ICollection<MenuItemIngredient>
        MenuItemIngredients
    { get; set; } = [];
}
