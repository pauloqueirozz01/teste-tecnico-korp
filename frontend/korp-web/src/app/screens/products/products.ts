import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { ProductTableComponent } from '../../components/products/product-table/product-table';
import { EmptyStateComponent } from '../../components/shared/empty-state/empty-state';
import { ErrorMessageComponent } from '../../components/shared/error-message/error-message';
import { LoadingComponent } from '../../components/shared/loading/loading';
import { PageHeaderComponent } from '../../components/shared/page-header/page-header';
import { Produto } from '../../core/models/product.model';
import { ApiErrorService } from '../../core/services/api-error.service';
import { ProdutoService } from '../../core/services/produto.service';

@Component({
  selector: 'app-products-screen',
  imports: [
    RouterLink,
    PageHeaderComponent,
    EmptyStateComponent,
    LoadingComponent,
    ErrorMessageComponent,
    ProductTableComponent
  ],
  templateUrl: './products.html',
  styleUrl: './products.css'
})
export class ProductsScreen implements OnInit {
  private readonly produtoService = inject(ProdutoService);
  private readonly apiErrorService = inject(ApiErrorService);
  private readonly route = inject(ActivatedRoute);

  protected readonly produtos = signal<Produto[]>([]);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly sucesso = signal<string | null>(null);

  ngOnInit(): void {
    if (this.route.snapshot.queryParamMap.get('criado') === '1') {
      this.sucesso.set('Produto cadastrado com sucesso.');
    }

    this.carregarProdutos();
  }

  protected carregarProdutos(): void {
    this.carregando.set(true);
    this.erro.set(null);

    this.produtoService
      .listarProdutos()
      .pipe(
        catchError((erro) => {
          this.erro.set(this.apiErrorService.mapear(erro).mensagem);
          return of([]);
        }),
        finalize(() => this.carregando.set(false))
      )
      .subscribe((produtos) => this.produtos.set(produtos));
  }
}
