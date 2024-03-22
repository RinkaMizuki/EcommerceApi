using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceApi.Migrations
{
    /// <inheritdoc />
    public partial class updateTablePaymentNoti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentNotifications_Merchants_MerchantId",
                table: "PaymentNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentNotifications_Orders_PaymentOrderId",
                table: "PaymentNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentNotifications_Payments_PaymentId",
                table: "PaymentNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PaymentNotifications_MerchantId",
                table: "PaymentNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PaymentNotifications_PaymentOrderId",
                table: "PaymentNotifications");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "PaymentNotifications");

            migrationBuilder.DropColumn(
                name: "PaymentOrderId",
                table: "PaymentNotifications");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentNotifications_Payments_PaymentId",
                table: "PaymentNotifications",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentNotifications_Payments_PaymentId",
                table: "PaymentNotifications");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "PaymentNotifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentOrderId",
                table: "PaymentNotifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotifications_MerchantId",
                table: "PaymentNotifications",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotifications_PaymentOrderId",
                table: "PaymentNotifications",
                column: "PaymentOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentNotifications_Merchants_MerchantId",
                table: "PaymentNotifications",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "MerchantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentNotifications_Orders_PaymentOrderId",
                table: "PaymentNotifications",
                column: "PaymentOrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentNotifications_Payments_PaymentId",
                table: "PaymentNotifications",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId");
        }
    }
}
