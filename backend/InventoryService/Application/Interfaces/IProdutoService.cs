using KorpTeste.InventoryService.Application.DTOs;

namespace KorpTeste.InventoryService.Application.Interfaces;

public interface IProdutoService
{
    Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProdutoResponse>> ListarAsync(CancellationToken cancellationToken);
    Task<ProdutoResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
}

