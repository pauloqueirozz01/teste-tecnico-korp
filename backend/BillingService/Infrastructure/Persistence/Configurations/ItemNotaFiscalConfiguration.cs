using KorpTeste.BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KorpTeste.BillingService.Infrastructure.Persistence.Configurations;

public sealed class ItemNotaFiscalConfiguration : IEntityTypeConfiguration<ItemNotaFiscal>
{
    public void Configure(EntityTypeBuilder<ItemNotaFiscal> builder)
    {
        builder.ToTable("itens_nota_fiscal");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.NotaFiscalId)
            .HasColumnName("nota_fiscal_id")
            .IsRequired();

        builder.Property(item => item.ProdutoId)
            .HasColumnName("produto_id")
            .IsRequired();

        builder.Property(item => item.CodigoProduto)
            .HasColumnName("codigo_produto")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(item => item.DescricaoProduto)
            .HasColumnName("descricao_produto")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.Quantidade)
            .HasColumnName("quantidade")
            .IsRequired();

        builder.HasIndex(item => item.NotaFiscalId);

        builder.HasIndex(item => new { item.NotaFiscalId, item.ProdutoId })
            .IsUnique();

        builder.ToTable(tabela =>
            tabela.HasCheckConstraint("ck_itens_nota_fiscal_quantidade_positiva", "quantidade > 0"));
    }
}
