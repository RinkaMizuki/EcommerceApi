using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceApi.Migrations
{
    /// <inheritdoc />
    public partial class updateTableParticipation_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participations_Conversations_UserId",
                table: "Participations");

            migrationBuilder.CreateIndex(
                name: "IX_Participations_ConversationId",
                table: "Participations",
                column: "ConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participations_Conversations_ConversationId",
                table: "Participations",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "ConversationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participations_Conversations_ConversationId",
                table: "Participations");

            migrationBuilder.DropIndex(
                name: "IX_Participations_ConversationId",
                table: "Participations");

            migrationBuilder.AddForeignKey(
                name: "FK_Participations_Conversations_UserId",
                table: "Participations",
                column: "UserId",
                principalTable: "Conversations",
                principalColumn: "ConversationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
