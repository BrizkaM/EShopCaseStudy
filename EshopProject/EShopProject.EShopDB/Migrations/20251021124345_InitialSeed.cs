using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EShopProject.EShopDB.Migrations
{
    /// <inheritdoc />
    public partial class InitialSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "Idate", "ImageUrl", "Name", "Price", "Quantity", "Udate" },
                values: new object[,]
                {
                    { 1, "High-performance gaming laptop with RTX 4070, 32GB RAM, and 1TB SSD", new DateTime(2025, 9, 21, 14, 0, 0, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1603302576837-37561b2e2302", "Gaming Laptop Pro X15", 34999.99m, 15, new DateTime(2025, 9, 21, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Premium noise-cancelling headphones with 30-hour battery life", new DateTime(2025, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1505740420928-5e560c06d30e", "Wireless Bluetooth Headphones", 2499.00m, 45, new DateTime(2025, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Professional 4K monitor with HDR support and 144Hz refresh rate", new DateTime(2025, 10, 1, 14, 0, 0, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf", "4K Ultra HD Monitor 32\"", 12999.00m, 8, new DateTime(2025, 10, 1, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "RGB mechanical keyboard with Cherry MX switches", new DateTime(2025, 10, 6, 14, 0, 0, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1587829741301-dc798b83add3", "Mechanical Gaming Keyboard RGB", 3299.00m, 32, new DateTime(2025, 10, 6, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "Latest flagship smartphone with advanced camera system", new DateTime(2025, 10, 11, 14, 0, 0, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9", "Smartphone Pro Max 256GB", 28999.00m, 22, new DateTime(2025, 10, 11, 14, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
