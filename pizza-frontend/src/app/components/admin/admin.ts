import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EMPTY, catchError, exhaustMap, filter, firstValueFrom, forkJoin, map, of, timer } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { nomeTamanho } from '../../tamanhos-pizza';
import { correspondeAoNome } from '../../busca';
import { ApiService, Categoria, Cliente, Entrada, Item, Pedido, mensagemErro } from '../../services/api';

type Aba = 'resumo' | 'produtos' | 'clientes' | 'pedidos';

@Component({
  selector: 'app-admin',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.html'
})
export class AdminComponent implements OnInit {
  readonly abas: { id: Aba; nome: string }[] = [
    { id: 'resumo', nome: 'Resumo' }, { id: 'produtos', nome: 'Produtos' },
    { id: 'clientes', nome: 'Clientes' }, { id: 'pedidos', nome: 'Pedidos' }
  ];
  readonly situacoes = ['Em Processamento', 'Sendo preparado', 'Saiu para entrega', 'Pronto para retirada', 'Concluído', 'Cancelado'];
  aba: Aba = 'resumo';
  chave = '';
  autorizado = false;
  mensagem = '';
  ocupado = false;
  itens: Item[] = [];
  categorias: Categoria[] = [];
  clientes: Cliente[] = [];
  pedidos: Pedido[] = [];
  entradas: Entrada[] = [];
  pedido?: Pedido;
  cliente?: Cliente;
  filtroStatus = '';
  ordemPedidos: 'recentes' | 'antigos' = 'recentes';

  get pedidosOrdenados() {
    return [...this.pedidos].sort((a, b) => {
      const diferenca = new Date(b.dataHora).getTime() - new Date(a.dataHora).getTime() ||
        b.idAtendimento - a.idAtendimento;
      return this.ordemPedidos === 'recentes' ? diferenca : -diferenca;
    });
  }
  buscaNome = '';
  filtroAtivo: '' | 'S' | 'N' = '';
  novoStatus = '';
  statusNaLista: Record<number, string> = {};
  consultaId: number | null = null;
  consultaCategoriaId: number | null = null;
  categoria = { id: 0, nome: '', grupoCardapio: 'outros' as Categoria['grupoCardapio'] };
  item = this.itemVazio();
  movimento = {
    idItem: null as number | null, nome: '', quantidade: 1, data: this.dataDeHoje(), valorUnitario: '',
    preco: '', idCategoria: null as number | null, grupoCardapio: 'outros', descricaoItem: '', detalhes: ''
  };
  operacao: 'entrada' | 'saida' = 'entrada';
  readonly nomeTamanho = nomeTamanho;
  readonly formatoValor = '[0-9]{1,8}([,.][0-9]{1,2})?';

  erroSincronizacao = '';

  constructor(private api: ApiService) {
    timer(5000, 5000).pipe(
      filter(() => this.autorizado && this.aba === 'pedidos' && !this.ocupado),
      exhaustMap(() => {
        const chave = this.chave;
        const filtro = this.filtroStatus;
        const id = this.pedido?.idAtendimento;
        const rota = '/atendimentos' + (filtro ? '?status=' + encodeURIComponent(filtro) : '');
        return forkJoin({
          pedidos: this.api.get<Pedido[]>(rota, chave),
          detalhe: id ? this.api.get<Pedido>('/atendimentos/' + id, chave) : of(undefined)
        }).pipe(
          map(resultado => ({ ...resultado, chave, filtro, id })),
          catchError(() => {
            this.erroSincronizacao = 'Não foi possível atualizar os pedidos. Tentaremos novamente.';
            return EMPTY;
          })
        );
      }),
      takeUntilDestroyed()
    ).subscribe(resultado => {
      if (!this.autorizado || this.aba !== 'pedidos' || this.ocupado ||
          this.chave !== resultado.chave || this.filtroStatus !== resultado.filtro) return;
      this.pedidos = resultado.pedidos;
      if (this.pedido?.idAtendimento === resultado.id) this.pedido = resultado.detalhe;
      this.erroSincronizacao = '';
    });
  }

  ngOnInit() {
    this.chave = sessionStorage.getItem('pizzaria-admin') ?? '';
    if (this.chave) this.entrar();
  }

  readonly tipo = 'P';
  get produtos() { return this.itens.filter(i => i.tipoItem === 'P'); }
  get itensFiltrados() {
    return this.itens.filter(i => i.tipoItem === this.tipo && correspondeAoNome(i.nome, this.buscaNome) &&
      (!this.filtroAtivo || i.ativoProdutoServico === this.filtroAtivo));
  }
  get clientesFiltrados() { return this.clientes.filter(c => correspondeAoNome(c.nome, this.buscaNome)); }
  get saldoTotal() { return this.produtos.reduce((total, item) => total + item.saldo, 0); }
  get produtoSelecionado() {
    return this.produtos.find(item => item.idItem === this.movimento.idItem);
  }
  get produtoComMesmoNome() {
    const nome = this.movimento.nome.trim();
    if (!nome) return undefined;
    const correspondencias = this.produtos.filter(item =>
      item.nome.trim().localeCompare(nome, 'pt-BR', { sensitivity: 'base' }) === 0);
    return correspondencias.length === 1 ? correspondencias[0] : undefined;
  }
  get emProcessamento() { return this.pedidos.filter(p => p.status === 'Em Processamento').length; }

