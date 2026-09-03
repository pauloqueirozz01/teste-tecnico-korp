import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EmptyStateComponent } from '../../components/shared/empty-state/empty-state';
import { PageHeaderComponent } from '../../components/shared/page-header/page-header';

@Component({
  selector: 'app-invoice-details-screen',
  imports: [PageHeaderComponent, EmptyStateComponent],
  templateUrl: './invoice-details.html'
})
export class InvoiceDetailsScreen {
  private readonly route = inject(ActivatedRoute);
  protected readonly notaFiscalId = this.route.snapshot.paramMap.get('id');
}
