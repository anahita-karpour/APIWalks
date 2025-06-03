using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NZWalks.API.Migrations.NZWalksDb
{
    /// <inheritdoc />
    public partial class RegionTableWithListWalksv02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "Id", "Code", "Name", "RegionImageUrl" },
                values: new object[] { new Guid("c7b2f25a-e6b0-4fc0-b5d0-08dd62b97fae"), "WKT", "Waikato", "https://images.unsplash.com/photo-1599636578877-1689486b6243?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: new Guid("c7b2f25a-e6b0-4fc0-b5d0-08dd62b97fae"));
        }
    }
}
