import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-pedidos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pedidos.component.html',
  styleUrls: ['./pedidos.component.css']
})
export class PedidosComponent implements OnInit {
  cardapio: any[] = [];
  pedidos: any[] = [];

  clienteNome: string = '';
  itemSelecionadoId: number | null = null;
  observacao: string = '';

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.carregarDados();
  }

  carregarDados(): void {
    this.apiService.getItens().subscribe(data => this.cardapio = data);
    this.apiService.getPedidos().subscribe(data => this.pedidos = data);
  }

  finalizarPedido(): void {
    if (!this.clienteNome || !this.itemSelecionadoId) {
      alert('Por favor, informe o nome do cliente e selecione um item!');
      return;
    }

    const itemObj = this.cardapio.find(i => i.idItem === Number(this.itemSelecionadoId));

    const novoPedido = {
      observacao: `Cliente: ${this.clienteNome}. Obs: ${this.observacao}`,
      valorTotal: itemObj ? itemObj.preco : 0,
      atendimentoItens: [
        {
          idItem: Number(this.itemSelecionadoId),
          quantidade: 1,
          precoUnitario: itemObj ? itemObj.preco : 0
        }
      ]
    };

    this.apiService.criarPedido(novoPedido).subscribe({
      next: () => {
        alert('Pedido realizado com sucesso!');
        this.clienteNome = '';
        this.observacao = '';
        this.itemSelecionadoId = null;
        this.carregarDados();
      },
      error: (err) => console.error('Erro ao salvar pedido:', err)
    });
  }

  cancelarPedido(id: number): void {
    if (confirm('Deseja realmente cancelar este pedido?')) {
      this.apiService.cancelarPedido(id).subscribe({
        next: () => {
          alert('Pedido cancelado!');
          this.carregarDados();
        },
        error: (err) => console.error('Erro ao cancelar:', err)
      });
    }
  }
}