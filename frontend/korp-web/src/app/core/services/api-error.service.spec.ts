import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ApiErrorService } from './api-error.service';

describe('ApiErrorService', () => {
  let service: ApiErrorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ApiErrorService);
  });

  it('deve priorizar mensagem segura retornada pela API', () => {
    const erro = new HttpErrorResponse({
      status: 409,
      error: {
        codigo: 'SALDO_INSUFICIENTE',
        mensagem: 'O produto não possui saldo suficiente para concluir a operação.',
        status: 409
      }
    });

    const resultado = service.mapear(erro);

    expect(resultado.codigo).toBe('SALDO_INSUFICIENTE');
    expect(resultado.mensagem).toContain('saldo suficiente');
  });

  it('deve mapear indisponibilidade sem expor detalhes técnicos', () => {
    const erro = new HttpErrorResponse({ status: 0, statusText: 'Unknown Error' });

    const resultado = service.mapear(erro);

    expect(resultado.mensagem).toBe(
      'O serviço está temporariamente indisponível. Tente novamente em alguns instantes.'
    );
  });
});
