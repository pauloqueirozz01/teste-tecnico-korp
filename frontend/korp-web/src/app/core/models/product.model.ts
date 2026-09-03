export interface Produto {
  id: string;
  codigo: string;
  descricao: string;
  saldo: number;
  criadoEm: string;
  atualizadoEm: string;
}

export interface CriarProdutoRequest {
  codigo: string;
  descricao: string;
  saldo: number;
}
