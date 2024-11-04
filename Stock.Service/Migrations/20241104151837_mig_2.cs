using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock.Service.Migrations
{
    /// <inheritdoc />
    public partial class mig_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OrderInboxes",
                newName: "IdempotentToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdempotentToken",
                table: "OrderInboxes",
                newName: "Id");
        }
    }
}
