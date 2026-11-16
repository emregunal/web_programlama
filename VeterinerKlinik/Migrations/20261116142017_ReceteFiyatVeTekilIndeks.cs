using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeterinerKlinik.Migrations
{
    /// <inheritdoc />
    public partial class ReceteFiyatVeTekilIndeks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReceteKalemleri_MuayeneId",
                table: "ReceteKalemleri");

            migrationBuilder.DropIndex(
                name: "IX_Muayeneler_RandevuId",
                table: "Muayeneler");

            migrationBuilder.AddColumn<decimal>(
                name: "BirimFiyat",
                table: "ReceteKalemleri",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ReceteKalemleri_MuayeneId_IlacId",
                table: "ReceteKalemleri",
                columns: new[] { "MuayeneId", "IlacId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Muayeneler_RandevuId",
                table: "Muayeneler",
                column: "RandevuId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReceteKalemleri_MuayeneId_IlacId",
                table: "ReceteKalemleri");

            migrationBuilder.DropIndex(
                name: "IX_Muayeneler_RandevuId",
                table: "Muayeneler");

            migrationBuilder.DropColumn(
                name: "BirimFiyat",
                table: "ReceteKalemleri");

            migrationBuilder.CreateIndex(
                name: "IX_ReceteKalemleri_MuayeneId",
                table: "ReceteKalemleri",
                column: "MuayeneId");

            migrationBuilder.CreateIndex(
                name: "IX_Muayeneler_RandevuId",
                table: "Muayeneler",
                column: "RandevuId");
        }
    }
}
