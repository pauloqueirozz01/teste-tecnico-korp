using KorpTeste.BillingService.Domain.Enums;
using KorpTeste.BillingService.Domain.Exceptions;

namespace KorpTeste.BillingService.Domain.Entities;

public sealed class NotaFiscal
{
    private NotaFiscal()
    {
        Itens = [];
    }

    public NotaFiscal(long numero, IEnumerable<ItemNotaFiscal> itens)
    {
        if (numero <= 0)
        {
            throw new RequisicaoInvalidaException("O número da nota fiscal deve ser maior que zero.");
        }

        var itensNormalizados = itens.ToList();
        if (itensNormalizados.Count == 0)
        {
            throw new NotaFiscalSemItensException();
        }

        Id = Guid.NewGuid();
        Numero = numero;
        Status = StatusNotaFiscal.Aberta;
        CriadaEm = DateTimeOffset.UtcNow;
        Itens = itensNormalizados;
        Versao = 1;
    }

    public Guid Id { get; private set; }

    public long Numero { get; private set; }

    public StatusNotaFiscal Status { get; private set; }

    public DateTimeOffset CriadaEm { get; private set; }

    public DateTimeOffset? FechadaEm { get; private set; }

    public bool ProcessamentoEmAndamento { get; private set; }

    public int Versao { get; private set; }

    public string? NomeArquivo { get; private set; }

    public string? CaminhoArquivo { get; private set; }

    public DateTimeOffset? GeradaEm { get; private set; }

    public ICollection<ItemNotaFiscal> Itens { get; private set; }

    public void IniciarProcessamento()
    {
        if (Status != StatusNotaFiscal.Aberta)
        {
            throw new NotaFiscalJaFechadaException();
        }

        if (ProcessamentoEmAndamento)
        {
            throw new NotaFiscalEmProcessamentoException();
        }

        ProcessamentoEmAndamento = true;
        Versao++;
    }

    public void LiberarProcessamento()
    {
        ProcessamentoEmAndamento = false;
        Versao++;
    }

    public void Fechar(string nomeArquivo, string caminhoArquivo, DateTimeOffset geradaEm)
    {
        if (Status != StatusNotaFiscal.Aberta || !ProcessamentoEmAndamento)
        {
            throw new NotaFiscalJaFechadaException();
        }

        Status = StatusNotaFiscal.Fechada;
        FechadaEm = geradaEm;
        GeradaEm = geradaEm;
        NomeArquivo = nomeArquivo;
        CaminhoArquivo = caminhoArquivo;
        ProcessamentoEmAndamento = false;
        Versao++;
    }
}
