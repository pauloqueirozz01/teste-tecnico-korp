import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ProductFormComponent } from './product-form';

@Component({
  imports: [ProductFormComponent],
  template: '<app-product-form (salvar)="produtoSalvo = $event" />'
})
class ProductFormHost {
  produtoSalvo: unknown;
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
    expect(texto).toContain('Informe um código');
    expect(texto).toContain('Informe uma descrição');
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
});
