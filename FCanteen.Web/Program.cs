using FCanteen.Data;

using FCanteen.Repositories.Implementations;
using FCanteen.Repositories.Interfaces;

using FCanteen.Services.Auditing;
using FCanteen.Services.Discounts;
using FCanteen.Services.Discounts.Policies;
using FCanteen.Services.Implementations;
using FCanteen.Services.Interfaces;
using FCanteen.Services.Notifications;
using FCanteen.Services.Notifications.Implementations;
using FCanteen.Services.Reporting;
using FCanteen.Data.Seeders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder =
    WebApplication.CreateBuilder(args);

/*
 * MVC
 */
builder.Services.AddControllersWithViews();

/*
 * YC4 - Session cart.
 */
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(
    options =>
    {
        options.IdleTimeout =
            TimeSpan.FromMinutes(30);

        options.Cookie.HttpOnly =
            true;

        options.Cookie.IsEssential =
            true;
    });

/*
 * Database
 */
var connectionString =
    builder.Configuration
        .GetConnectionString(
            "FCanteen")
    ?? throw new InvalidOperationException(
        "Connection string 'FCanteen' " +
        "was not found.");

builder.Services.AddDbContext<FCanteenContext>(
    options =>
        options.UseSqlServer(
            connectionString));

/*
 * Repositories reused from Lab03.
 */
builder.Services.AddScoped<
    IMenuItemRepository,
    MenuItemRepository>();

builder.Services.AddScoped<
    ICategoryRepository,
    CategoryRepository>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddScoped<
    IIngredientRepository,
    IngredientRepository>();

builder.Services.AddScoped<
    IDiscountPolicyLogRepository,
    DiscountPolicyLogRepository>();

builder.Services.AddScoped<
    IDeviceLogRepository,
    DeviceLogRepository>();

/*
 * Services reused from Lab03.
 */
builder.Services.AddScoped<
    IReportService,
    ReportService>();

builder.Services.AddScoped<
    IInventoryService,
    InventoryService>();

builder.Services.AddScoped<
    IPurchaseOrderService,
    PurchaseOrderService>();

/*
 * Lab03 discount policies.
 */
builder.Services.AddScoped<
    IDiscountPolicy,
    StudentDiscountPolicy>();

builder.Services.AddScoped<
    IDiscountPolicy,
    StaffDiscountPolicy>();

builder.Services.AddScoped<
    IDiscountPolicy,
    ComboDiscountPolicy>();

/*
 * Notification.
 */
builder.Services.AddScoped<
    INotificationService,
    ConsoleNotificationService>();

/*
 * YC5 Lab03 dependencies.
 */
builder.Services.AddScoped<
    IAuditLogger,
    ConsoleAuditLogger>();

builder.Services.AddScoped<
    IReportExporter,
    ConsoleReportExporter>();

/*
 * OrderService vẫn giữ Property Injection
 * giống bản Lab03 đã hoàn thiện.
 */
builder.Services.AddScoped<OrderService>(
    serviceProvider =>
    {
        var service =
            ActivatorUtilities
                .CreateInstance<OrderService>(
                    serviceProvider);

        service.AuditLogger =
            serviceProvider
                .GetService<IAuditLogger>();

        return service;
    });

builder.Services.AddScoped<IOrderService>(
    serviceProvider =>
        serviceProvider
            .GetRequiredService<OrderService>());

/*
 * Build Web Application.
 *
 * Sau dòng này không đăng ký service nữa.
 */
var app =
    builder.Build();

using (var scope =
    app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<
                FCanteenContext>();

    await Lab04DemoDataSeeder
        .SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");

app.Run();