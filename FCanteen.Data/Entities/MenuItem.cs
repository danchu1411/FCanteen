using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class MenuItem
{
    public int MenuItemId { get; set; }

    /*
     * YC2 Lab04 cần mã món
     * dạng MON-0001.
     */
    public string Code { get; set; } =
        string.Empty;

    public string Name { get; set; } =
        string.Empty;

    public decimal Price { get; set; }

    public string Unit { get; set; } =
        string.Empty;

    public bool IsAvailable { get; set; } =
        true;

    /*
     * Nullable ở tầng DB để migration
     * an toàn với dữ liệu Lab01-Lab03.
     *
     * YC2 sau này form sẽ bắt buộc
     * chọn Category.
     */
    public int? CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<TicketLine>
        TicketLines
    { get; set; } = [];

    public ICollection<MenuItemIngredient>
        MenuItemIngredients
    { get; set; } = [];
}
