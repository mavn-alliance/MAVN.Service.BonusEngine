using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.BonusEngine.MsSqlRepositories.Migrations
{
    public partial class AddIndeciesOnCampaignCompletion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_campaign_completion_campaign_id_customer_id",
                schema: "bonus_engine",
                table: "campaign_completion",
                columns: new[] { "campaign_id", "customer_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_campaign_completion_campaign_id_customer_id",
                schema: "bonus_engine",
                table: "campaign_completion");
        }
    }
}
