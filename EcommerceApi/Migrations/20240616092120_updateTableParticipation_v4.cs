using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceApi.Migrations
{
    /// <inheritdoc />
    public partial class updateTableParticipation_v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AdminId",
                table: "Participations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Participations");
        }
    }
}
