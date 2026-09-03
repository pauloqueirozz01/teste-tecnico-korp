import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../config/api.config';
import { CriarProdutoRequest, Produto } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(API_CONFIG);
  private readonly baseUrl = `${this.apiConfig.inventoryApiUrl}/api/produtos`;

  listarProdutos(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.baseUrl);
  }

  buscarProduto(id: string): Observable<Produto> {
    return this.http.get<Produto>(`${this.baseUrl}/${id}`);
  }

  criarProduto(request: CriarProdutoRequest): Observable<Produto> {
    return this.http.post<Produto>(this.baseUrl, request);
  }
}
