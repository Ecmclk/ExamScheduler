using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamScheduler.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bolumler",
                columns: table => new
                {
                    BolumID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BolumKod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BolumAd = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bolumler", x => x.BolumID);
                });

            migrationBuilder.CreateTable(
                name: "Roller",
                columns: table => new
                {
                    RolID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RolAd = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roller", x => x.RolID);
                });

            migrationBuilder.CreateTable(
                name: "Dersler",
                columns: table => new
                {
                    DersID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BolumID = table.Column<int>(type: "int", nullable: false),
                    DersKodu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DersAdi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OgretimGorevlisi = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Sinif = table.Column<byte>(type: "tinyint", nullable: false),
                    DersTuru = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dersler", x => x.DersID);
                    table.ForeignKey(
                        name: "FK_Dersler_Bolumler_BolumID",
                        column: x => x.BolumID,
                        principalTable: "Bolumler",
                        principalColumn: "BolumID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Derslikler",
                columns: table => new
                {
                    DerslikID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BolumID = table.Column<int>(type: "int", nullable: false),
                    DerslikKod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DerslikAd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kapasite = table.Column<int>(type: "int", nullable: false),
                    EnineSira = table.Column<int>(type: "int", nullable: false),
                    BoyunaSira = table.Column<int>(type: "int", nullable: false),
                    SiraYapisi = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Derslikler", x => x.DerslikID);
                    table.ForeignKey(
                        name: "FK_Derslikler_Bolumler_BolumID",
                        column: x => x.BolumID,
                        principalTable: "Bolumler",
                        principalColumn: "BolumID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ogrenciler",
                columns: table => new
                {
                    OgrenciID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    BolumID = table.Column<int>(type: "int", nullable: false),
                    Sinif = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ogrenciler", x => x.OgrenciID);
                    table.ForeignKey(
                        name: "FK_Ogrenciler_Bolumler_BolumID",
                        column: x => x.BolumID,
                        principalTable: "Bolumler",
                        principalColumn: "BolumID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    KullaniciID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Eposta = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SifreHash = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: false),
                    SifreSalt = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    RolID = table.Column<int>(type: "int", nullable: false),
                    BolumID = table.Column<int>(type: "int", nullable: true),
                    Aktiflik = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.KullaniciID);
                    table.ForeignKey(
                        name: "FK_Kullanicilar_Bolumler_BolumID",
                        column: x => x.BolumID,
                        principalTable: "Bolumler",
                        principalColumn: "BolumID");
                    table.ForeignKey(
                        name: "FK_Kullanicilar_Roller_RolID",
                        column: x => x.RolID,
                        principalTable: "Roller",
                        principalColumn: "RolID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciDersler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciID = table.Column<int>(type: "int", nullable: false),
                    DersID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciDersler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciDersler_Dersler_DersID",
                        column: x => x.DersID,
                        principalTable: "Dersler",
                        principalColumn: "DersID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OgrenciDersler_Ogrenciler_OgrenciID",
                        column: x => x.OgrenciID,
                        principalTable: "Ogrenciler",
                        principalColumn: "OgrenciID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Bolumler__4BC8DE108A0D08B3",
                table: "Bolumler",
                column: "BolumKod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dersler_BolumID",
                table: "Dersler",
                column: "BolumID");

            migrationBuilder.CreateIndex(
                name: "UQ__Dersler__9DCB30EF802CD853",
                table: "Dersler",
                column: "DersKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Derslikler_BolumID",
                table: "Derslikler",
                column: "BolumID");

            migrationBuilder.CreateIndex(
                name: "UQ__Derslikl__041E03CAADCAED8B",
                table: "Derslikler",
                column: "DerslikKod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Derslikl__8E73FCE1D6CDC992",
                table: "Derslikler",
                column: "DerslikAd",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_BolumID",
                table: "Kullanicilar",
                column: "BolumID");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_RolID",
                table: "Kullanicilar",
                column: "RolID");

            migrationBuilder.CreateIndex(
                name: "UQ__Kullanic__03ABA3916F7CD1B2",
                table: "Kullanicilar",
                column: "Eposta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciDersler_DersID",
                table: "OgrenciDersler",
                column: "DersID");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciDersler_OgrenciID",
                table: "OgrenciDersler",
                column: "OgrenciID");

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_BolumID",
                table: "Ogrenciler",
                column: "BolumID");

            migrationBuilder.CreateIndex(
                name: "UQ__Ogrencil__E497FE1A0D69C5B3",
                table: "Ogrenciler",
                column: "OgrenciNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Roller__F92343FA036F3BB5",
                table: "Roller",
                column: "RolAd",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Derslikler");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "OgrenciDersler");

            migrationBuilder.DropTable(
                name: "Roller");

            migrationBuilder.DropTable(
                name: "Dersler");

            migrationBuilder.DropTable(
                name: "Ogrenciler");

            migrationBuilder.DropTable(
                name: "Bolumler");
        }
    }
}
