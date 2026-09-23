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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureMenuItem(modelBuilder);
        ConfigureOrderTicket(modelBuilder);
        ConfigureTicketLine(modelBuilder);
        ConfigureDeviceLog(modelBuilder);
        ConfigureIngredient(modelBuilder);
        ConfigureDailySettlement(modelBuilder);
        ConfigureStaff(modelBuilder);

        SeedMenuItems(modelBuilder);
    }

    private static void ConfigureMenuItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(x => x.MenuItemId);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Price)
                .HasPrecision(18, 2);

            entity.Property(x => x.Unit)
                .IsRequired()
                .HasMaxLength(50);
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

    private static void ConfigureIngredient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(x => x.IngredientId);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Unit)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.StockQuantity)
                .HasPrecision(18, 2);

            entity.Property(x => x.AlertThreshold)
                .HasPrecision(18, 2);
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

    private static void SeedMenuItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem
            {
                MenuItemId = 1,
                Name = "Cơm gà",
                Price = 35000,
                Unit = "Phần",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 2,
                Name = "Cơm sườn",
                Price = 40000,
                Unit = "Phần",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 3,
                Name = "Cơm bò xào",
                Price = 45000,
                Unit = "Phần",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 4,
                Name = "Mì xào bò",
                Price = 40000,
                Unit = "Phần",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 5,
                Name = "Mì xào trứng",
                Price = 30000,
                Unit = "Phần",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 6,
                Name = "Bún bò",
                Price = 40000,
                Unit = "Tô",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 7,
                Name = "Phở bò",
                Price = 45000,
                Unit = "Tô",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 8,
                Name = "Bánh mì thịt",
                Price = 25000,
                Unit = "Ổ",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 9,
                Name = "Bánh mì trứng",
                Price = 20000,
                Unit = "Ổ",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 10,
                Name = "Xôi gà",
                Price = 30000,
                Unit = "Hộp",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 11,
                Name = "Nước suối",
                Price = 10000,
                Unit = "Chai",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 12,
                Name = "Coca Cola",
                Price = 15000,
                Unit = "Lon",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 13,
                Name = "Pepsi",
                Price = 15000,
                Unit = "Lon",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 14,
                Name = "Trà đào",
                Price = 20000,
                Unit = "Ly",
                IsAvailable = true
            },
            new MenuItem
            {
                MenuItemId = 15,
                Name = "Cà phê sữa",
                Price = 20000,
                Unit = "Ly",
                IsAvailable = true
            }
        );
    }
}