  entrar() {
    void this.executar(async () => {
      await this.carregar();
      this.autorizado = true;
      sessionStorage.setItem('pizzaria-admin', this.chave);
    });
  }

  sair() {
    this.autorizado = false;
    this.chave = '';
    sessionStorage.removeItem('pizzaria-admin');
  }

  abrir(aba: Aba) {
    this.aba = aba;
    this.consultaId = null;
    this.filtroStatus = '';
    this.buscaNome = '';
    this.pedido = undefined;
    this.cliente = undefined;
    this.categoria = { id: 0, nome: '', grupoCardapio: 'outros' as Categoria['grupoCardapio'] };
    this.item = this.itemVazio();
    this.entrar();
  }

  consultar(id: number) {
    if (!Number.isInteger(id) || id <= 0) return;
    void this.executar(async () => {
      switch (this.aba) {
        case 'produtos': {
          const item = await this.get<Item>('/itens/' + id);
          if (item.tipoItem !== this.tipo) {
            this.mensagem = 'O item pertence a outro tipo.';
            return;
          }
          this.item = { ...item, preco: String(item.preco).replace('.', ','), descricaoItem: item.descricaoItem ?? '', detalhes: item.detalhes ?? '' };
          break;
        }
        case 'clientes': this.cliente = await this.get<Cliente>('/pessoas/' + id); break;
        case 'pedidos':
          this.pedido = await this.get<Pedido>('/atendimentos/' + id);
          this.novoStatus = '';
          break;
      }
    });
  }

  limparItem() { this.item = this.itemVazio(); }
  limparCategoria() { this.categoria = { id: 0, nome: '', grupoCardapio: 'outros' as Categoria['grupoCardapio'] }; }

  salvarItem() {
    const { idItem, ...dados } = this.item;
    this.salvar('/itens', idItem, { ...dados, preco: this.valor(dados.preco) }, () => this.limparItem());
  }

  salvarCategoria() {
    this.salvar('/categorias', this.categoria.id, { nome: this.categoria.nome, grupoCardapio: this.categoria.grupoCardapio }, () => this.limparCategoria());
  }

  editarCategoria(id: number) {
    void this.executar(async () => { this.categoria = await this.get<Categoria>('/categorias/' + id); });
  }

  selecionarMovimento(id: number, operacao: 'entrada' | 'saida') {
    this.movimento.idItem = id;
    this.operacao = operacao;
    document.getElementById('operacao-estoque')?.focus();
  }

  registrarMovimento() {
    if (!Number.isInteger(this.movimento.quantidade) || this.movimento.quantidade < 1) {
      this.mensagem = 'Informe uma quantidade inteira maior que zero.';
      return;
    }
    void this.executar(async () => {
      const { idItem, quantidade, data, preco, valorUnitario, ...produto } = this.movimento;
      if (this.operacao === 'saida') {
        await firstValueFrom(this.api.salvar('POST', '/estoque/saidas', { idItem, quantidade, data }, this.chave));
        this.mensagem = 'Retirada registrada.';
      } else {
        const existente = idItem ?? this.produtoComMesmoNome?.idItem ?? null;
        const resultado = await firstValueFrom(this.api.salvar<Entrada & { saldo: number; novoProduto: boolean }>(
          'POST', '/estoque/entradas', {
            ...produto, idItem: existente, quantidade, data, preco: preco ? this.valor(preco) : null,
            valorUnitario: existente ? null : this.valor(valorUnitario)
          }, this.chave));
        const cadastrado = this.itens.find(item => item.idItem === resultado.idItem);
        if (cadastrado) cadastrado.saldo = resultado.saldo;
        this.entradas = [resultado, ...this.entradas];
        this.movimento.idItem = resultado.idItem;
        this.movimento.nome = '';
        const confirmacao = resultado.novoProduto ? 'Produto cadastrado e entrada registrada.' : 'Entrada registrada.';
        this.mensagem = confirmacao + ' Saldo: ' + resultado.saldo + ' un.';
      }
      this.movimento.quantidade = 1;
      try { await this.carregarEstoque(); }
      catch { this.mensagem += ' Use Atualizar saldos para recarregar a lista.'; }
    });
  }

  atualizar() { void this.executar(() => this.carregarEstoque()); }

