using KorpTeste.InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KorpTeste.InventoryService.Infrastructure.Persistence.Configurations;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public const int CodigoMaxLength = 50;
    public const int DescricaoMaxLength = 200;

    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos");

        builder.HasKey(produto => produto.Id);

        builder.Property(produto => produto.Id)
            .HasColumnName("id");

        builder.Property(produto => produto.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(CodigoMaxLength)
            .IsRequired();

        builder.HasIndex(produto => produto.Codigo)
            .IsUnique();

        builder.Property(produto => produto.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(DescricaoMaxLength)
            .IsRequired();

        builder.Property(produto => produto.Saldo)
            .HasColumnName("saldo")
            .IsRequired();

        builder.ToTable(tabela =>
            tabela.HasCheckConstraint("CK_produtos_saldo_nao_negativo", "saldo >= 0"));

        builder.Property(produto => produto.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(produto => produto.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}

