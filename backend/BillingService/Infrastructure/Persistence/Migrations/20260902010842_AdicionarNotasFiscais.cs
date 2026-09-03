using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KorpTeste.BillingService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarNotasFiscais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "nota_fiscal_numero_seq");

            migrationBuilder.CreateTable(
                name: "notas_fiscais",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    criada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fechada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notas_fiscais", x => x.id);
                    table.CheckConstraint("ck_notas_fiscais_numero_positivo", "numero > 0");
                    table.CheckConstraint("ck_notas_fiscais_status_valido", "status IN ('Aberta', 'Fechada')");
                });

            migrationBuilder.CreateTable(
                name: "itens_nota_fiscal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nota_fiscal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_produto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descricao_produto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_nota_fiscal", x => x.id);
                    table.CheckConstraint("ck_itens_nota_fiscal_quantidade_positiva", "quantidade > 0");
                    table.ForeignKey(
                        name: "FK_itens_nota_fiscal_notas_fiscais_nota_fiscal_id",
                        column: x => x.nota_fiscal_id,
                        principalTable: "notas_fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_itens_nota_fiscal_nota_fiscal_id",
                table: "itens_nota_fiscal",
                column: "nota_fiscal_id");

            migrationBuilder.CreateIndex(
                name: "IX_itens_nota_fiscal_nota_fiscal_id_produto_id",
                table: "itens_nota_fiscal",
                columns: new[] { "nota_fiscal_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notas_fiscais_numero",
                table: "notas_fiscais",
                column: "numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itens_nota_fiscal");

            migrationBuilder.DropTable(
                name: "notas_fiscais");

            migrationBuilder.DropSequence(
                name: "nota_fiscal_numero_seq");
        }
    }
}
