using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusinessLogic.Migrations
{
    public partial class OpenIddict : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpenIddictApplication<int>",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(maxLength: 100, nullable: false),
                    ClientSecret = table.Column<string>(nullable: true),
                    ConcurrencyToken = table.Column<string>(maxLength: 50, nullable: true),
                    ConsentType = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    Permissions = table.Column<string>(nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(nullable: true),
                    Properties = table.Column<string>(nullable: true),
                    RedirectUris = table.Column<string>(nullable: true),
                    Type = table.Column<string>(maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictApplication<int>", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictScope<int>",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConcurrencyToken = table.Column<string>(maxLength: 50, nullable: true),
                    Description = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Properties = table.Column<string>(nullable: true),
                    Resources = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictScope<int>", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorization<int>",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(nullable: true),
                    ConcurrencyToken = table.Column<string>(maxLength: 50, nullable: true),
                    Properties = table.Column<string>(nullable: true),
                    Scopes = table.Column<string>(nullable: true),
                    Status = table.Column<string>(maxLength: 25, nullable: false),
                    Subject = table.Column<string>(maxLength: 450, nullable: false),
                    Type = table.Column<string>(maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictAuthorization<int>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictAuthorization<int>_OpenIddictApplication<int>_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "OpenIddictApplication<int>",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictToken<int>",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(nullable: true),
                    AuthorizationId = table.Column<int>(nullable: true),
                    ConcurrencyToken = table.Column<string>(maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(nullable: true),
                    ExpirationDate = table.Column<DateTimeOffset>(nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    Properties = table.Column<string>(nullable: true),
                    ReferenceId = table.Column<string>(maxLength: 100, nullable: true),
                    Status = table.Column<string>(maxLength: 25, nullable: false),
                    Subject = table.Column<string>(maxLength: 450, nullable: false),
                    Type = table.Column<string>(maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictToken<int>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictToken<int>_OpenIddictApplication<int>_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "OpenIddictApplication<int>",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpenIddictToken<int>_OpenIddictAuthorization<int>_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalTable: "OpenIddictAuthorization<int>",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictApplication<int>_ClientId",
                table: "OpenIddictApplication<int>",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictAuthorization<int>_ApplicationId_Status_Subject_Type",
                table: "OpenIddictAuthorization<int>",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictScope<int>_Name",
                table: "OpenIddictScope<int>",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictToken<int>_AuthorizationId",
                table: "OpenIddictToken<int>",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictToken<int>_ReferenceId",
                table: "OpenIddictToken<int>",
                column: "ReferenceId",
                unique: true,
                filter: "[ReferenceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictToken<int>_ApplicationId_Status_Subject_Type",
                table: "OpenIddictToken<int>",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenIddictScope<int>");

            migrationBuilder.DropTable(
                name: "OpenIddictToken<int>");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorization<int>");

            migrationBuilder.DropTable(
                name: "OpenIddictApplication<int>");
        }
    }
}
