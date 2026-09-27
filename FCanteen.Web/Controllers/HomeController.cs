using System.Diagnostics;

using FCanteen.Repositories.Interfaces;
using FCanteen.Web.Models;

using Microsoft.AspNetCore.Mvc;

namespace FCanteen.Web.Controllers;

public class HomeController
    : Controller
{
    private readonly ILogger<HomeController>
        _logger;

    private readonly IMenuItemRepository
        _menuItemRepository;

    public HomeController(
        ILogger<HomeController> logger,
        IMenuItemRepository menuItemRepository)
    {
        _logger =
            logger;

        _menuItemRepository =
            menuItemRepository;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var menuItems =
            await _menuItemRepository
                .GetAllAsync(
                    cancellationToken);

        var model =
            new HomeDashboardViewModel
            {
                MenuItemCount =
                    menuItems.Count
            };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location =
            ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id
                    ?? HttpContext
                        .TraceIdentifier
            });
    }
}