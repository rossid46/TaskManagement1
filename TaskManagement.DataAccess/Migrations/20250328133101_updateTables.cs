using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "TaskItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_ApplicationUserId",
                table: "TaskItems",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_AspNetUsers_ApplicationUserId",
                table: "TaskItems",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_AspNetUsers_ApplicationUserId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_ApplicationUserId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "TaskItems");
        }
    }
}
