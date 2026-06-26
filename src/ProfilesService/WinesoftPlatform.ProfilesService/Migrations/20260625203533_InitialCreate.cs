using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace WinesoftPlatform.ProfilesService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    business_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    branch = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    street = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    city = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    postal_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    country = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    legal_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_profiles", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profiles");
        }
    }
}
