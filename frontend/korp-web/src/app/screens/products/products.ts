import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EmptyStateComponent } from '../../components/shared/empty-state/empty-state';
import { PageHeaderComponent } from '../../components/shared/page-header/page-header';

@Component({
  selector: 'app-products-screen',
  imports: [RouterLink, PageHeaderComponent, EmptyStateComponent],
  templateUrl: './products.html',
  styleUrl: './products.css'
})
export class ProductsScreen {}
