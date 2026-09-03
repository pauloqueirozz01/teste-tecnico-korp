import { Component } from '@angular/core';
import { EmptyStateComponent } from '../../components/shared/empty-state/empty-state';
import { PageHeaderComponent } from '../../components/shared/page-header/page-header';

@Component({
  selector: 'app-product-create-screen',
  imports: [PageHeaderComponent, EmptyStateComponent],
  templateUrl: './product-create.html'
})
export class ProductCreateScreen {}
