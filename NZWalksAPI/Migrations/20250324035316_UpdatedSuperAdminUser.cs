using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NZWalks.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSuperAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "f7b3e81d-8637-4392-a6d5-40d78d9f7566", 0, "f7b3e81d-8637-4392-a6d5-40d78d9f7566", "superadmin@NZWalks.com", false, false, null, "SUPERADMIN@NZWALKS.COM", "SUPERADMIN@NZWALKS.COM", "AQAAAAIAAYagAAAAEP/zoNaLK6G2CUUP0Tro8KzbFkNSUYztuVASpVqoLrL+lCRSBaIEGvMgJ13HWUhP5g==", null, false, "89451b9f-c165-40b9-8b57-703e1acd38a8", false, "superadmin@NZWalks.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "3707e4aa-2e65-41b3-a6c3-a2e212832522", "f7b3e81d-8637-4392-a6d5-40d78d9f7566" },
                    { "bf63e81d-8637-4392-a6d5-40d78d9f7566", "f7b3e81d-8637-4392-a6d5-40d78d9f7566" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3707e4aa-2e65-41b3-a6c3-a2e212832522", "f7b3e81d-8637-4392-a6d5-40d78d9f7566" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "bf63e81d-8637-4392-a6d5-40d78d9f7566", "f7b3e81d-8637-4392-a6d5-40d78d9f7566" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7b3e81d-8637-4392-a6d5-40d78d9f7566");
        }
    }
}
