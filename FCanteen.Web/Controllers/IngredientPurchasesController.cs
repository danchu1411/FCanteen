using FCanteen.Services.Interfaces;
using FCanteen.Services.Models;
using FCanteen.Web.Extensions;
using FCanteen.Web.Models;

using Microsoft.AspNetCore.Mvc;

namespace FCanteen.Web.Controllers;

public class IngredientPurchasesController
    : Controller
{
    private const string
        CartSessionKey =
            "IngredientPurchaseCart";

    private const string
        CartCountSessionKey =
            "IngredientPurchaseCartCount";

    private readonly IInventoryService
        _inventoryService;

    private readonly IPurchaseOrderService
        _purchaseOrderService;

    public IngredientPurchasesController(
        IInventoryService inventoryService,
        IPurchaseOrderService purchaseOrderService)
    {
        _inventoryService =
            inventoryService;

        _purchaseOrderService =
            purchaseOrderService;
    }

    /*
     * Danh sách nguyên liệu.
     */
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var ingredients =
            await _inventoryService
                .GetIngredientsAsync(
                    cancellationToken);

        var cart =
            GetCart();

        var model =
            new IngredientPurchaseIndexViewModel
            {
                Ingredients =
                    ingredients,

                CartLineCount =
                    cart.Count
            };

        return View(
            model);
    }

    /*
     * Add vào Session cart.
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        int ingredientId,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        if (quantity <= 0)
        {
            TempData["PurchaseError"] =
                "Số lượng nhập phải lớn hơn 0.";

            return RedirectToAction(
                nameof(Index));
        }

        var ingredient =
            await _inventoryService
                .GetIngredientAsync(
                    ingredientId,
                    cancellationToken);

        if (ingredient is null)
        {
            return NotFound();
        }

        var cart =
            GetCart();

        var existingItem =
            cart.FirstOrDefault(
                x =>
                    x.IngredientId ==
                    ingredientId);

        if (existingItem is null)
        {
            cart.Add(
                new PurchaseCartItem
                {
                    IngredientId =
                        ingredient.IngredientId,

                    Name =
                        ingredient.Name,

                    Unit =
                        ingredient.Unit,

                    UnitCost =
                        ingredient.UnitCost,

                    SupplierName =
                        ingredient.Supplier?.Name
                        ?? "Chưa gán nhà cung cấp",

                    Quantity =
                        quantity
                });
        }
        else
        {
            /*
             * Cùng nguyên liệu:
             * tăng Quantity,
             * không tăng số dòng.
             */
            existingItem.Quantity +=
                quantity;
        }

        SaveCart(
            cart);

        TempData["PurchaseSuccess"] =
            $"Đã thêm {ingredient.Name} " +
            $"vào giỏ nhập.";

        return RedirectToAction(
            nameof(Index));
    }

    /*
     * Xem cart.
     */
    [HttpGet]
    public IActionResult Cart()
    {
        var model =
            new PurchaseCartViewModel
            {
                Items =
                    GetCart()
            };

        return View(
            model);
    }

    /*
     * Edit quantity.
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(
        int ingredientId,
        decimal quantity)
    {
        if (quantity <= 0)
        {
            TempData["PurchaseError"] =
                "Số lượng nhập phải lớn hơn 0.";

            return RedirectToAction(
                nameof(Cart));
        }

        var cart =
            GetCart();

        var item =
            cart.FirstOrDefault(
                x =>
                    x.IngredientId ==
                    ingredientId);

        if (item is null)
        {
            TempData["PurchaseError"] =
                "Không tìm thấy nguyên liệu " +
                "trong giỏ.";

            return RedirectToAction(
                nameof(Cart));
        }

        item.Quantity =
            quantity;

        SaveCart(
            cart);

        TempData["PurchaseSuccess"] =
            $"Đã cập nhật số lượng " +
            $"{item.Name}.";

        return RedirectToAction(
            nameof(Cart));
    }

    /*
     * Remove cart line.
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(
        int ingredientId)
    {
        var cart =
            GetCart();

        var item =
            cart.FirstOrDefault(
                x =>
                    x.IngredientId ==
                    ingredientId);

        if (item is not null)
        {
            cart.Remove(
                item);

            SaveCart(
                cart);

            TempData["PurchaseSuccess"] =
                $"Đã xoá {item.Name} " +
                $"khỏi giỏ nhập.";
        }

        return RedirectToAction(
            nameof(Cart));
    }

    /*
     * Confirm:
     * DB write + clear Session.
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(
        CancellationToken cancellationToken)
    {
        var cart =
            GetCart();

        if (cart.Count == 0)
        {
            TempData["PurchaseError"] =
                "Giỏ nhập đang trống.";

            return RedirectToAction(
                nameof(Cart));
        }

        var lines =
            cart
                .Select(
                    x =>
                        new PurchaseOrderLineRequest
                        {
                            IngredientId =
                                x.IngredientId,

                            Quantity =
                                x.Quantity
                        })
                .ToList();

        try
        {
            var confirmation =
                await _purchaseOrderService
                    .ConfirmAsync(
                        lines,
                        cancellationToken);

            /*
             * Chỉ clear Session
             * SAU KHI DB save thành công.
             */
            ClearCart();

            TempData["PurchaseSuccess"] =
                $"Đã xác nhận đơn " +
                $"{confirmation.OrderNumber}. " +
                $"Tổng tiền: " +
                $"{confirmation.TotalAmount:N0} VND.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PurchaseError"] =
                ex.Message;

            return RedirectToAction(
                nameof(Cart));
        }
    }

    private List<PurchaseCartItem>
        GetCart()
    {
        return HttpContext.Session
            .GetJson<List<PurchaseCartItem>>(
                CartSessionKey)
            ?? [];
    }

    private void SaveCart(
        List<PurchaseCartItem> cart)
    {
        if (cart.Count == 0)
        {
            ClearCart();
            return;
        }

        HttpContext.Session
            .SetJson(
                CartSessionKey,
                cart);

        /*
         * Navbar chỉ cần số dòng,
         * không cần deserialize cả cart.
         */
        HttpContext.Session
            .SetInt32(
                CartCountSessionKey,
                cart.Count);
    }

    private void ClearCart()
    {
        HttpContext.Session.Remove(
            CartSessionKey);

        HttpContext.Session.Remove(
            CartCountSessionKey);
    }
}