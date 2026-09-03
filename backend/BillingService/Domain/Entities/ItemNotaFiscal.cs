using KorpTeste.BillingService.Domain.Exceptions;

namespace KorpTeste.BillingService.Domain.Entities;

public sealed class ItemNotaFiscal
{
    private ItemNotaFiscal()
    {
        CodigoProduto = string.Empty;
        DescricaoProduto = string.Empty;
    }

    public ItemNotaFiscal(Guid produtoId, string codigoProduto, string descricaoProduto, int quantidade)
    {
        if (produtoId == Guid.Empty)
        {
            throw new ProdutoItemInvalidoException("O produto do item da nota fiscal é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(codigoProduto))
        {
            throw new ProdutoItemInvalidoException("O código do produto do item da nota fiscal é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(descricaoProduto))
        {
            throw new ProdutoItemInvalidoException("A descrição do produto do item da nota fiscal é obrigatória.");
        }

        if (quantidade <= 0)
        {
            throw new QuantidadeItemInvalidaException();
        }

        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        CodigoProduto = codigoProduto.Trim().ToUpperInvariant();
        DescricaoProduto = descricaoProduto.Trim();
        Quantidade = quantidade;
    }

    public Guid Id { get; private set; }

    public Guid NotaFiscalId { get; private set; }

    public NotaFiscal NotaFiscal { get; private set; } = null!;

    public Guid ProdutoId { get; private set; }

    public string CodigoProduto { get; private set; }

    public string DescricaoProduto { get; private set; }

    public int Quantidade { get; private set; }
}
