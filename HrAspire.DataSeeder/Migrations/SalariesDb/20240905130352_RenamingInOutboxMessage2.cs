using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrAspire.DataSeeder.Migrations.SalariesDb
{
    /// <inheritdoc />
    public partial class RenamingInOutboxMessage2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedResult",
                table: "OutboxMessages");

            migrationBuilder.RenameColumn(
                name: "ProcessedOn",
                table: "OutboxMessages",
                newName: "SentOn");

            migrationBuilder.RenameColumn(
                name: "IsProcessed",
                table: "OutboxMessages",
                newName: "IsSent");

            migrationBuilder.RenameColumn(
                name: "Data",
                table: "OutboxMessages",
                newName: "Payload");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessages_IsProcessed",
                table: "OutboxMessages",
                newName: "IX_OutboxMessages_IsSent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SentOn",
                table: "OutboxMessages",
                newName: "ProcessedOn");

            migrationBuilder.RenameColumn(
                name: "Payload",
                table: "OutboxMessages",
                newName: "Data");

            migrationBuilder.RenameColumn(
                name: "IsSent",
                table: "OutboxMessages",
                newName: "IsProcessed");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessages_IsSent",
                table: "OutboxMessages",
                newName: "IX_OutboxMessages_IsProcessed");

            migrationBuilder.AddColumn<string>(
                name: "ProcessedResult",
                table: "OutboxMessages",
                type: "text",
                nullable: true);
        }
    }
}
