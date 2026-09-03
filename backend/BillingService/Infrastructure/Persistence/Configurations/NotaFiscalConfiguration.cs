using KorpTeste.BillingService.Domain.Entities;
using KorpTeste.BillingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KorpTeste.BillingService.Infrastructure.Persistence.Configurations;

public sealed class NotaFiscalConfiguration : IEntityTypeConfiguration<NotaFiscal>
{
    public void Configure(EntityTypeBuilder<NotaFiscal> builder)
    {
        builder.ToTable("notas_fiscais");

        builder.HasKey(notaFiscal => notaFiscal.Id);

        builder.Property(notaFiscal => notaFiscal.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(notaFiscal => notaFiscal.Numero)
            .HasColumnName("numero")
            .IsRequired();

        builder.HasIndex(notaFiscal => notaFiscal.Numero)
            .IsUnique();

        builder.Property(notaFiscal => notaFiscal.Status)
            .HasColumnName("status")
            .HasConversion(
                status => status.ToString(),
                valor => Enum.Parse<StatusNotaFiscal>(valor))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(notaFiscal => notaFiscal.CriadaEm)
            .HasColumnName("criada_em")
            .IsRequired();

        builder.Property(notaFiscal => notaFiscal.FechadaEm)
            .HasColumnName("fechada_em");

        builder.Property(notaFiscal => notaFiscal.ProcessamentoEmAndamento)
            .HasColumnName("processamento_em_andamento")
            .IsRequired();

        builder.Property(notaFiscal => notaFiscal.Versao)
            .HasColumnName("versao")
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(notaFiscal => notaFiscal.NomeArquivo)
            .HasColumnName("nome_arquivo")
            .HasMaxLength(150);

        builder.Property(notaFiscal => notaFiscal.CaminhoArquivo)
            .HasColumnName("caminho_arquivo")
            .HasMaxLength(300);

        builder.Property(notaFiscal => notaFiscal.GeradaEm)
            .HasColumnName("gerada_em");

        builder.HasMany(notaFiscal => notaFiscal.Itens)
            .WithOne(item => item.NotaFiscal)
            .HasForeignKey(item => item.NotaFiscalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(tabela =>
        {
            tabela.HasCheckConstraint("ck_notas_fiscais_numero_positivo", "numero > 0");
            tabela.HasCheckConstraint("ck_notas_fiscais_status_valido", "status IN ('Aberta', 'Fechada')");
        });
    }
}
