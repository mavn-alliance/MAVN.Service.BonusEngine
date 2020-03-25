using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.BonusEngine.MsSqlRepositories.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bonus_engine");

            migrationBuilder.CreateTable(
                name: "campaign_completion",
                schema: "bonus_engine",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    customer_id = table.Column<Guid>(nullable: false),
                    campaign_completion_count = table.Column<int>(nullable: false),
                    campaign_id = table.Column<Guid>(nullable: false),
                    is_completed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campaign_completion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "condition_completion",
                schema: "bonus_engine",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    customer_id = table.Column<Guid>(nullable: false),
                    current_count = table.Column<int>(nullable: false),
                    is_completed = table.Column<bool>(nullable: false),
                    condition_id = table.Column<Guid>(nullable: false),
                    data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_condition_completion", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign_completion",
                schema: "bonus_engine");

            migrationBuilder.DropTable(
                name: "condition_completion",
                schema: "bonus_engine");
        }
    }
}
