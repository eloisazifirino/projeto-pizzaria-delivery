import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-catalogo',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './catalogo.html',
  styleUrls: ['./catalogo.css']
})
export class CatalogoComponent implements OnInit {
  itens: any[] = [];

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    this.apiService.getItens().subscribe({
      next: (data) => this.itens = data,
      error: (err) => console.error('Erro ao carregar menu:', err)
    });
  }

  adicionarAoCarrinho(item: any): void {
    alert(`Item "${item.nome}" adicionado ao pedido!`);
  }
}