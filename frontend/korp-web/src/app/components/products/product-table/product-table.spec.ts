import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Produto } from '../../../core/models/product.model';
import { ProductTableComponent } from './product-table';

@Component({
  imports: [ProductTableComponent],
  template: '<app-product-table [produtos]="produtos" />'
})
class ProductTableHost {
  produtos: Produto[] = [
    {
      id: 'produto-1',
      codigo: 'PROD-001',
      descricao: 'Teclado',
      saldo: 10,
      criadoEm: '2026-09-03T10:00:00Z',
      atualizadoEm: '2026-09-03T10:00:00Z'
    }
  ];
}

describe('ProductTableComponent', () => {
  it('deve renderizar código, descrição e saldo', async () => {
    await TestBed.configureTestingModule({
      imports: [ProductTableHost],
      providers: [provideRouter([])]
    }).compileComponents();

    const fixture = TestBed.createComponent(ProductTableHost);
    fixture.detectChanges();

    const texto = fixture.nativeElement.textContent as string;
    expect(texto).toContain('PROD-001');
    expect(texto).toContain('Teclado');
    expect(texto).toContain('10');
  });
});
