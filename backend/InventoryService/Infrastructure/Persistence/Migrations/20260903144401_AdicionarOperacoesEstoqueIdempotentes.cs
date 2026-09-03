using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KorpTeste.InventoryService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarOperacoesEstoqueIdempotentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operacoes_estoque_idempotentes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    resposta_json = table.Column<string>(type: "jsonb", nullable: false),
                    criada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operacoes_estoque_idempotentes", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_operacoes_estoque_idempotentes_chave",
                table: "operacoes_estoque_idempotentes",
                column: "chave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operacoes_estoque_idempotentes");
        }
    }
}
