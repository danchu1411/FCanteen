using FCanteen.ConsoleApp;
using FCanteen.Data;
using FCanteen.Repositories.Implementations;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Implementations;
using FCanteen.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FCanteen.Services.Discounts;
using FCanteen.Services.Discounts.Policies;
using FCanteen.Services.Notifications;
using FCanteen.Services.Notifications.Implementations;

var builder =
    Host.CreateApplicationBuilder(args);

var connectionString =
    builder.Configuration
        .GetConnectionString(
            "FCanteen")
    ?? throw new InvalidOperationException(
        "Connection string 'FCanteen' " +
        "was not found.");

/*
 * DbContext mặc định được đăng ký Scoped.
 */
builder.Services.AddDbContext
    <FCanteenContext>(
        options =>
            options.UseSqlServer(
                connectionString));

/*
 * Repository registrations.
 */
builder.Services.AddScoped
    <IMenuItemRepository,
     MenuItemRepository>();

builder.Services.AddScoped
    <IOrderRepository,
     OrderRepository>();

builder.Services.AddScoped
    <IIngredientRepository,
     IngredientRepository>();

/*
 * Service registrations.
 */
builder.Services.AddScoped
    <IOrderService,
     OrderService>();

builder.Services.AddScoped
    <IReportService,
     ReportService>();

builder.Services.AddScoped
    <IInventoryService,
     InventoryService>();

builder.Services.AddScoped
    <IDiscountPolicyLogRepository,
     DiscountPolicyLogRepository>();

builder.Services.AddScoped
    <INotificationService,
     ConsoleNotificationService>();

/*
 * Discount Policies.
 *
 * Có thể đăng ký nhiều implementation
 * cho cùng một interface.
 */
builder.Services.AddScoped
    <IDiscountPolicy,
     StudentDiscountPolicy>();

builder.Services.AddScoped
    <IDiscountPolicy,
     StaffDiscountPolicy>();

builder.Services.AddScoped
    <IDiscountPolicy,
     ComboDiscountPolicy>();

/*
 * Console UI.
 */
builder.Services.AddScoped
    <ConsoleApplication>();

using var host =
    builder.Build();

/*
 * Tạo scope vì các repository/service
 * và DbContext đang Scoped.
 */
using var scope =
    host.Services.CreateScope();

var app =
    scope.ServiceProvider
        .GetRequiredService
            <ConsoleApplication>();

await app.RunAsync();
