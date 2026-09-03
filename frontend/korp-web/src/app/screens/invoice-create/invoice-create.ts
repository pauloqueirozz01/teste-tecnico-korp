import { Component } from '@angular/core';
import { EmptyStateComponent } from '../../components/shared/empty-state/empty-state';
import { PageHeaderComponent } from '../../components/shared/page-header/page-header';

@Component({
  selector: 'app-invoice-create-screen',
  imports: [PageHeaderComponent, EmptyStateComponent],
  templateUrl: './invoice-create.html'
})
export class InvoiceCreateScreen {}
