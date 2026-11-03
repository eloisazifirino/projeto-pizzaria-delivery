import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
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

  readonly administrativo: boolean;

  constructor(private apiService: ApiService, route: ActivatedRoute) {
    this.administrativo = route.snapshot.data['administrativo'] === true;
    this.itemSelecionadoId = Number(route.snapshot.queryParamMap.get('item')) || null;
  }

  ngOnInit(): void {
    this.carregarDados();
  }

  carregarDados(): void {
    if (this.administrativo) {
      this.apiService.getPedidos().subscribe(data => this.pedidos = data);
    } else {
      this.apiService.getItens().subscribe(data => {
        this.cardapio = data.filter(item => item.ativoProdutoServico === 'S');
      });
    }
  }

  finalizarPedido(): void {
    if (!this.clienteNome.trim() || !this.itemSelecionadoId) {
      alert('Por favor, informe o nome do cliente e selecione um item!');
      return;
    }

    const novoPedido = {
      observacao: `Cliente: ${this.clienteNome}. Obs: ${this.observacao}`,
      atendimentoItens: [
        {
          idItem: Number(this.itemSelecionadoId),
          quantidade: 1
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
      error: (err) => {
        console.error('Erro ao salvar pedido:', err);
        alert('Não foi possível confirmar o pedido. Verifique se a API e o banco estão disponíveis.');
      }
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