import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ApiErrorService } from '../../core/services/api-error.service';
import { ProdutoService } from '../../core/services/produto.service';
import { ProductCreateScreen } from './product-create';

describe('ProductCreateScreen', () => {
  function criarTela(criarProduto: jasmine.Spy): ComponentFixture<ProductCreateScreen> {
    TestBed.configureTestingModule({
      imports: [ProductCreateScreen],
      providers: [
        provideRouter([]),
        {
          provide: ProdutoService,
          useValue: { criarProduto }
        },
        ApiErrorService
      ]
    });

    return TestBed.createComponent(ProductCreateScreen);
  }

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('deve cadastrar produto e voltar para listagem com feedback', () => {
    const criarProduto = jasmine.createSpy().and.returnValue(
      of({
        id: 'produto-1',
        codigo: 'PROD-001',
        descricao: 'Teclado',
        saldo: 10,
        criadoEm: '2026-09-03T10:00:00Z',
        atualizadoEm: '2026-09-03T10:00:00Z'
      })
    );
    const fixture = criarTela(criarProduto);
    const router = TestBed.inject(Router);
    spyOn(router, 'navigate');
    fixture.detectChanges();

    const inputs = fixture.debugElement.queryAll(By.css('input'));
    inputs[0].nativeElement.value = 'PROD-001';
    inputs[0].nativeElement.dispatchEvent(new Event('input'));
    inputs[1].nativeElement.value = 'Teclado';
    inputs[1].nativeElement.dispatchEvent(new Event('input'));
    inputs[2].nativeElement.value = '10';
    inputs[2].nativeElement.dispatchEvent(new Event('input'));

    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit');

    expect(criarProduto).toHaveBeenCalledWith({
      codigo: 'PROD-001',
      descricao: 'Teclado',
      saldo: 10
    });
    expect(router.navigate).toHaveBeenCalledWith(['/produtos'], { state: { produtoCriado: true } });
  });

  it('deve navegar para listagem ao cancelar', () => {
    const criarProduto = jasmine.createSpy();
    const fixture = criarTela(criarProduto);
    const router = TestBed.inject(Router);
    spyOn(router, 'navigate');
    fixture.detectChanges();

    fixture.debugElement.query(By.css('button[type="button"]')).nativeElement.click();

    expect(criarProduto).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/produtos']);
  });

  it('deve manter usuário na tela e não chamar service quando submit for inválido', () => {
    const criarProduto = jasmine.createSpy();
    const fixture = criarTela(criarProduto);
    const router = TestBed.inject(Router);
    spyOn(router, 'navigate');
    fixture.detectChanges();

    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit');
    fixture.detectChanges();

    expect(criarProduto).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Informe o código do produto.');
  });

  it('deve exibir erro quando API recusar cadastro', () => {
    const criarProduto = jasmine
      .createSpy()
      .and.returnValue(throwError(() => new Error('falha')));
    const fixture = criarTela(criarProduto);
    fixture.detectChanges();

    const inputs = fixture.debugElement.queryAll(By.css('input'));
    inputs[0].nativeElement.value = 'PROD-001';
    inputs[0].nativeElement.dispatchEvent(new Event('input'));
    inputs[1].nativeElement.value = 'Teclado';
    inputs[1].nativeElement.dispatchEvent(new Event('input'));
    inputs[2].nativeElement.value = '10';
    inputs[2].nativeElement.dispatchEvent(new Event('input'));

    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(
      'Não foi possível concluir a operação. Tente novamente.'
    );
  });
});
