using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Data;

public class FCanteenContext(
    DbContextOptions<FCanteenContext> options) : DbContext(options)
{
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    public DbSet<OrderTicket> OrderTickets => Set<OrderTicket>();

    public DbSet<TicketLine> TicketLines => Set<TicketLine>();

    public DbSet<DeviceLog> DeviceLogs => Set<DeviceLog>();

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    public DbSet<DailySettlement> DailySettlements => Set<DailySettlement>();

    public DbSet<Staff> Staffs => Set<Staff>();

    public DbSet<DiscountPolicyLog>
    DiscountPolicyLogs =>
        Set<DiscountPolicyLog>();

    public DbSet<Category> Categories =>
        Set<Category>();

    public DbSet<Supplier> Suppliers =>
        Set<Supplier>();

    public DbSet<MenuItemIngredient>
        MenuItemIngredients =>
            Set<MenuItemIngredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCategory(modelBuilder);
        ConfigureSupplier(modelBuilder);
        ConfigureMenuItem(modelBuilder);
        ConfigureIngredient(modelBuilder);
        ConfigureMenuItemIngredient(modelBuilder);

        ConfigureOrderTicket(modelBuilder);
        ConfigureTicketLine(modelBuilder);
        ConfigureDeviceLog(modelBuilder);
        ConfigureDailySettlement(modelBuilder);
        ConfigureStaff(modelBuilder);
        ConfigureDiscountPolicyLog(modelBuilder);

        SeedCategories(modelBuilder);
        SeedSuppliers(modelBuilder);
        SeedMenuItems(modelBuilder);
    }

    private static void ConfigureMenuItem(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>(
            entity =>
            {
                entity.HasKey(
                    x => x.MenuItemId);

                entity.Property(
                        x => x.Code)
                    .IsRequired()
                    .HasMaxLength(20);

                /*
                 * YC2 yêu cầu tên tối đa 100 ký tự.
                 */
                entity.Property(
                        x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(
                        x => x.Price)
                    .HasPrecision(18, 2);

                entity.Property(
                        x => x.Unit)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(
                    x => x.Code);

                entity.HasOne(
                        x => x.Category)
                    .WithMany(
                        x => x.MenuItems)
                    .HasForeignKey(
                        x => x.CategoryId)
                    .OnDelete(
                        DeleteBehavior.SetNull);
            });
    }

    private static void ConfigureOrderTicket(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderTicket>(entity =>
        {
            entity.HasKey(x => x.OrderTicketId);

            entity.Property(x => x.CounterName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.BranchCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("BR01");

            entity.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);
        });
    }

    private static void ConfigureDiscountPolicyLog(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscountPolicyLog>(
            entity =>
            {
                entity.HasKey(
                    x => x.DiscountPolicyLogId);

                entity.Property(
                        x => x.PolicyName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(
                        x => x.CustomerType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(
                        x => x.AmountBefore)
                    .HasPrecision(18, 2);

                entity.Property(
                        x => x.DiscountAmount)
                    .HasPrecision(18, 2);

                entity.Property(
                        x => x.AmountAfter)
                    .HasPrecision(18, 2);
            });
    }

    private static void ConfigureIngredient(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredient>(
            entity =>
            {
                entity.HasKey(
                    x => x.IngredientId);

                entity.Property(
                        x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(
                        x => x.Unit)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(
                        x => x.StockQuantity)
                    .HasPrecision(18, 2);

                entity.Property(
                        x => x.AlertThreshold)
                    .HasPrecision(18, 2);

                entity.Property(
                        x => x.UnitCost)
                    .HasPrecision(18, 2);

                entity.HasOne(
                        x => x.Supplier)
                    .WithMany(
                        x => x.Ingredients)
                    .HasForeignKey(
                        x => x.SupplierId)
                    .OnDelete(
                        DeleteBehavior.SetNull);
            });
    }

    private static void ConfigureDailySettlement(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailySettlement>(entity =>
        {
            entity.HasKey(x => x.DailySettlementId);

            entity.Property(x => x.Date)
                .HasColumnType("date");

            entity.Property(x => x.BranchCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.TotalRevenue)
                .HasPrecision(18, 2);
        });
    }

    private static void ConfigureTicketLine(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketLine>(entity =>
        {
            entity.HasKey(x => x.TicketLineId);

            entity.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(x => x.Note)
                .HasMaxLength(500);

            entity.HasOne(x => x.OrderTicket)
                .WithMany(x => x.TicketLines)
                .HasForeignKey(x => x.OrderTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.TicketLines)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDeviceLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceLog>(entity =>
        {
            entity.HasKey(x => x.DeviceLogId);

            entity.Property(x => x.Protocol)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.SourceAddress)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(4000);
        });
    }

    private static void ConfigureStaff(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Staff>(
            entity =>
            {
                entity.HasKey(
                    x => x.StaffId);

                entity.Property(
                        x => x.StaffCode)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasIndex(
                        x => x.StaffCode)
                    .IsUnique();

                entity.Property(
                        x => x.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(
                        x => x.Role)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(
                        x => x.BranchCode)
                    .IsRequired()
                    .HasMaxLength(20);
            });
    }

    private static void SeedMenuItems(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>()
            .HasData(
                new MenuItem
                {
                    MenuItemId = 1,
                    Code = "MON-0001",
                    Name = "Cơm gà",
                    Price = 35000,
                    Unit = "Phần",
                    IsAvailable = true,
                    CategoryId = 1
                },
                new MenuItem
                {
                    MenuItemId = 2,
                    Code = "MON-0002",
                    Name = "Cơm sườn",
                    Price = 40000,
                    Unit = "Phần",
                    IsAvailable = true,
                    CategoryId = 1
                },
                new MenuItem
                {
                    MenuItemId = 3,
                    Code = "MON-0003",
                    Name = "Cơm bò xào",
                    Price = 45000,
                    Unit = "Phần",
                    IsAvailable = true,
                    CategoryId = 1
                },
                new MenuItem
                {
                    MenuItemId = 4,
                    Code = "MON-0004",
                    Name = "Mì xào bò",
                    Price = 40000,
                    Unit = "Phần",
                    IsAvailable = true,
                    CategoryId = 1
                },
                new MenuItem
                {
                    MenuItemId = 5,
                    Code = "MON-0005",
                    Name = "Mì xào trứng",
                    Price = 30000,
                    Unit = "Phần",
                    IsAvailable = true,
                    CategoryId = 1
                },
                new MenuItem
                {
                    MenuItemId = 6,
                    Code = "MON-0006",
                    Name = "Bún bò",
                    Price = 40000,
                    Unit = "Tô",
                    IsAvailable = true,
                    CategoryId = 1
                },
                new MenuItem
                {
                    MenuItemId = 7,
                    Code = "MON-0007",
                    Name = "Phở bò",
                    Price = 45000,
                    Unit = "Tô",
                    IsAvailable = true,
                    CategoryId = 1
                },
                new MenuItem
                {
                    MenuItemId = 8,
                    Code = "MON-0008",
                    Name = "Bánh mì thịt",
                    Price = 25000,
                    Unit = "Ổ",
                    IsAvailable = true,
                    CategoryId = 2
                },
                new MenuItem
                {
                    MenuItemId = 9,
                    Code = "MON-0009",
                    Name = "Bánh mì trứng",
                    Price = 20000,
                    Unit = "Ổ",
                    IsAvailable = true,
                    CategoryId = 2
                },
                new MenuItem
                {
                    MenuItemId = 10,
                    Code = "MON-0010",
                    Name = "Xôi gà",
                    Price = 30000,
                    Unit = "Hộp",
                    IsAvailable = true,
                    CategoryId = 2
                },
                new MenuItem
                {
                    MenuItemId = 11,
                    Code = "MON-0011",
                    Name = "Nước suối",
                    Price = 10000,
                    Unit = "Chai",
                    IsAvailable = true,
                    CategoryId = 3
                },
                new MenuItem
                {
                    MenuItemId = 12,
                    Code = "MON-0012",
                    Name = "Coca Cola",
                    Price = 15000,
                    Unit = "Lon",
                    IsAvailable = true,
                    CategoryId = 3
                },
                new MenuItem
                {
                    MenuItemId = 13,
                    Code = "MON-0013",
                    Name = "Pepsi",
                    Price = 15000,
                    Unit = "Lon",
                    IsAvailable = true,
                    CategoryId = 3
                },
                new MenuItem
                {
                    MenuItemId = 14,
                    Code = "MON-0014",
                    Name = "Trà đào",
                    Price = 20000,
                    Unit = "Ly",
                    IsAvailable = true,
                    CategoryId = 3
                },
                new MenuItem
                {
                    MenuItemId = 15,
                    Code = "MON-0015",
                    Name = "Cà phê sữa",
                    Price = 20000,
                    Unit = "Ly",
                    IsAvailable = true,
                    CategoryId = 3
                }
            );
    }

    private static void ConfigureCategory(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(
            entity =>
            {
                entity.HasKey(
                    x => x.CategoryId);

                entity.Property(
                        x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(
                        x => x.Name)
                    .IsUnique();
            });
    }

    private static void ConfigureSupplier(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>(
            entity =>
            {
                entity.HasKey(
                    x => x.SupplierId);

                entity.Property(
                        x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(
                        x => x.Phone)
                    .HasMaxLength(30);

                entity.Property(
                        x => x.Email)
                    .HasMaxLength(150);

                entity.Property(
                        x => x.Address)
                    .HasMaxLength(250);
            });
    }

    private static void ConfigureMenuItemIngredient(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItemIngredient>(
            entity =>
            {
                /*
                 * Composite Primary Key.
                 *
                 * Một Ingredient chỉ xuất hiện
                 * một lần trong cùng một MenuItem.
                 */
                entity.HasKey(
                    x => new
                    {
                        x.MenuItemId,
                        x.IngredientId
                    });

                entity.Property(
                        x => x.Quantity)
                    .HasPrecision(18, 4);

                entity.HasOne(
                        x => x.MenuItem)
                    .WithMany(
                        x => x.MenuItemIngredients)
                    .HasForeignKey(
                        x => x.MenuItemId)
                    .OnDelete(
                        DeleteBehavior.Cascade);

                entity.HasOne(
                        x => x.Ingredient)
                    .WithMany(
                        x => x.MenuItemIngredients)
                    .HasForeignKey(
                        x => x.IngredientId)
                    .OnDelete(
                        DeleteBehavior.Cascade);
            });
    }

    private static void SeedCategories(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasData(
                new Category
                {
                    CategoryId = 1,
                    Name = "Món chính"
                },
                new Category
                {
                    CategoryId = 2,
                    Name = "Món phụ"
                },
                new Category
                {
                    CategoryId = 3,
                    Name = "Đồ uống"
                },
                new Category
                {
                    CategoryId = 4,
                    Name = "Tráng miệng"
                });
    }

    private static void SeedSuppliers(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>()
            .HasData(
                new Supplier
                {
                    SupplierId = 1,
                    Name =
                        "Nhà cung cấp Thực phẩm Đà Nẵng",
                    Phone =
                        "0900000001",
                    Email =
                        "food@fcanteen.local",
                    Address =
                        "Đà Nẵng"
                },
                new Supplier
                {
                    SupplierId = 2,
                    Name =
                        "Nhà cung cấp Rau sạch",
                    Phone =
                        "0900000002",
                    Email =
                        "vegetable@fcanteen.local",
                    Address =
                        "Đà Nẵng"
                },
                new Supplier
                {
                    SupplierId = 3,
                    Name =
                        "Nhà cung cấp Đồ uống",
                    Phone =
                        "0900000003",
                    Email =
                        "drink@fcanteen.local",
                    Address =
                        "Đà Nẵng"
                });
    }
}