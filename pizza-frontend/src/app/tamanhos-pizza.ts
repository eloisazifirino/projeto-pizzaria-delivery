export const tamanhosPizza = [
  { fatias: 8, nome: 'Família (8 fatias)' },
  { fatias: 6, nome: 'Média (6 fatias)' },
  { fatias: 4, nome: 'Pequena (4 fatias)' }
];

export function nomeTamanho(fatias: number | null): string {
  return tamanhosPizza.find(tamanho => tamanho.fatias === fatias)?.nome ?? '';
}
