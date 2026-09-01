using BibliotecaAspNet.Data;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace BibliotecaAspNet.Data.Migrations;

/// <summary>
/// Completa los datos demo de una base existente una sola vez.
/// Al estar en una migración, quitar una imagen desde administración no se deshace
/// en el siguiente arranque de la aplicación.
/// </summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260901143000_BackfillDemoImages")]
public partial class BackfillDemoImages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE Authors
            SET PhotoFileName = 'demo-author-garcia-marquez.jpg'
            WHERE Name = 'Gabriel García Márquez' AND PhotoFileName IS NULL;

            UPDATE Authors
            SET PhotoFileName = 'demo-author-jane-austen.jpg'
            WHERE Name = 'Jane Austen' AND PhotoFileName IS NULL;

            UPDATE Authors
            SET PhotoFileName = 'demo-author-miguel-de-cervantes.jpg'
            WHERE Name = 'Miguel de Cervantes' AND PhotoFileName IS NULL;

            UPDATE Books
            SET CoverImageFileName = 'demo-book-cien-anos-de-soledad.jpg'
            WHERE Title = 'Cien años de soledad' AND CoverImageFileName IS NULL;

            UPDATE Books
            SET CoverImageFileName = 'demo-book-orgullo-y-prejuicio.jpg'
            WHERE Title = 'Orgullo y prejuicio' AND CoverImageFileName IS NULL;

            UPDATE Books
            SET CoverImageFileName = 'demo-book-don-quijote.jpg'
            WHERE Title = 'Don Quijote de la Mancha' AND CoverImageFileName IS NULL;

            UPDATE Books
            SET CoverImageFileName = 'demo-book-biblioteca-de-los-suenos.jpg'
            WHERE Title = 'La biblioteca de los sueños' AND CoverImageFileName IS NULL;

            UPDATE Books
            SET CoverImageFileName = 'demo-book-amor-en-tiempos-del-colera.jpg'
            WHERE Title = 'El amor en los tiempos del cólera' AND CoverImageFileName IS NULL;

            UPDATE Books
            SET CoverImageFileName = 'demo-book-sentido-y-sensibilidad.jpg'
            WHERE Title = 'Sentido y sensibilidad' AND CoverImageFileName IS NULL;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE Authors
            SET PhotoFileName = NULL
            WHERE PhotoFileName IN (
                'demo-author-garcia-marquez.jpg',
                'demo-author-jane-austen.jpg',
                'demo-author-miguel-de-cervantes.jpg');

            UPDATE Books
            SET CoverImageFileName = NULL
            WHERE CoverImageFileName IN (
                'demo-book-cien-anos-de-soledad.jpg',
                'demo-book-orgullo-y-prejuicio.jpg',
                'demo-book-don-quijote.jpg',
                'demo-book-biblioteca-de-los-suenos.jpg',
                'demo-book-amor-en-tiempos-del-colera.jpg',
                'demo-book-sentido-y-sensibilidad.jpg');
            """);
    }
}
