using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.BonusEngine.MsSqlRepositories.Migrations
{
    public partial class AddActiveCampaign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveCampaigns",
                schema: "bonus_engine",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveCampaigns", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveCampaigns",
                schema: "bonus_engine");
        }
    }
}
