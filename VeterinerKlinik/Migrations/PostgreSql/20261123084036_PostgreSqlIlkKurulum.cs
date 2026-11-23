using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VeterinerKlinik.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreSqlIlkKurulum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Asilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TekrarAraligiGun = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ilaclar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Form = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Birim = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BirimFiyat = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ilaclar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sahipler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Eposta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Adres = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sahipler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Veterinerler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Uzmanlik = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veterinerler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hayvanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SahipId = table.Column<int>(type: "integer", nullable: false),
                    Ad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tur = table.Column<int>(type: "integer", nullable: false),
                    Irk = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Cinsiyet = table.Column<int>(type: "integer", nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Kilo = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Notlar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hayvanlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hayvanlar_Sahipler_SahipId",
                        column: x => x.SahipId,
                        principalTable: "Sahipler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KullaniciAdi = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AdSoyad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParolaOzeti = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    VeterinerId = table.Column<int>(type: "integer", nullable: true),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "AsiKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HayvanId = table.Column<int>(type: "integer", nullable: false),
                    AsiId = table.Column<int>(type: "integer", nullable: false),
                    VeterinerId = table.Column<int>(type: "integer", nullable: true),
                    UygulamaTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SonrakiTarih = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsiKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsiKayitlari_Asilar_AsiId",
                        column: x => x.AsiId,
                        principalTable: "Asilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsiKayitlari_Hayvanlar_HayvanId",
                        column: x => x.HayvanId,
                        principalTable: "Hayvanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsiKayitlari_Veterinerler_VeterinerId",
                        column: x => x.VeterinerId,
                        principalTable: "Veterinerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HayvanId = table.Column<int>(type: "integer", nullable: false),
                    VeterinerId = table.Column<int>(type: "integer", nullable: false),
                    Baslangic = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SureDakika = table.Column<int>(type: "integer", nullable: false),
                    Neden = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Durum = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Randevular_Hayvanlar_HayvanId",
                        column: x => x.HayvanId,
                        principalTable: "Hayvanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Randevular_Veterinerler_VeterinerId",
                        column: x => x.VeterinerId,
                        principalTable: "Veterinerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Muayeneler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HayvanId = table.Column<int>(type: "integer", nullable: false),
                    VeterinerId = table.Column<int>(type: "integer", nullable: false),
                    RandevuId = table.Column<int>(type: "integer", nullable: true),
                    Tarih = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Sikayet = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Teshis = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Tedavi = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notlar = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ucret = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Muayeneler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Muayeneler_Hayvanlar_HayvanId",
                        column: x => x.HayvanId,
                        principalTable: "Hayvanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Muayeneler_Randevular_RandevuId",
                        column: x => x.RandevuId,
                        principalTable: "Randevular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Muayeneler_Veterinerler_VeterinerId",
                        column: x => x.VeterinerId,
                        principalTable: "Veterinerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Odemeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SahipId = table.Column<int>(type: "integer", nullable: false),
                    MuayeneId = table.Column<int>(type: "integer", nullable: true),
                    Tutar = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Yontem = table.Column<int>(type: "integer", nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Odemeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Odemeler_Muayeneler_MuayeneId",
                        column: x => x.MuayeneId,
                        principalTable: "Muayeneler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Odemeler_Sahipler_SahipId",
                        column: x => x.SahipId,
                        principalTable: "Sahipler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceteKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MuayeneId = table.Column<int>(type: "integer", nullable: false),
                    IlacId = table.Column<int>(type: "integer", nullable: false),
                    Doz = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    KullanimSekli = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SureGun = table.Column<int>(type: "integer", nullable: false),
                    Adet = table.Column<int>(type: "integer", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceteKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceteKalemleri_Ilaclar_IlacId",
                        column: x => x.IlacId,
                        principalTable: "Ilaclar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceteKalemleri_Muayeneler_MuayeneId",
                        column: x => x.MuayeneId,
                        principalTable: "Muayeneler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Asilar",
                columns: new[] { "Id", "Ad", "TekrarAraligiGun" },
                values: new object[,]
                {
                    { 1, "Kuduz", 365 },
                    { 2, "Karma", 365 },
                    { 3, "İç parazit", 90 },
                    { 4, "Dış parazit", 30 }
                });

            migrationBuilder.InsertData(
                table: "Ilaclar",
                columns: new[] { "Id", "Ad", "Birim", "BirimFiyat", "Form" },
                values: new object[,]
                {
                    { 1, "Amoksisilin", "Tablet", 12.50m, "Tablet" },
                    { 2, "Meloksikam", "ml", 8.75m, "Şurup" },
                    { 3, "Ivermektin", "ml", 25.00m, "Enjeksiyon" },
                    { 4, "Prazikuantel", "Tablet", 15.00m, "Tablet" },
                    { 5, "Serum Fizyolojik", "Flakon", 30.00m, "Enjeksiyon" },
                    { 6, "Probiyotik", "ml", 18.00m, "Şurup" }
                });

            migrationBuilder.InsertData(
                table: "Veterinerler",
                columns: new[] { "Id", "Ad", "Aktif", "Soyad", "Telefon", "Uzmanlik" },
                values: new object[,]
                {
                    { 1, "Ayşe", true, "Yılmaz", "0532 000 00 01", "Genel Veterinerlik" },
                    { 2, "Mehmet", true, "Demir", "0532 000 00 02", "Cerrahi" },
                    { 3, "Elif", true, "Kaya", "0532 000 00 03", "Egzotik Hayvanlar" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsiKayitlari_AsiId",
                table: "AsiKayitlari",
                column: "AsiId");

            migrationBuilder.CreateIndex(
                name: "IX_AsiKayitlari_HayvanId",
                table: "AsiKayitlari",
                column: "HayvanId");

            migrationBuilder.CreateIndex(
                name: "IX_AsiKayitlari_VeterinerId",
                table: "AsiKayitlari",
                column: "VeterinerId");

            migrationBuilder.CreateIndex(
                name: "IX_Hayvanlar_SahipId",
                table: "Hayvanlar",
                column: "SahipId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_VeterinerId",
                table: "Kullanicilar",
                column: "VeterinerId");

            migrationBuilder.CreateIndex(
                name: "IX_Muayeneler_HayvanId",
                table: "Muayeneler",
                column: "HayvanId");

            migrationBuilder.CreateIndex(
                name: "IX_Muayeneler_RandevuId",
                table: "Muayeneler",
                column: "RandevuId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Muayeneler_VeterinerId",
                table: "Muayeneler",
                column: "VeterinerId");

            migrationBuilder.CreateIndex(
                name: "IX_Odemeler_MuayeneId",
                table: "Odemeler",
                column: "MuayeneId");

            migrationBuilder.CreateIndex(
                name: "IX_Odemeler_SahipId",
                table: "Odemeler",
                column: "SahipId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_HayvanId",
                table: "Randevular",
                column: "HayvanId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_VeterinerId",
                table: "Randevular",
                column: "VeterinerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceteKalemleri_IlacId",
                table: "ReceteKalemleri",
                column: "IlacId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceteKalemleri_MuayeneId_IlacId",
                table: "ReceteKalemleri",
                columns: new[] { "MuayeneId", "IlacId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsiKayitlari");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Odemeler");

            migrationBuilder.DropTable(
                name: "ReceteKalemleri");

            migrationBuilder.DropTable(
                name: "Asilar");

            migrationBuilder.DropTable(
                name: "Ilaclar");

            migrationBuilder.DropTable(
                name: "Muayeneler");

            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropTable(
                name: "Hayvanlar");

            migrationBuilder.DropTable(
                name: "Veterinerler");

            migrationBuilder.DropTable(
                name: "Sahipler");
        }
    }
}
