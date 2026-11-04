import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EMPTY, catchError, exhaustMap, filter, firstValueFrom, map, timer } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { nomeTamanho, tamanhosPizza } from '../../tamanhos-pizza';
import { correspondeAoNome } from '../../busca';
import { ApiService, Categoria, Cliente, Item, Pedido, SessaoCliente, mensagemErro } from '../../services/api';

@Component({
  selector: 'app-loja',
  imports: [CommonModule, FormsModule],
  templateUrl: './loja.html'
})
export class LojaComponent implements OnInit {
  aba = 'catalogo';
  itens: Item[] = [];
  categorias: Categoria[] = [];
  pedidos: Pedido[] = [];
  ordemPedidos: 'recentes' | 'antigos' = 'recentes';

  get pedidosOrdenados(): Pedido[] {
    return [...this.pedidos].sort((a, b) => {
      const porData = new Date(b.dataHora).getTime() - new Date(a.dataHora).getTime();
      if (this.ordemPedidos === 'antigos') return -porData || a.idAtendimento - b.idAtendimento;
      return porData || b.idAtendimento - a.idAtendimento;
    });
  }
  detalhe?: Item;
  nome = '';
  categoria: number | null = null;
  grupo: 'todos' | 'pizza' | 'bebidas' | 'outros' = 'todos';
  modoConta: 'entrar' | 'cadastrar' = 'entrar';
  credenciais = { email: '', senha: '' };
  novaSenha = '';
  perfil?: Cliente;
  @ViewChild('janelaDetalhes') janelaDetalhes!: ElementRef<HTMLDialogElement>;
  observacao = '';
  modalidade: 'entrega' | 'retirada' = 'retirada';
  get taxaEntrega() { return this.carrinho.length && this.modalidade === 'entrega' ? 5 : 0; }
  mensagem = '';
  ocupado = false;
  cadastro = { nome: '', cpf: '', email: '', telefone: '', senha: '' };
  sessao: SessaoCliente | null = this.lerSessao();
  readonly tamanhos = tamanhosPizza;
  readonly nomeTamanho = nomeTamanho;
  tamanhosSelecionados: Record<number, number | undefined> = {};
  carrinho: { item: Item; quantidade: number; fatias: number | null }[] = [];

  erroSincronizacao = '';

  constructor(private api: ApiService) {
    timer(5000, 5000).pipe(
      filter(() => this.aba === 'pedidos' && !!this.sessao && !this.ocupado),
      exhaustMap(() => {
        const token = this.sessao!.token;
        return this.api.get<Pedido[]>('/pessoas/meus-pedidos', '', token).pipe(
          map(pedidos => ({ pedidos, token })),
          catchError(() => {
            this.erroSincronizacao = 'Não foi possível atualizar os pedidos. Tentaremos novamente.';
            return EMPTY;
          })
        );
      }),
      takeUntilDestroyed()
    ).subscribe(({ pedidos, token }) => {
      if (this.sessao?.token === token && this.aba === 'pedidos') {
        this.pedidos = pedidos;
        this.erroSincronizacao = '';
      }
    });
  }

  ngOnInit() { void this.executar(() => this.carregarCatalogo()); }

  get filtrados() {
    return this.itens.filter(i => i.ativoProdutoServico === 'S' &&
      (i.tipoItem === 'P' && (this.grupo === 'todos' ||
        (this.grupo === 'pizza' ? this.ehPizza(i) : !this.ehPizza(i) && i.grupoCardapio === this.grupo))) &&
      correspondeAoNome(i.nome, this.nome) &&
      (this.categoria === null || i.idCategoria === this.categoria));
  }

  get categoriasDaAba() {
    return this.categorias.filter(c => c.grupoCardapio !== 'servicos' && (this.grupo === 'todos' || c.grupoCardapio === this.grupo));
  }

  get subtotal() {
    return this.carrinho.reduce((soma, linha) => soma + linha.item.preco * linha.quantidade, 0);
  }

  get carrinhoValido() {
    return this.carrinho.length > 0 && this.carrinho.every(({ item, quantidade }) =>
      Number.isInteger(quantidade) && quantidade > 0 && this.quantidadeNoCarrinho(item) <= this.limite(item));
  }

