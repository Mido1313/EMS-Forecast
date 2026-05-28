using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FDemo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FDemo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MDemo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FDemoId = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MDemo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MDemo_FDemo_FDemoId",
                        column: x => x.FDemoId,
                        principalTable: "FDemo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DDemo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Length = table.Column<decimal>(type: "decimal(9,3)", nullable: true),
                    IsElectric = table.Column<bool>(type: "INTEGER", nullable: true),
                    MDemoId = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DDemo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DDemo_MDemo_MDemoId",
                        column: x => x.MDemoId,
                        principalTable: "MDemo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DDemo_MDemoId_Name",
                table: "DDemo",
                columns: new[] { "MDemoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FDemo_Name",
                table: "FDemo",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MDemo_FDemoId",
                table: "MDemo",
                column: "FDemoId");

            migrationBuilder.CreateIndex(
                name: "IX_MDemo_Name",
                table: "MDemo",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DDemo");

            migrationBuilder.DropTable(
                name: "FUser");

            migrationBuilder.DropTable(
                name: "MDemo");

            migrationBuilder.DropTable(
                name: "FDemo");
        }
    }
}
