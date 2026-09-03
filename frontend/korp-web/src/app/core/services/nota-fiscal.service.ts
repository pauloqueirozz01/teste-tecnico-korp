import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../config/api.config';
import {
  CriarNotaFiscalRequest,
  NotaFiscal,
  NotaFiscalResumo,
  ResultadoProcessamentoNotaFiscal
} from '../models/invoice.model';

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(API_CONFIG);
  private readonly baseUrl = `${this.apiConfig.billingApiUrl}/api/notas-fiscais`;

  listarNotas(): Observable<NotaFiscalResumo[]> {
    return this.http.get<NotaFiscalResumo[]>(this.baseUrl);
  }

  buscarNota(id: string): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(`${this.baseUrl}/${id}`);
  }

  criarNota(request: CriarNotaFiscalRequest): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.baseUrl, request);
  }

  processarNota(id: string): Observable<ResultadoProcessamentoNotaFiscal> {
    return this.http.post<ResultadoProcessamentoNotaFiscal>(`${this.baseUrl}/${id}/processar`, null);
  }

  baixarArquivo(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/arquivo`, { responseType: 'blob' });
  }
}
