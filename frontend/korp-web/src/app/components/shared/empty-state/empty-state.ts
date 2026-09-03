import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.css'
})
export class EmptyStateComponent {
  titulo = input.required<string>();
  descricao = input<string>();
}
