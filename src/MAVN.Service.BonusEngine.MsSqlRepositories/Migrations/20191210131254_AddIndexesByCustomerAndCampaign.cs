using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.BonusEngine.MsSqlRepositories.Migrations
{
    public partial class AddIndexesByCustomerAndCampaign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_condition_completion_customer_id_campaign_id",
                schema: "bonus_engine",
                table: "condition_completion",
                columns: new[] { "customer_id", "campaign_id" });

            migrationBuilder.CreateIndex(
                name: "IX_condition_completion_customer_id_condition_id",
                schema: "bonus_engine",
                table: "condition_completion",
                columns: new[] { "customer_id", "condition_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_condition_completion_customer_id_campaign_id",
                schema: "bonus_engine",
                table: "condition_completion");

            migrationBuilder.DropIndex(
                name: "IX_condition_completion_customer_id_condition_id",
                schema: "bonus_engine",
                table: "condition_completion");
        }
    }
}
