export function correspondeAoNome(nome: string, busca: string): boolean {
  const normalizar = (texto: string) => texto.normalize('NFD').replace(/\p{M}/gu, '').toLowerCase();
  const nomeNormalizado = normalizar(nome);
  return normalizar(busca).trim().split(/\s+/).every(termo => nomeNormalizado.includes(termo));
}
