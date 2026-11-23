using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeterinerKlinik.Migrations
{
    /// <inheritdoc />
    public partial class KullanicilarVeRoller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KullaniciAdi = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    AdSoyad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ParolaOzeti = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Rol = table.Column<int>(type: "INTEGER", nullable: false),
                    VeterinerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kullanicilar_Veterinerler_VeterinerId",
                        column: x => x.VeterinerId,
                        principalTable: "Veterinerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_VeterinerId",
                table: "Kullanicilar",
                column: "VeterinerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kullanicilar");
        }
    }
}
