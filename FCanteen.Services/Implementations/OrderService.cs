using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Implementations;

public class OrderService
    : IOrderService
{
    private readonly IOrderRepository
        _orderRepository;

    private readonly IMenuItemRepository
        _menuItemRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IMenuItemRepository menuItemRepository)
    {
        _orderRepository =
            orderRepository;

        _menuItemRepository =
            menuItemRepository;
    }

    public Task<OrderTicket?>
        GetOrderAsync(
            int orderTicketId,
            CancellationToken cancellationToken =
                default)
    {
        return _orderRepository
            .GetByIdAsync(
                orderTicketId,
                cancellationToken);
    }

    public Task<IReadOnlyList<MenuItem>>
        GetAvailableMenuAsync(
            CancellationToken cancellationToken =
                default)
    {
        return _menuItemRepository
            .GetAvailableAsync(
                cancellationToken);
    }
}
