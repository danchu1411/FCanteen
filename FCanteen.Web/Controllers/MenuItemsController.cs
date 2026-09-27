using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
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

    public MenuItemsController(
        IMenuItemRepository menuItemRepository,
        ICategoryRepository categoryRepository)
    {
        _menuItemRepository =
            menuItemRepository;

        _categoryRepository =
            categoryRepository;
    }

    /*
     * INDEX
     */
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var menuItems =
            await _menuItemRepository
                .GetAllAsync(
                    cancellationToken);

        return View(
            menuItems);
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
            "Đã xoá món thành công.";

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
}