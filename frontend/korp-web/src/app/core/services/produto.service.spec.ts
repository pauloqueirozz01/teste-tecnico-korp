import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ProdutoService } from './produto.service';
import { CriarProdutoRequest, Produto } from '../models/product.model';

describe('ProdutoService', () => {
  let service: ProdutoService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(ProdutoService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('deve listar produtos usando a URL configurada do InventoryService', () => {
    const produtos: Produto[] = [
      {
        id: 'produto-1',
        codigo: 'PROD-001',
        descricao: 'Teclado',
        saldo: 10,
        criadoEm: '2026-09-03T10:00:00Z',
        atualizadoEm: '2026-09-03T10:00:00Z'
      }
    ];

    service.listarProdutos().subscribe((resposta) => {
      expect(resposta).toEqual(produtos);
    });

    const request = httpMock.expectOne('http://localhost:5001/api/produtos');
    expect(request.request.method).toBe('GET');
    request.flush(produtos);
  });

  it('deve criar produto com payload tipado', () => {
    const payload: CriarProdutoRequest = {
      codigo: 'PROD-002',
      descricao: 'Mouse',
      saldo: 5
    };

    service.criarProduto(payload).subscribe();

    const request = httpMock.expectOne('http://localhost:5001/api/produtos');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush({ ...payload, id: 'produto-2', criadoEm: '2026-09-03T10:00:00Z', atualizadoEm: '2026-09-03T10:00:00Z' });
  });
});
