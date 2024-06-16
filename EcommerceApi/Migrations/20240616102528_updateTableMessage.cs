using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceApi.Migrations
{
    /// <inheritdoc />
    public partial class updateTableMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_SeenderId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "SeenderId",
                table: "Messages",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_SeenderId",
                table: "Messages",
                newName: "IX_Messages_SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Messages",
                newName: "SeenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                newName: "IX_Messages_SeenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SeenderId",
                table: "Messages",
                column: "SeenderId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
