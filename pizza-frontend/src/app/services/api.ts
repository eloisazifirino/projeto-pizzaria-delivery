import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = 'http://localhost:5217/api';

  constructor(private http: HttpClient) { }

  // ITENS
  getItens(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/itens`);
  }

  // PEDIDOS (CRUD)
  getPedidos(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/atendimentos`);
  }

  criarPedido(pedido: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/atendimentos`, pedido);
  }

  cancelarPedido(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/atendimentos/${id}`);
  }
}