  limite(item: Item) { return item.tipoItem === 'P' ? item.saldo : 1000000; }

  ehPizza(item: Item) { return item.ehPizza; }

  quantidadeNoCarrinho(item: Item) {
    return this.carrinho.filter(linha => linha.item.idItem === item.idItem)
      .reduce((total, linha) => total + linha.quantidade, 0);
  }

  adicionar(item: Item) {
    const fatias = this.ehPizza(item) ? this.tamanhosSelecionados[item.idItem] ?? null : null;
    if (this.ehPizza(item) && !fatias) {
      this.mensagem = 'Escolha o tamanho da pizza.';
      return;
    }
    const linha = this.carrinho.find(l => l.item.idItem === item.idItem && l.fatias === fatias);
    if (this.quantidadeNoCarrinho(item) >= this.limite(item)) {
      this.mensagem = 'Quantidade indisponível.';
      return;
    }
    if (linha) linha.quantidade++;
    else this.carrinho.push({ item, quantidade: 1, fatias });
    this.mensagem = '';
  }

  linhaSelecionada(item: Item) {
    const fatias = this.ehPizza(item) ? this.tamanhosSelecionados[item.idItem] : null;
    return this.carrinho.find(linha => linha.item.idItem === item.idItem && linha.fatias === fatias);
  }

  diminuir(item: Item) {
    const linha = this.linhaSelecionada(item);
    if (!linha) return;
    if (linha.quantidade <= 1) this.remover(linha);
    else linha.quantidade--;
  }

  remover(linha: (typeof this.carrinho)[number]) { this.carrinho = this.carrinho.filter(l => l !== linha); }

  abrir(aba: string) {
    this.aba = aba;
    this.mensagem = '';
    if (aba === 'catalogo') this.atualizar();
    if (aba === 'cadastro' && this.sessao) this.carregarPerfil();
    if (aba === 'pedidos' && this.sessao) void this.executar(async () => {
      await this.carregarPedidos();
    });
  }

  mudarGrupo(grupo: typeof this.grupo) {
    this.grupo = grupo;
    this.categoria = null;
  }

  consultar(id: number) {
    void this.executar(async () => {
      this.detalhe = await firstValueFrom(this.api.get<Item>('/itens/' + id));
      this.janelaDetalhes.nativeElement.showModal();
    });
  }

  cadastrar() {
    void this.executar(async () => {
      this.guardarSessao(await firstValueFrom(this.api.salvar<SessaoCliente>('POST', '/pessoas', {
        ...this.cadastro, telefone: this.cadastro.telefone || null
      })));
      this.cadastro.senha = '';
      this.aba = 'carrinho';
      this.mensagem = 'Cadastro realizado.';
    });
  }

  entrar() {
    void this.executar(async () => {
      this.guardarSessao(await firstValueFrom(this.api.salvar<SessaoCliente>('POST', '/pessoas/entrar', this.credenciais)));
      this.credenciais.senha = '';
      this.aba = this.carrinho.length ? 'carrinho' : 'catalogo';
      this.mensagem = 'Você entrou na sua conta.';
    });
  }

  private carregarPerfil() {
    const token = this.sessao?.token;
    this.perfil = undefined;
    void this.executar(async () => {
      this.perfil = await firstValueFrom(this.api.get<Cliente>('/pessoas/perfil', '', token));
    });
  }

  salvarPerfil() {
    if (!this.sessao || !this.perfil) return;
    const sessao = this.sessao;
    const { nome, cpf, email, telefone } = this.perfil;
    void this.executar(async () => {
      this.perfil = await firstValueFrom(this.api.salvar<Cliente>(
        'PUT', '/pessoas/perfil', { nome, cpf, email, telefone: telefone || null }, '', sessao.token));
      this.guardarSessao({ ...sessao, nome: this.perfil.nome });
      this.mensagem = 'Dados atualizados.';
    });
  }

