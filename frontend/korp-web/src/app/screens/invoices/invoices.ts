import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EmptyStateComponent } from '../../components/shared/empty-state/empty-state';
import { PageHeaderComponent } from '../../components/shared/page-header/page-header';

@Component({
  selector: 'app-invoices-screen',
  imports: [RouterLink, PageHeaderComponent, EmptyStateComponent],
  templateUrl: './invoices.html',
  styleUrl: './invoices.css'
})
export class InvoicesScreen {}
