using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.BonusEngine.MsSqlRepositories.Migrations
{
    public partial class RenameTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ActiveCampaigns",
                schema: "bonus_engine",
                table: "ActiveCampaigns");

            migrationBuilder.RenameTable(
                name: "ActiveCampaigns",
                schema: "bonus_engine",
                newName: "active_campaigns",
                newSchema: "bonus_engine");

            migrationBuilder.AddPrimaryKey(
                name: "PK_active_campaigns",
                schema: "bonus_engine",
                table: "active_campaigns",
                column: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_active_campaigns",
                schema: "bonus_engine",
                table: "active_campaigns");

            migrationBuilder.RenameTable(
                name: "active_campaigns",
                schema: "bonus_engine",
                newName: "ActiveCampaigns",
                newSchema: "bonus_engine");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActiveCampaigns",
                schema: "bonus_engine",
                table: "ActiveCampaigns",
                column: "id");
        }
    }
}
