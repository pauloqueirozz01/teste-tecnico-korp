using KorpTeste.InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KorpTeste.InventoryService.Infrastructure.Persistence.Configurations;

public sealed class OperacaoEstoqueIdempotenteConfiguration : IEntityTypeConfiguration<OperacaoEstoqueIdempotente>
{
    public const int ChaveMaxLength = 120;
    public const int TipoMaxLength = 30;

    public void Configure(EntityTypeBuilder<OperacaoEstoqueIdempotente> builder)
    {
        builder.ToTable("operacoes_estoque_idempotentes");

        builder.HasKey(operacao => operacao.Id);

        builder.Property(operacao => operacao.Id)
            .HasColumnName("id");

        builder.Property(operacao => operacao.Chave)
            .HasColumnName("chave")
            .HasMaxLength(ChaveMaxLength)
            .IsRequired();

        builder.HasIndex(operacao => operacao.Chave)
            .IsUnique();

        builder.Property(operacao => operacao.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(TipoMaxLength)
            .IsRequired();

        builder.Property(operacao => operacao.RespostaJson)
            .HasColumnName("resposta_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(operacao => operacao.CriadaEm)
            .HasColumnName("criada_em")
            .IsRequired();
    }
}
