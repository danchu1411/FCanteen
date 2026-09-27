using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;

using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Interfaces;
using FCanteen.Services.Models;

namespace FCanteen.Services.Implementations;

public class PurchaseOrderService
    : IPurchaseOrderService
{
    private readonly IIngredientRepository
        _ingredientRepository;

    private readonly IDeviceLogRepository
        _deviceLogRepository;

    public PurchaseOrderService(
        IIngredientRepository ingredientRepository,
        IDeviceLogRepository deviceLogRepository)
    {
        _ingredientRepository =
            ingredientRepository;

        _deviceLogRepository =
            deviceLogRepository;
    }

    public async Task<
        PurchaseOrderConfirmation>
        ConfirmAsync(
            IReadOnlyList<
                PurchaseOrderLineRequest> lines,
            CancellationToken cancellationToken =
                default)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException(
                "Giỏ nhập đang trống.");
        }

        var confirmedAt =
            DateTime.UtcNow;

        var orderNumber =
            $"PO-{confirmedAt:yyyyMMddHHmmssfff}";

        var persistedLines =
            new List<object>();

        decimal totalAmount =
            0m;

        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Số lượng nhập phải lớn hơn 0.");
            }

            var ingredient =
                await _ingredientRepository
                    .GetByIdAsync(
                        line.IngredientId,
                        cancellationToken);

            if (ingredient is null)
            {
                throw new InvalidOperationException(
                    $"Không tìm thấy nguyên liệu " +
                    $"{line.IngredientId}.");
            }

            var lineTotal =
                ingredient.UnitCost *
                line.Quantity;

            totalAmount +=
                lineTotal;

            persistedLines.Add(
                new
                {
                    ingredientId =
                        ingredient.IngredientId,

                    ingredientName =
                        ingredient.Name,

                    quantity =
                        line.Quantity,

                    unit =
                        ingredient.Unit,

                    unitCost =
                        ingredient.UnitCost,

                    lineTotal
                });
        }

        var payload =
            new
            {
                orderNumber,
                confirmedAt,
                lineCount =
                    persistedLines.Count,
                totalAmount,
                lines =
                    persistedLines
            };

        var json =
            JsonSerializer.Serialize(
                payload);

        /*
         * DeviceLogs.Content hiện có
         * max length = 4000.
         */
        if (json.Length > 4000)
        {
            throw new InvalidOperationException(
                "Đơn nhập quá lớn để lưu. " +
                "Vui lòng chia thành nhiều đơn.");
        }

        await _deviceLogRepository
            .AddAsync(
                new DeviceLog
                {
                    Protocol =
                        "PURCHASE_ORDER",

                    SourceAddress =
                        "FCanteen.Web",

                    Content =
                        json,

                    CreatedAt =
                        confirmedAt
                },
                cancellationToken);

        return new PurchaseOrderConfirmation
        {
            OrderNumber =
                orderNumber,

            LineCount =
                persistedLines.Count,

            TotalAmount =
                totalAmount
        };
    }
}
