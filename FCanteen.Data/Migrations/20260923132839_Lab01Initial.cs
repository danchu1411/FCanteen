using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FCanteen.Data.Migrations
{
    /// <inheritdoc />
    public partial class Lab01Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceLogs",
                columns: table => new
                {
                    DeviceLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Protocol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourceAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceLogs", x => x.DeviceLogId);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.MenuItemId);
                });

            migrationBuilder.CreateTable(
                name: "OrderTickets",
                columns: table => new
                {
                    OrderTicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CounterName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTickets", x => x.OrderTicketId);
                });

            migrationBuilder.CreateTable(
                name: "TicketLines",
                columns: table => new
                {
                    TicketLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderTicketId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLines", x => x.TicketLineId);
                    table.ForeignKey(
                        name: "FK_TicketLines_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketLines_OrderTickets_OrderTicketId",
                        column: x => x.OrderTicketId,
                        principalTable: "OrderTickets",
                        principalColumn: "OrderTicketId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "MenuItemId", "IsAvailable", "Name", "Price", "Unit" },
                values: new object[,]
                {
                    { 1, true, "Cơm gà", 35000m, "Phần" },
                    { 2, true, "Cơm sườn", 40000m, "Phần" },
                    { 3, true, "Cơm bò xào", 45000m, "Phần" },
                    { 4, true, "Mì xào bò", 40000m, "Phần" },
                    { 5, true, "Mì xào trứng", 30000m, "Phần" },
                    { 6, true, "Bún bò", 40000m, "Tô" },
                    { 7, true, "Phở bò", 45000m, "Tô" },
                    { 8, true, "Bánh mì thịt", 25000m, "Ổ" },
                    { 9, true, "Bánh mì trứng", 20000m, "Ổ" },
                    { 10, true, "Xôi gà", 30000m, "Hộp" },
                    { 11, true, "Nước suối", 10000m, "Chai" },
                    { 12, true, "Coca Cola", 15000m, "Lon" },
                    { 13, true, "Pepsi", 15000m, "Lon" },
                    { 14, true, "Trà đào", 20000m, "Ly" },
                    { 15, true, "Cà phê sữa", 20000m, "Ly" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketLines_MenuItemId",
                table: "TicketLines",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLines_OrderTicketId",
                table: "TicketLines",
                column: "OrderTicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceLogs");

            migrationBuilder.DropTable(
                name: "TicketLines");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "OrderTickets");
        }
    }
}
