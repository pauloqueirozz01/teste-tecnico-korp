import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, EMPTY, finalize } from 'rxjs';
import { ProductFormComponent } from '../../components/products/product-form/product-form';
import { ErrorMessageComponent } from '../../components/shared/error-message/error-message';
import { PageHeaderComponent } from '../../components/shared/page-header/page-header';
import { CriarProdutoRequest } from '../../core/models/product.model';
import { ApiErrorService } from '../../core/services/api-error.service';
import { ProdutoService } from '../../core/services/produto.service';

@Component({
  selector: 'app-product-create-screen',
  imports: [PageHeaderComponent, ErrorMessageComponent, ProductFormComponent],
  templateUrl: './product-create.html',
  styleUrl: './product-create.css'
})
export class ProductCreateScreen {
  private readonly produtoService = inject(ProdutoService);
  private readonly apiErrorService = inject(ApiErrorService);
  private readonly router = inject(Router);

  protected readonly salvando = signal(false);
  protected readonly erro = signal<string | null>(null);

  protected cancelar(): void {
    this.router.navigate(['/produtos']);
  }

  protected salvarProduto(request: CriarProdutoRequest): void {
    this.salvando.set(true);
    this.erro.set(null);

    this.produtoService
      .criarProduto(request)
      .pipe(
        catchError((erro) => {
          this.erro.set(this.apiErrorService.mapear(erro).mensagem);
          return EMPTY;
        }),
        finalize(() => this.salvando.set(false))
      )
      .subscribe(() => {
        this.router.navigate(['/produtos'], { state: { produtoCriado: true } });
      });
  }
}
