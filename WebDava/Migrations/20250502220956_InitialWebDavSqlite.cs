using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDava.Migrations
{
    /// <inheritdoc />
    public partial class InitialWebDavSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    IsDirectory = table.Column<bool>(type: "INTEGER", nullable: false),
                    Length = table.Column<long>(type: "INTEGER", nullable: false),
                    LastWriteTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ETag = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Extension = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Path",
                table: "Resources",
                column: "Path",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Resources");
        }
    }
}
