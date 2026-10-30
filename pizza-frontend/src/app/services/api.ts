import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = 'http://localhost:5217/api'; // Porta da API C#

  constructor(private http: HttpClient) { }

  getItens(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/itens`);
  }
}