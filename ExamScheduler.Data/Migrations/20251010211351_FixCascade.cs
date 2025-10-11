using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamScheduler.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OgrenciDersler_Ogrenciler_OgrenciID",
                table: "OgrenciDersler");

            migrationBuilder.AddForeignKey(
                name: "FK_OgrenciDersler_Ogrenciler_OgrenciID",
                table: "OgrenciDersler",
                column: "OgrenciID",
                principalTable: "Ogrenciler",
                principalColumn: "OgrenciID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OgrenciDersler_Ogrenciler_OgrenciID",
                table: "OgrenciDersler");

            migrationBuilder.AddForeignKey(
                name: "FK_OgrenciDersler_Ogrenciler_OgrenciID",
                table: "OgrenciDersler",
                column: "OgrenciID",
                principalTable: "Ogrenciler",
                principalColumn: "OgrenciID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