  definirSenha() {
    if (!this.sessao) return;
    const token = this.sessao.token;
    void this.executar(async () => {
      this.guardarSessao(await firstValueFrom(this.api.salvar<SessaoCliente>(
        'POST', '/pessoas/definir-senha', { senha: this.novaSenha }, '', token)));
      this.novaSenha = '';
      this.mensagem = 'Senha definida. Agora você pode entrar com seu e-mail.';
    });
  }

  sair() {
    const token = this.sessao?.token;
    void this.executar(async () => {
      await firstValueFrom(this.api.salvar('POST', '/pessoas/sair', {}, '', token));
      localStorage.removeItem('pizzaria-cliente');
      this.sessao = null;
      this.pedidos = [];
      this.perfil = undefined;
      this.cadastro = { nome: '', cpf: '', email: '', telefone: '', senha: '' };
      this.aba = 'catalogo';
    });
  }

  private guardarSessao(sessao: SessaoCliente) {
    this.sessao = sessao;
    localStorage.setItem('pizzaria-cliente', JSON.stringify(sessao));
  }

  confirmar() {
    if (!this.sessao?.temSenha || !this.carrinhoValido) return;
    void this.executar(async () => {
      const pedido = await firstValueFrom(this.api.salvar<Pedido>('POST', '/atendimentos', {
        observacao: this.observacao, modalidade: this.modalidade,
        atendimentoItens: this.carrinho.map(({ item, quantidade, fatias }) => ({ idItem: item.idItem, quantidade, fatias }))
      }, '', this.sessao!.token));
      this.carrinho = [];
      this.observacao = '';
      for (const linha of pedido.atendimentoItens) {
        const item = this.itens.find(i => i.idItem === linha.idItem);
        if (item?.tipoItem === 'P') item.saldo = Math.max(0, item.saldo - linha.quantidade);
      }
      this.pedidos = [pedido, ...this.pedidos];
      this.aba = 'pedidos';
      this.mensagem = 'Pedido #' + pedido.idAtendimento + ' confirmado.';
      try { await this.carregarPedidos(); }
      catch { this.mensagem += ' Abra Meus pedidos novamente para atualizar o histórico.'; }
    });
  }

  podeCancelar(pedido: Pedido) {
    return pedido.status === 'Em Processamento';
  }

  cancelarPedido(pedido: Pedido) {
    if (!this.sessao || !this.podeCancelar(pedido) || !confirm('Cancelar o pedido #' + pedido.idAtendimento + '?')) return;
    const token = this.sessao.token;
    void this.executar(async () => {
      await firstValueFrom(this.api.salvar('POST', '/atendimentos/' + pedido.idAtendimento + '/cancelar', {}, '', token));
      pedido.status = 'Cancelado';
      await this.carregarPedidos();
      this.mensagem = 'Pedido cancelado.';
    });
  }

  nomeItem(id: number) { return this.itens.find(i => i.idItem === id)?.nome ?? 'Item indisponível'; }

  private async carregarPedidos() {
    if (!this.sessao) return;
    this.pedidos = await firstValueFrom(this.api.get<Pedido[]>('/pessoas/meus-pedidos', '', this.sessao.token));
  }

  private async carregarCatalogo() {
    await Promise.all([
      firstValueFrom(this.api.get<Item[]>('/itens')).then(itens => { this.itens = itens; }),
      firstValueFrom(this.api.get<Categoria[]>('/categorias')).then(categorias => { this.categorias = categorias; })
    ]);
    for (const linha of this.carrinho) {
      const atual = this.itens.find(i => i.idItem === linha.item.idItem);
      if (atual) linha.item = atual;
    }
  }

  atualizar() { void this.executar(() => this.carregarCatalogo()); }

  private async executar(acao: () => Promise<void>) {
    if (this.ocupado) return;
    this.ocupado = true;
    this.mensagem = '';
    try { await acao(); }
    catch (erro) { this.mensagem = mensagemErro(erro); }
    finally { this.ocupado = false; }
  }

  private lerSessao(): SessaoCliente | null {
    try { return JSON.parse(localStorage.getItem('pizzaria-cliente') ?? 'null'); }
    catch { return null; }
  }
}
