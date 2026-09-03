import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { NotaFiscalService } from './nota-fiscal.service';
import { NotaFiscalResumo } from '../models/invoice.model';

describe('NotaFiscalService', () => {
  let service: NotaFiscalService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(NotaFiscalService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('deve listar notas usando a URL configurada do BillingService', () => {
    const notas: NotaFiscalResumo[] = [
      {
        id: 'nota-1',
        numero: 1,
        status: 'Aberta',
        criadaEm: '2026-09-03T10:00:00Z',
        fechadaEm: null,
        quantidadeItens: 1
      }
    ];

    service.listarNotas().subscribe((resposta) => {
      expect(resposta).toEqual(notas);
    });

    const request = httpMock.expectOne('http://localhost:5002/api/notas-fiscais');
    expect(request.request.method).toBe('GET');
    request.flush(notas);
  });

  it('deve buscar nota por id', () => {
    service.buscarNota('nota-1').subscribe();

    const request = httpMock.expectOne('http://localhost:5002/api/notas-fiscais/nota-1');
    expect(request.request.method).toBe('GET');
    request.flush({
      id: 'nota-1',
      numero: 1,
      status: 'Aberta',
      criadaEm: '2026-09-03T10:00:00Z',
      fechadaEm: null,
      itens: [],
      nomeArquivo: null,
      geradaEm: null
    });
  });

  it('deve preparar processamento da nota fiscal', () => {
    service.processarNota('nota-1').subscribe();

    const request = httpMock.expectOne('http://localhost:5002/api/notas-fiscais/nota-1/processar');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush({
      notaFiscal: {
        id: 'nota-1',
        numero: 1,
        status: 'Fechada',
        criadaEm: '2026-09-03T10:00:00Z',
        fechadaEm: '2026-09-03T10:01:00Z',
        itens: [],
        nomeArquivo: 'NF-000001.txt',
        geradaEm: '2026-09-03T10:01:00Z'
      },
      nomeArquivo: 'NF-000001.txt'
    });
  });
});
