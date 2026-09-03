import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { EmptyStateComponent } from './empty-state';

@Component({
  imports: [EmptyStateComponent],
  template: '<app-empty-state titulo="Sem registros" descricao="Nada encontrado." />'
})
class EmptyStateHost {}

describe('EmptyStateComponent', () => {
  it('deve renderizar título e descrição', async () => {
    await TestBed.configureTestingModule({
      imports: [EmptyStateHost]
    }).compileComponents();

    const fixture = TestBed.createComponent(EmptyStateHost);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Sem registros');
    expect(fixture.nativeElement.textContent).toContain('Nada encontrado.');
  });
});
