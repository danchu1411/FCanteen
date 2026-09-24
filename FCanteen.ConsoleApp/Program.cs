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
using FCanteen.Services.Lifetimes;
using FCanteen.Services.Auditing;
using FCanteen.Services.Reporting;

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
    <OrderService>();

builder.Services.AddScoped
    <IOrderService>(
        serviceProvider =>
            serviceProvider
                .GetRequiredService
                    <OrderService>());

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
 * YC4 - Service lifetime demonstrations.
 */

builder.Services.AddTransient
    <ITransientLifetimeService,
     TransientLifetimeService>();

builder.Services.AddScoped
    <IScopedLifetimeService,
     ScopedLifetimeService>();

builder.Services.AddSingleton
    <ISingletonLifetimeService,
     SingletonLifetimeService>();

builder.Services.AddScoped
    <CaptiveDependencyDemoService>();

/*
 * YC5 - Optional audit logger.
 */
builder.Services.AddScoped
    <IAuditLogger,
     ConsoleAuditLogger>();

/*
 * YC5 - Report exporter.
 */
builder.Services.AddScoped
    <IReportExporter,
     ConsoleReportExporter>();

/*
 * Console UI.
 */
builder.Services.AddScoped
    <ConsoleApplication>();

using var host =
    builder.Build();

using var scope =
    host.Services.CreateScope();

/*
 * YC5 - PROPERTY INJECTION.
 *
 * OrderService do DI container tạo.
 * Ta KHÔNG dùng new OrderService().
 */
var concreteOrderService =
    scope.ServiceProvider
        .GetRequiredService
            <OrderService>();

/*
 * Logger optional nên dùng GetService().
 * Nếu không đăng ký IAuditLogger,
 * kết quả là null và OrderService
 * vẫn hoạt động.
 */
concreteOrderService.AuditLogger =
    scope.ServiceProvider
        .GetService
            <IAuditLogger>();

var app =
    scope.ServiceProvider
        .GetRequiredService
            <ConsoleApplication>();

await app.RunAsync();
