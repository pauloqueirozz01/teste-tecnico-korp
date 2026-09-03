import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Produto } from '../../core/models/product.model';
import { ApiErrorService } from '../../core/services/api-error.service';
import { ProdutoService } from '../../core/services/produto.service';
import { ProductsScreen } from './products';

describe('ProductsScreen', () => {
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

  function criarTela(listarProdutos: () => ReturnType<ProdutoService['listarProdutos']>): ComponentFixture<ProductsScreen> {
    TestBed.configureTestingModule({
      imports: [ProductsScreen],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: convertToParamMap({})
            }
          }
        },
        {
          provide: ProdutoService,
          useValue: { listarProdutos }
        },
        ApiErrorService
      ]
    });

    return TestBed.createComponent(ProductsScreen);
  }

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('deve listar produtos retornados pela API', () => {
    const fixture = criarTela(() => of(produtos));

    fixture.detectChanges();

    const texto = fixture.nativeElement.textContent as string;
    expect(texto).toContain('PROD-001');
    expect(texto).toContain('Teclado');
  });

  it('deve exibir estado vazio quando API retornar lista vazia', () => {
    const fixture = criarTela(() => of([]));

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Nenhum produto cadastrado');
  });

  it('deve exibir erro amigável quando API falhar', () => {
    const fixture = criarTela(() => throwError(() => new Error('falha')));

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(
      'Não foi possível concluir a operação. Tente novamente.'
    );
  });
});
