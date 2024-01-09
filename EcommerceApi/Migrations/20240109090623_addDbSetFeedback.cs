using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceApi.Migrations
{
    /// <inheritdoc />
    public partial class addDbSetFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackRate_Rates_FeedbackRateId",
                table: "FeedbackRate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FeedbackRate",
                table: "FeedbackRate");

            migrationBuilder.RenameTable(
                name: "FeedbackRate",
                newName: "Feedbacks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Feedbacks",
                table: "Feedbacks",
                column: "FeedbackRateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Rates_FeedbackRateId",
                table: "Feedbacks",
                column: "FeedbackRateId",
                principalTable: "Rates",
                principalColumn: "RateId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Rates_FeedbackRateId",
                table: "Feedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Feedbacks",
                table: "Feedbacks");

            migrationBuilder.RenameTable(
                name: "Feedbacks",
                newName: "FeedbackRate");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FeedbackRate",
                table: "FeedbackRate",
                column: "FeedbackRateId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackRate_Rates_FeedbackRateId",
                table: "FeedbackRate",
                column: "FeedbackRateId",
                principalTable: "Rates",
                principalColumn: "RateId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
