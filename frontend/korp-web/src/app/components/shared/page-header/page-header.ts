import { Component, input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  templateUrl: './page-header.html',
  styleUrl: './page-header.css'
})
export class PageHeaderComponent {
  titulo = input.required<string>();
  descricao = input<string>();
}
