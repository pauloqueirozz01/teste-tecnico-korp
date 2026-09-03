import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ErroApi, ErroAplicacao } from '../models/api-error.model';

@Injectable({ providedIn: 'root' })
export class ApiErrorService {
  mapear(erro: unknown): ErroAplicacao {
    if (erro instanceof HttpErrorResponse) {
      const erroApi = this.extrairErroApi(erro.error);

      if (erroApi) {
        return {
          codigo: erroApi.codigo,
          mensagem: erroApi.mensagem,
          status: erroApi.status
        };
      }

      if (erro.status === 0 || erro.status === 503) {
        return {
          mensagem: 'O serviço está temporariamente indisponível. Tente novamente em alguns instantes.',
          status: erro.status
        };
      }
    }

    return {
      mensagem: 'Não foi possível concluir a operação. Tente novamente.'
    };
  }

  private extrairErroApi(valor: unknown): ErroApi | null {
    if (!valor || typeof valor !== 'object') {
      return null;
    }

    const candidato = valor as Partial<ErroApi>;
    if (
      typeof candidato.codigo === 'string' &&
      typeof candidato.mensagem === 'string' &&
      typeof candidato.status === 'number'
    ) {
      return {
        codigo: candidato.codigo,
        mensagem: candidato.mensagem,
        status: candidato.status
      };
    }

    return null;
  }
}
