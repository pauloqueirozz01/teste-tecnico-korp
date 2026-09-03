import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./screens/dashboard/dashboard').then((m) => m.DashboardScreen)
  },
  {
    path: 'produtos',
    loadComponent: () => import('./screens/products/products').then((m) => m.ProductsScreen)
  },
  {
    path: 'produtos/novo',
    loadComponent: () => import('./screens/product-create/product-create').then((m) => m.ProductCreateScreen)
  },
  {
    path: 'notas-fiscais',
    loadComponent: () => import('./screens/invoices/invoices').then((m) => m.InvoicesScreen)
  },
  {
    path: 'notas-fiscais/nova',
    loadComponent: () => import('./screens/invoice-create/invoice-create').then((m) => m.InvoiceCreateScreen)
  },
  {
    path: 'notas-fiscais/:id',
    loadComponent: () => import('./screens/invoice-details/invoice-details').then((m) => m.InvoiceDetailsScreen)
  },
  {
    path: '**',
    loadComponent: () => import('./screens/not-found/not-found').then((m) => m.NotFoundScreen)
  }
];
