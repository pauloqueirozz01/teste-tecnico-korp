export interface ErroApi {
  codigo: string;
  mensagem: string;
  status: number;
}

export interface ErroAplicacao {
  mensagem: string;
  codigo?: string;
  status?: number;
}
