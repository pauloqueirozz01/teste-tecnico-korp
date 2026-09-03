import { Component, input } from '@angular/core';
import { Produto } from '../../../core/models/product.model';

@Component({
  selector: 'app-product-table',
  templateUrl: './product-table.html',
  styleUrl: './product-table.css'
})
export class ProductTableComponent {
  produtos = input.required<Produto[]>();
}
