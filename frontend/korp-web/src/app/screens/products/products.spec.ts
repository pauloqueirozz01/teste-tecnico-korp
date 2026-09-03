import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router, provideRouter } from '@angular/router';
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

  function criarTela(
    listarProdutos: jasmine.Spy | (() => ReturnType<ProdutoService['listarProdutos']>)
  ): ComponentFixture<ProductsScreen> {
    TestBed.configureTestingModule({
      imports: [ProductsScreen],
      providers: [
        provideRouter([]),
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
    const listarProdutos = jasmine.createSpy().and.returnValue(of(produtos));
    const fixture = criarTela(listarProdutos);

    fixture.detectChanges();

    const texto = fixture.nativeElement.textContent as string;
    expect(listarProdutos).toHaveBeenCalledTimes(1);
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

  it('deve permitir retry manual após erro de carregamento', () => {
    const listarProdutos = jasmine
      .createSpy()
      .and.returnValues(throwError(() => new Error('falha')), of(produtos));
    const fixture = criarTela(listarProdutos);
    fixture.detectChanges();

    fixture.debugElement.query(By.css('button')).nativeElement.click();
    fixture.detectChanges();

    expect(listarProdutos).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('PROD-001');
  });

  it('deve exibir feedback de sucesso vindo do state de navegação sem manter query param', () => {
    history.replaceState({ produtoCriado: true }, '');
    const fixture = criarTela(() => of([]));
    const router = TestBed.inject(Router);
    spyOn(router, 'navigate');

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Produto cadastrado com sucesso.');
    expect(router.navigate).toHaveBeenCalledWith([], { replaceUrl: true, state: {} });
    history.replaceState({}, '');
  });
});
