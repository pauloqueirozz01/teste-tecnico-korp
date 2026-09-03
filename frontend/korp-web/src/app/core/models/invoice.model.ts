export type StatusNotaFiscal = 'Aberta' | 'Fechada' | string;

export interface ItemNotaFiscal {
  produtoId: string;
  codigoProduto: string;
  descricaoProduto: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: string;
  numero: number;
  status: StatusNotaFiscal;
  criadaEm: string;
  fechadaEm: string | null;
  itens: ItemNotaFiscal[];
  nomeArquivo: string | null;
  geradaEm: string | null;
}

export interface NotaFiscalResumo {
  id: string;
  numero: number;
  status: StatusNotaFiscal;
  criadaEm: string;
  fechadaEm: string | null;
  quantidadeItens: number;
}

export interface CriarItemNotaFiscalRequest {
  produtoId: string;
  codigoProduto: string;
  descricaoProduto: string;
  quantidade: number;
}

export interface CriarNotaFiscalRequest {
  itens: CriarItemNotaFiscalRequest[];
}

export interface ResultadoProcessamentoNotaFiscal {
  notaFiscal: NotaFiscal;
  nomeArquivo: string;
}
