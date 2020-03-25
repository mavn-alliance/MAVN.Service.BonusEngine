using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.BonusEngine.MsSqlRepositories.Migrations
{
    public partial class UpdateCampaignCompletion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "campaign_id",
                schema: "bonus_engine",
                table: "condition_completion",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "campaign_id",
                schema: "bonus_engine",
                table: "condition_completion");
        }
    }
}
