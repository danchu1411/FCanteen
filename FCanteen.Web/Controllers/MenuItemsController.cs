using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Interfaces;
using FCanteen.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FCanteen.Web.Controllers;

public class MenuItemsController
    : Controller
{
    private readonly IMenuItemRepository
        _menuItemRepository;

    private readonly ICategoryRepository
        _categoryRepository;

    private readonly IInventoryService
        _inventoryService;

    public MenuItemsController(
        IMenuItemRepository menuItemRepository,
        ICategoryRepository categoryRepository,
        IInventoryService inventoryService)
    {
        _menuItemRepository =
            menuItemRepository;

        _categoryRepository =
            categoryRepository;

        _inventoryService =
            inventoryService;
    }

    /*
     * INDEX
     */
    public async Task<IActionResult> Index(
    string? searchTerm = null,
    int? categoryId = null,
    bool? isAvailable = null,
    string? sortBy = null,
    int page = 1,
    CancellationToken cancellationToken =
        default)
    {
        const int pageSize =
            5;

        /*
         * Chỉ chấp nhận 4 kiểu sort
         * mà YC3 yêu cầu.
         */
        sortBy =
            sortBy switch
            {
                "name_desc" =>
                    "name_desc",

                "price_asc" =>
                    "price_asc",

                "price_desc" =>
                    "price_desc",

                _ =>
                    "name_asc"
            };

        var result =
            await _menuItemRepository
                .SearchAsync(
                    searchTerm,
                    categoryId,
                    isAvailable,
                    sortBy,
                    page,
                    pageSize,
                    cancellationToken);

        var categories =
            await _categoryRepository
                .GetAllAsync(
                    cancellationToken);

        var model =
            new MenuItemIndexViewModel
            {
                MenuItems =
                    result.Items,

                Categories =
                    categories,

                SearchTerm =
                    searchTerm,

                CategoryId =
                    categoryId,

                IsAvailable =
                    isAvailable,

                SortBy =
                    sortBy,

                Page =
                    result.Page,

                PageSize =
                    result.PageSize,

                TotalCount =
                    result.TotalCount,

                TotalPages =
                    result.TotalPages
            };

        /*
         * Dùng ViewData có chủ đích.
         */
        ViewData["ResultSummary"] =
            $"Tìm thấy " +
            $"{result.TotalCount} món.";

        return View(
            model);
    }

    /*
     * DETAILS
     */
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var menuItem =
            await _menuItemRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (menuItem is null)
        {
            return NotFound();
        }

        ViewBag.CostPrice =
            await _menuItemRepository
                .CalculateCostAsync(
                    id,
                    cancellationToken);

        return View(
            menuItem);
    }

    /*
     * CREATE GET
     */
    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(
            null,
            cancellationToken);

        var model =
            new MenuItemFormViewModel
            {
                IsAvailable = true,
                CostPrice = 0m
            };

        return View(
            model);
    }

    /*
     * CREATE POST
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        MenuItemFormViewModel model,
        CancellationToken cancellationToken)
    {
        /*
         * Chưa có ingredient assignment
         * cho món mới ở YC2.
         *
         * YC5 sẽ bổ sung.
         */
        model.CostPrice =
            0m;

        /*
         * Model binding đã validate trước,
         * nhưng ta chủ động validate lại
         * sau khi server xác định CostPrice.
         */
        ModelState.Clear();

        TryValidateModel(
            model);

        /*
         * YC2 yêu cầu kiểm tra mã trùng
         * ở server bằng ModelState.
         */
        var duplicateCode =
            await _menuItemRepository
                .CodeExistsAsync(
                    model.Code,
                    null,
                    cancellationToken);

        if (duplicateCode)
        {
            ModelState.AddModelError(
                nameof(model.Code),
                "Mã món đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(
                model.CategoryId,
                cancellationToken);

            return View(
                model);
        }

        var menuItem =
            new MenuItem
            {
                Code =
                    model.Code,
                Name =
                    model.Name,
                Price =
                    model.Price,
                Unit =
                    model.Unit,
                IsAvailable =
                    model.IsAvailable,
                CategoryId =
                    model.CategoryId
            };

        await _menuItemRepository
            .AddAsync(
                menuItem,
                cancellationToken);

        TempData["SuccessMessage"] =
            $"Đã tạo món " +
            $"{menuItem.Code} thành công.";

        return RedirectToAction(
            nameof(Index));
    }

    /*
     * EDIT GET
     */
    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var menuItem =
            await _menuItemRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (menuItem is null)
        {
            return NotFound();
        }

        var cost =
            await _menuItemRepository
                .CalculateCostAsync(
                    id,
                    cancellationToken);

        var model =
            new MenuItemFormViewModel
            {
                MenuItemId =
                    menuItem.MenuItemId,
                Code =
                    menuItem.Code,
                Name =
                    menuItem.Name,
                Price =
                    menuItem.Price,
                Unit =
                    menuItem.Unit,
                IsAvailable =
                    menuItem.IsAvailable,
                CategoryId =
                    menuItem.CategoryId,
                CostPrice =
                    cost
            };

        await LoadCategoriesAsync(
            model.CategoryId,
            cancellationToken);

        return View(
            model);
    }

    /*
     * EDIT POST
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        MenuItemFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (id !=
            model.MenuItemId)
        {
            return BadRequest();
        }

        var existing =
            await _menuItemRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (existing is null)
        {
            return NotFound();
        }

        /*
         * Không tin CostPrice từ browser.
         *
         * Server tính lại từ DB.
         */
        model.CostPrice =
            await _menuItemRepository
                .CalculateCostAsync(
                    id,
                    cancellationToken);

        ModelState.Clear();

        TryValidateModel(
            model);

        var duplicateCode =
            await _menuItemRepository
                .CodeExistsAsync(
                    model.Code,
                    id,
                    cancellationToken);

        if (duplicateCode)
        {
            ModelState.AddModelError(
                nameof(model.Code),
                "Mã món đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(
                model.CategoryId,
                cancellationToken);

            return View(
                model);
        }

        var updatedMenuItem =
            new MenuItem
            {
                MenuItemId =
                    id,
                Code =
                    model.Code,
                Name =
                    model.Name,
                Price =
                    model.Price,
                Unit =
                    model.Unit,
                IsAvailable =
                    model.IsAvailable,
                CategoryId =
                    model.CategoryId
            };

        await _menuItemRepository
            .UpdateAsync(
                updatedMenuItem,
                cancellationToken);

        TempData["SuccessMessage"] =
            $"Đã cập nhật món " +
            $"{updatedMenuItem.Code}.";

        return RedirectToAction(
            nameof(Index));
    }

    /*
     * DELETE GET
     */
    [HttpGet]
    public async Task<IActionResult> Delete(
    int id,
    CancellationToken cancellationToken)
    {
        var menuItem =
            await _menuItemRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (menuItem is null)
        {
            return NotFound();
        }

        ViewBag.HasTicketLines =
            await _menuItemRepository
                .HasTicketLinesAsync(
                    id,
                    cancellationToken);

        return View(
            menuItem);
    }

    /*
     * DELETE POST
     */
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>
    DeleteConfirmed(
        int id,
        CancellationToken cancellationToken)
    {
        var menuItem =
            await _menuItemRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (menuItem is null)
        {
            return NotFound();
        }

        var hasTicketLines =
            await _menuItemRepository
                .HasTicketLinesAsync(
                    id,
                    cancellationToken);

        if (hasTicketLines)
        {
            TempData["ErrorMessage"] =
                $"Không thể xoá món " +
                $"{menuItem.Code} vì món này " +
                $"đã xuất hiện trong hoá đơn.";

            return RedirectToAction(
                nameof(Index));
        }

        var deleted =
            await _menuItemRepository
                .DeleteAsync(
                    id,
                    cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            $"Đã xoá món " +
            $"{menuItem.Code} thành công.";

        return RedirectToAction(
            nameof(Index));
    }

    private async Task
        LoadCategoriesAsync(
            int? selectedCategoryId,
            CancellationToken cancellationToken)
    {
        var categories =
            await _categoryRepository
                .GetAllAsync(
                    cancellationToken);

        ViewBag.Categories =
            new SelectList(
                categories,
                nameof(Category.CategoryId),
                nameof(Category.Name),
                selectedCategoryId);
    }

    [HttpGet]
    public async Task<IActionResult> Ingredients(
        int id,
        CancellationToken cancellationToken)
    {
        var menuItem =
            await _menuItemRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (menuItem is null)
        {
            return NotFound();
        }

        var ingredients =
            await _inventoryService
                .GetIngredientsAsync(
                    cancellationToken);

        var currentQuantities =
            menuItem.MenuItemIngredients
                .ToDictionary(
                    x => x.IngredientId,
                    x => x.Quantity);

        var model =
            new MenuItemIngredientsViewModel
            {
                MenuItemId =
                    menuItem.MenuItemId,

                Code =
                    menuItem.Code,

                Name =
                    menuItem.Name,

                SellingPrice =
                    menuItem.Price,

                Ingredients =
                    ingredients
                        .Select(
                            ingredient =>
                            {
                                var selected =
                                    currentQuantities
                                        .TryGetValue(
                                            ingredient.IngredientId,
                                            out var quantity);

                                return new
                                    MenuItemIngredientRowViewModel
                                {
                                    IngredientId =
                                            ingredient.IngredientId,

                                    Name =
                                            ingredient.Name,

                                    Unit =
                                            ingredient.Unit,

                                    UnitCost =
                                            ingredient.UnitCost,

                                    IsSelected =
                                            selected,

                                    Quantity =
                                            selected
                                                ? quantity
                                                : 0m
                                };
                            })
                        .ToList()
            };

        return View(
            model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ingredients(
        int id,
        MenuItemIngredientsViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.MenuItemId)
        {
            return BadRequest();
        }

        var menuItem =
            await _menuItemRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (menuItem is null)
        {
            return NotFound();
        }

        var ingredients =
            await _inventoryService
                .GetIngredientsAsync(
                    cancellationToken);

        var ingredientMap =
            ingredients.ToDictionary(
                x => x.IngredientId);

        var quantities =
            new Dictionary<int, decimal>();

        decimal costPrice =
            0m;

        for (var index = 0;
             index < model.Ingredients.Count;
             index++)
        {
            var row =
                model.Ingredients[index];

            if (!row.IsSelected)
            {
                continue;
            }

            if (!ingredientMap.TryGetValue(
                row.IngredientId,
                out var ingredient))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Có nguyên liệu không hợp lệ.");

                continue;
            }

            if (row.Quantity <= 0)
            {
                ModelState.AddModelError(
                    $"Ingredients[{index}].Quantity",
                    "Định lượng phải lớn hơn 0.");

                continue;
            }

            quantities[
                ingredient.IngredientId] =
                    row.Quantity;

            costPrice +=
                row.Quantity *
                ingredient.UnitCost;
        }

        if (quantities.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "Vui lòng chọn ít nhất một nguyên liệu.");
        }

        var minimumSellingPrice =
            costPrice *
            1.20m;

        /*
         * Giữ nhất quán với YC2:
         * sau khi đổi recipe,
         * giá bán vẫn phải >= cost + 20%.
         */
        if (costPrice > 0 &&
            menuItem.Price <
            minimumSellingPrice)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Giá vốn mới là " +
                $"{costPrice:N0} VND. " +
                $"Giá bán hiện tại phải ít nhất " +
                $"{minimumSellingPrice:N0} VND.");
        }

        if (!ModelState.IsValid)
        {
            /*
             * Không tin Name/Unit/UnitCost
             * được POST từ browser.
             * Rebuild lại bằng dữ liệu DB.
             */
            foreach (var row
                in model.Ingredients)
            {
                if (ingredientMap.TryGetValue(
                    row.IngredientId,
                    out var ingredient))
                {
                    row.Name =
                        ingredient.Name;

                    row.Unit =
                        ingredient.Unit;

                    row.UnitCost =
                        ingredient.UnitCost;
                }
            }

            model.Code =
                menuItem.Code;

            model.Name =
                menuItem.Name;

            model.SellingPrice =
                menuItem.Price;

            return View(
                model);
        }

        await _menuItemRepository
            .ReplaceIngredientsAsync(
                id,
                quantities,
                cancellationToken);

        TempData["SuccessMessage"] =
            $"Đã cập nhật nguyên liệu cho " +
            $"{menuItem.Code}. " +
            $"Giá vốn: {costPrice:N0} VND.";

        return RedirectToAction(
            nameof(Ingredients),
            new
            {
                id
            });
    }

    [HttpGet("/api/menu-items/available")]
    public async Task<JsonResult> AvailableJson(
    CancellationToken cancellationToken)
    {
        var menuItems =
            await _menuItemRepository
                .GetAvailableAsync(
                    cancellationToken);

        var result =
            menuItems.Select(
                x => new
                {
                    menuItemId =
                        x.MenuItemId,

                    code =
                        x.Code,

                    name =
                        x.Name,

                    price =
                        x.Price,

                    unit =
                        x.Unit,

                    isAvailable =
                        x.IsAvailable
                });

        return Json(
            result);
    }
}