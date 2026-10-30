export interface AtendimentoItem {
  idAtendimentoItem?: number;
  idAtendimento?: number;
  idItem: number;
  quantidade: number;
  precoUnitario: number;
}

export interface Atendimento {
  idAtendimento?: number;
  dataHora?: string;
  valorTotal: number;
  status?: string;
  observacao?: string;
  atendimentoItens: AtendimentoItem[];
}