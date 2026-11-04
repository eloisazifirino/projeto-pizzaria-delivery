import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

export interface Item {
  idItem: number;
  nome: string;
  descricaoItem: string | null;
  preco: number;
  idCategoria: number | null;
  tipoItem: 'P' | 'S';
  ativoProdutoServico: string;
  saldo: number;
  grupoCardapio: 'pizza' | 'bebidas' | 'outros';
  detalhes: string | null;
  ehPizza: boolean;
}

export interface Categoria { id: number; nome: string; grupoCardapio: 'pizza' | 'bebidas' | 'outros' | 'servicos'; }
export interface Cliente {
  id: number;
  nome: string;
  cpf: string;
  email: string;
  telefone: string | null;
}
export interface SessaoCliente { id: number; nome: string; token: string; temSenha?: boolean; }
export interface Pedido {
  idAtendimento: number;
  idPessoa: number | null;
  dataHora: string;
  status: string;
  observacao: string | null;
  valorTotal: number;
  modalidade: 'entrega' | 'retirada' | null;
  taxaEntrega: number;
  atendimentoItens: { idItem: number; quantidade: number; precoUnitario: number; fatias: number | null }[];
}
export interface Entrada {
  id: number;
  idItem: number;
  quantidade: number;
  data: string;
  valorUnitario: number;
}

export function mensagemErro(erro: unknown): string {
  if (erro instanceof HttpErrorResponse) {
    if (!erro.status) return 'Não foi possível conectar à API.';
    if (erro.status < 500 && typeof erro.error === 'string' && erro.error) return erro.error;
    if (erro.status === 401) return 'Acesso inválido. Confira a chave ou a sessão.';
    if (erro.status === 404) return 'Registro não encontrado.';
    if (erro.status === 400) return 'Confira os campos informados.';
  }
  return 'Não foi possível concluir a operação.';
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly url = 'http://localhost:5217/api';

  constructor(private http: HttpClient) {}

  get<T>(rota: string, chave = '', token = '') {
    return this.http.get<T>(this.url + rota, { headers: this.cabecalhos(chave, token) });
  }

  salvar<T>(metodo: 'POST' | 'PUT', rota: string, dados: object, chave = '', token = '') {
    return this.http.request<T>(metodo, this.url + rota, {
      body: dados, headers: this.cabecalhos(chave, token)
    });
  }

  excluir(rota: string, chave: string) {
    return this.http.delete(this.url + rota, { headers: this.cabecalhos(chave, '') });
  }

  private cabecalhos(chave: string, token: string): Record<string, string> {
    if (chave) return { 'X-Admin-Key': chave };
    if (token) return { 'X-Cliente-Token': token };
    return {};
  }
}
