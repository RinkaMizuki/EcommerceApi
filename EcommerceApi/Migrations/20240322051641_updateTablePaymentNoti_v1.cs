using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceApi.Migrations
{
    /// <inheritdoc />
    public partial class updateTablePaymentNoti_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentNotifications",
                table: "PaymentNotifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentNotifications",
                table: "PaymentNotifications",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotifications_PaymentId",
                table: "PaymentNotifications",
                column: "PaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentNotifications",
                table: "PaymentNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PaymentNotifications_PaymentId",
                table: "PaymentNotifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentNotifications",
                table: "PaymentNotifications",
                column: "PaymentId");
        }
    }
}
