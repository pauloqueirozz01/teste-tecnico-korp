import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ProductFormComponent } from './product-form';

@Component({
  imports: [ProductFormComponent],
  template: '<app-product-form (salvar)="produtoSalvo = $event" (cancelar)="cancelou = true" />'
})
class ProductFormHost {
  produtoSalvo: unknown;
  cancelou = false;
}

describe('ProductFormComponent', () => {
  it('deve exibir validações quando formulário inválido for submetido', async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFormHost]
    }).compileComponents();

    const fixture = TestBed.createComponent(ProductFormHost);
    fixture.detectChanges();

    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit');
    fixture.detectChanges();

    const texto = fixture.nativeElement.textContent as string;
    expect(texto).toContain('Informe o código do produto.');
    expect(texto).toContain('Informe a descrição do produto.');
  });

  it('deve aceitar saldo zero como válido', async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFormHost]
    }).compileComponents();

    const fixture = TestBed.createComponent(ProductFormHost);
    fixture.detectChanges();

    const inputs = fixture.debugElement.queryAll(By.css('input'));
    inputs[0].nativeElement.value = 'PROD-ZERO';
    inputs[0].nativeElement.dispatchEvent(new Event('input'));
    inputs[1].nativeElement.value = 'Produto sem saldo inicial';
    inputs[1].nativeElement.dispatchEvent(new Event('input'));
    inputs[2].nativeElement.value = '0';
    inputs[2].nativeElement.dispatchEvent(new Event('input'));

    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit');

    expect(fixture.componentInstance.produtoSalvo).toEqual({
      codigo: 'PROD-ZERO',
      descricao: 'Produto sem saldo inicial',
      saldo: 0
    });
  });

  it('deve rejeitar saldo negativo', async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFormHost]
    }).compileComponents();

    const fixture = TestBed.createComponent(ProductFormHost);
    fixture.detectChanges();

    const inputs = fixture.debugElement.queryAll(By.css('input'));
    inputs[0].nativeElement.value = 'PROD-NEG';
    inputs[0].nativeElement.dispatchEvent(new Event('input'));
    inputs[1].nativeElement.value = 'Produto inválido';
    inputs[1].nativeElement.dispatchEvent(new Event('input'));
    inputs[2].nativeElement.value = '-1';
    inputs[2].nativeElement.dispatchEvent(new Event('input'));

    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit');
    fixture.detectChanges();

    expect(fixture.componentInstance.produtoSalvo).toBeUndefined();
    expect(fixture.nativeElement.textContent).toContain('O saldo não pode ser negativo.');
  });

  it('deve emitir payload normalizado quando formulário for válido', async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFormHost]
    }).compileComponents();

    const fixture = TestBed.createComponent(ProductFormHost);
    fixture.detectChanges();

    const inputs = fixture.debugElement.queryAll(By.css('input'));
    inputs[0].nativeElement.value = ' prod-001 ';
    inputs[0].nativeElement.dispatchEvent(new Event('input'));
    inputs[1].nativeElement.value = ' Teclado ';
    inputs[1].nativeElement.dispatchEvent(new Event('input'));
    inputs[2].nativeElement.value = '7';
    inputs[2].nativeElement.dispatchEvent(new Event('input'));

    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit');

    expect(fixture.componentInstance.produtoSalvo).toEqual({
      codigo: 'prod-001',
      descricao: 'Teclado',
      saldo: 7
    });
  });

  it('deve emitir cancelamento por botão sem submeter o formulário', async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFormHost]
    }).compileComponents();

    const fixture = TestBed.createComponent(ProductFormHost);
    fixture.detectChanges();

    fixture.debugElement.query(By.css('button[type="button"]')).nativeElement.click();

    expect(fixture.componentInstance.cancelou).toBeTrue();
    expect(fixture.componentInstance.produtoSalvo).toBeUndefined();
  });
});
