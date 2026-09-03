using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KorpTeste.BillingService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMetadadosProcessamentoNota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "caminho_arquivo",
                table: "notas_fiscais",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "gerada_em",
                table: "notas_fiscais",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nome_arquivo",
                table: "notas_fiscais",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "processamento_em_andamento",
                table: "notas_fiscais",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "versao",
                table: "notas_fiscais",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "caminho_arquivo",
                table: "notas_fiscais");

            migrationBuilder.DropColumn(
                name: "gerada_em",
                table: "notas_fiscais");

            migrationBuilder.DropColumn(
                name: "nome_arquivo",
                table: "notas_fiscais");

            migrationBuilder.DropColumn(
                name: "processamento_em_andamento",
                table: "notas_fiscais");

            migrationBuilder.DropColumn(
                name: "versao",
                table: "notas_fiscais");
        }
    }
}
