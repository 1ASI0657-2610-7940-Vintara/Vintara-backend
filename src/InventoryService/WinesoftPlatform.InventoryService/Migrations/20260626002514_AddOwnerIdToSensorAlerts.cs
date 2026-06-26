using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WinesoftPlatform.InventoryService.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToSensorAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "owner_id",
                table: "sensor_alerts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "sensor_alerts");
        }
    }
}
