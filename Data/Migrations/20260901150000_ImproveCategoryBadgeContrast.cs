using BibliotecaAspNet.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAspNet.Data.Migrations;

/// <summary>Corrige el color demo de Aventuras para usar texto blanco legible.</summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260901150000_ImproveCategoryBadgeContrast")]
public partial class ImproveCategoryBadgeContrast : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE Categories
            SET Color = '#047857'
            WHERE Name = 'Aventuras' AND Color = '#059669';
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE Categories
            SET Color = '#059669'
            WHERE Name = 'Aventuras' AND Color = '#047857';
            """);
    }
}
