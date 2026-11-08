using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonolithToMicro.Order.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAndQuantityToCustomerOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                schema: "orders",
                table: "CustomerOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                schema: "orders",
                table: "CustomerOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                schema: "orders",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "orders",
                table: "CustomerOrders");
        }
    }
}