  private async carregarEstoque() {
    [this.itens, this.entradas] = await Promise.all([
      this.get<Item[]>('/itens'), this.get<Entrada[]>('/estoque/entradas')
    ]);
  }

  private dataDeHoje() {
    const agora = new Date();
    const local = new Date(agora.getTime() - agora.getTimezoneOffset() * 60000);
    return local.toISOString().slice(0, 10);
  }

  private valor(texto: string) { return Number(texto.replace(',', '.')); }

  filtrarPedidos(status: string) {
    this.filtroStatus = status;
    this.pedido = undefined;
    this.novoStatus = '';
    this.statusNaLista = {};
    void this.executar(() => this.carregar());
  }

  atualizarStatus(pedido = this.pedido, status = this.novoStatus) {
    if (!pedido || !status) return;
    const rota = '/atendimentos/' + pedido.idAtendimento;
    void this.executar(async () => {
      await firstValueFrom(this.api.salvar('PUT', rota + '/status', { status }, this.chave));
      if (this.pedido?.idAtendimento === pedido.idAtendimento) this.pedido = await this.get<Pedido>(rota);
      delete this.statusNaLista[pedido.idAtendimento];
      this.novoStatus = '';
      this.mensagem = 'Situação atualizada.';
      await this.carregar();
    });
  }

  opcoesStatus(pedido: Pedido): string[] {
    if (pedido.status === 'Cancelado' || pedido.status === 'Concluído') return [];
    return this.situacoes.filter(status => status !== pedido.status &&
      !(status === 'Saiu para entrega' && pedido.modalidade === 'retirada') &&
      !(status === 'Pronto para retirada' && pedido.modalidade === 'entrega'));
  }


  reativar(id: number) {
    void this.executar(async () => {
      await firstValueFrom(this.api.salvar('PUT', '/itens/' + id + '/reativar', {}, this.chave));
      const produto = this.itens.find(item => item.idItem === id);
      if (produto) produto.ativoProdutoServico = 'S';
      this.mensagem = 'Produto reativado.';
    });
  }

  excluir(rota: string, id: number) {
    if (!confirm(rota === '/itens' ? 'Desativar este item?' : 'Excluir esta categoria?')) return;
    void this.executar(async () => {
      await firstValueFrom(this.api.excluir(rota + '/' + id, this.chave));
      this.limparItem();
      this.limparCategoria();
      this.mensagem = rota === '/itens' ? 'Item desativado.' : 'Categoria excluída.';
      await this.carregar();
    });
  }

  nomeItem(id: number) { return this.itens.find(i => i.idItem === id)?.nome ?? 'Item indisponível'; }
  categoriasDoGrupo(grupo: string) {
    return this.categorias.filter(c => c.grupoCardapio === grupo);
  }

  nomeGrupo(grupo: string) {
    return ({ pizza: 'Pizzas', bebidas: 'Bebidas', outros: 'Outros produtos', servicos: 'Serviços' } as Record<string, string>)[grupo] ?? grupo;
  }

  nomeCategoria(id: number | null) { return this.categorias.find(c => c.id === id)?.nome ?? 'Sem categoria'; }
  nomeCliente(id: number | null) { return this.clientes.find(c => c.id === id)?.nome ?? 'Sem cadastro vinculado'; }

  private itemVazio() {
    return { idItem: 0, nome: '', descricaoItem: '', preco: '', idCategoria: null as number | null, tipoItem: this.tipo,
      grupoCardapio: 'outros' as Item['grupoCardapio'], detalhes: '' };
  }

  private salvar(rota: string, id: number, dados: object, limpar: () => void) {
    void this.executar(async () => {
      await firstValueFrom(this.api.salvar(id ? 'PUT' : 'POST', rota + (id ? '/' + id : ''), dados, this.chave));
      limpar();
      this.mensagem = 'Registro salvo.';
      await this.carregar();
    });
  }

  private get<T>(rota: string) { return firstValueFrom(this.api.get<T>(rota, this.chave)); }

  private async carregar() {
    const status = this.filtroStatus ? '?status=' + encodeURIComponent(this.filtroStatus) : '';
    [this.itens, this.categorias, this.clientes, this.pedidos, this.entradas] = await Promise.all([
      this.get<Item[]>('/itens'), this.get<Categoria[]>('/categorias').then(categorias => categorias.filter(c => c.grupoCardapio !== 'servicos')), this.get<Cliente[]>('/pessoas'),
      this.get<Pedido[]>('/atendimentos' + status), this.get<Entrada[]>('/estoque/entradas')
    ]);
  }

  private async executar(acao: () => Promise<void>) {
    if (this.ocupado) return;
    this.ocupado = true;
    this.mensagem = '';
    try { await acao(); }
    catch (erro) { this.mensagem = mensagemErro(erro); }
    finally { this.ocupado = false; }
  }
}
