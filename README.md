# Pizzaria Delivery

Aplicação acadêmica com loja virtual e painel administrativo. O cliente escolhe pizzas, bebidas e acompanhamentos, monta o carrinho, escolhe entrega ou retirada e acompanha os pedidos. O administrador gerencia produtos, categorias, estoque, clientes e situações dos pedidos.

O frontend usa Angular 21 e TypeScript. A API usa ASP.NET Core 8, Entity Framework Core e MySQL. A comunicação acontece por HTTP e JSON.

## Preparar uma máquina nova

Instale as ferramentas abaixo antes da apresentação.

- .NET SDK 8.
- Node.js 22.12 ou superior da linha 22, com npm. O Angular instalado também aceita Node 20.19 ou superior da linha 20.
- MySQL 8 em execução.
- Um editor SQL, como MySQL Workbench, ou o cliente de terminal do MySQL.
- Acesso à internet na primeira instalação para restaurar os pacotes NuGet e npm.

Confira as versões no terminal.

```bash
dotnet --version
node --version
npm --version
```

Copie o código-fonte completo, incluindo `package-lock.json`, os arquivos de configuração e a pasta `pizzaria_db`. Dependências e compilações são geradas novamente nos passos abaixo. As pastas `node_modules`, `bin`, `obj`, `dist` e `.angular` ficam fora do controle de versão.

## Criar o banco

Para uma instalação nova, abra e execute **todo o arquivo** [pizzaria_db/pizzaria_db.sql](pizzaria_db/pizzaria_db.sql) no MySQL Workbench.

Esse único arquivo cria as tabelas finais e inclui três categorias e quatro produtos com estoque inicial. O cardápio de demonstração tem duas pizzas, um refrigerante e um acompanhamento. A criação de contas de clientes é feita pela loja.

No Bash também é possível importar pela raiz do projeto.

```bash
mysql -u root -p < pizzaria_db/pizzaria_db.sql
```

O script de instalação foi feito para um banco novo. Ele não substitui tabelas existentes. A pasta [pizzaria_db/migracoes](pizzaria_db/migracoes) conserva o histórico de atualização do banco usado durante o desenvolvimento. Quem instalar pelo script consolidado já terá essas alterações e deve seguir diretamente para a configuração da API.

Para levar os dados reais que já estão na máquina atual, exporte o banco completo pelo MySQL Workbench e importe esse arquivo na máquina de apresentação, em vez de usar o cardápio de demonstração.

Se estiver atualizando um banco antigo, faça uma cópia de segurança e aplique somente as migrações que ainda faltam, na ordem abaixo. Executar novamente comandos que adicionam colunas causa erro de coluna duplicada.

1. `adicionar_observacao_pedido.sql`
2. `atividade04.sql`
3. `loja_contas_e_cardapio.sql`
4. `tamanhos_pizza.sql`
5. `entrega_retirada.sql`
6. `bebidas_categorias.sql`
7. `substituir_status_aberto.sql`

O arquivo `migracoes/00_base_anterior.sql` é a base histórica anterior às migrações. Para a apresentação em uma instalação nova, use o script consolidado da pasta principal.

## Configurar a API

Em [PizzariaApi/appsettings.json](PizzariaApi/appsettings.json), ajuste `ConnectionStrings.DefaultConnection` para o usuário, senha e porta do MySQL dessa máquina. A configuração inicial aponta para `127.0.0.1:3306`, banco `pizzaria_db`, usuário `root` e senha `root`.

Exemplo de formato da conexão.

```text
Server=127.0.0.1;Port=3306;Database=pizzaria_db;User=root;Password=SUA_SENHA;
```

A AdminKey é a chave escolhida para acessar o painel administrativo. Ela é definida ao iniciar a API e é independente da senha do MySQL. Use o mesmo valor ao preencher o campo Chave administrativa no navegador.

Na raiz do projeto, restaure e inicie a API em um terminal.

**Linux ou macOS, com Bash**

```bash
dotnet restore PizzariaApi/PizzariaApi.csproj
export AdminKey='PizzaApresentacao123'
dotnet run --project PizzariaApi/PizzariaApi.csproj --launch-profile http
```

**Windows, com PowerShell**

```powershell
dotnet restore PizzariaApi/PizzariaApi.csproj
$env:AdminKey = 'PizzaApresentacao123'
dotnet run --project PizzariaApi/PizzariaApi.csproj --launch-profile http
```

Deixe esse terminal aberto. A API deve informar que está escutando em `http://localhost:5217`. O Swagger fica em `http://localhost:5217/swagger`.

Se o terminal já estiver dentro de `PizzariaApi`, use `dotnet run --launch-profile http` depois de definir a variável AdminKey.

## Iniciar o Angular

Em outro terminal, a partir da raiz do projeto, execute os comandos abaixo.

```bash
cd pizza-frontend
npm ci
npm start
```

Abra `http://localhost:4200`. A API precisa continuar rodando no outro terminal e o MySQL deve estar ativo.

O endereço da API usado pelo frontend está em [pizza-frontend/src/app/services/api.ts](pizza-frontend/src/app/services/api.ts). Na configuração de apresentação, as portas são 4200 para o Angular e 5217 para a API.

Nas próximas execuções, basta iniciar MySQL, API e Angular. A restauração das dependências é necessária na primeira instalação ou quando elas mudarem.

## Usar a loja

- **Cardápio** mostra Todos, Pizzas, Bebidas e Outros produtos. Pesquise partes do nome sem diferenciar maiúsculas ou acentos. Cada aba tem suas próprias categorias.
- Cada card mostra a descrição curta. **Ver detalhes** abre a descrição completa em uma janela.
- Nas pizzas, escolha Família com 8 fatias, Média com 6 ou Pequena com 4. Os tamanhos compartilham o saldo e o preço cadastrado da pizza.
- O botão **+** adiciona uma unidade ao carrinho, e **−** remove uma unidade do tamanho selecionado. A quantidade também pode ser editada na aba Carrinho.
- Em **Minha conta**, crie uma conta com nome, CPF de 11 dígitos, e-mail, telefone opcional e senha de pelo menos 8 caracteres. CPF e e-mail são únicos. A senha é armazenada como hash.
- O perfil aparece no topo. Em Minha conta é possível editar nome, CPF, e-mail e telefone. O formulário de edição não altera a senha.
- No carrinho, escolha entrega ou retirada. Entrega acrescenta R$ 5,00, calculados também na API. Retirada tem frete zero.
- **Confirmar pedido** grava o pedido e baixa o estoque em uma transação. O preço registrado nos itens é o preço vigente no momento da confirmação.
- **Meus pedidos** mostra apenas os pedidos da conta conectada, com ordenação por data e atualização automática a cada 5 segundos.
- O cliente pode cancelar somente enquanto o pedido estiver em **Em Processamento**. O cancelamento devolve as unidades ao estoque uma única vez.

O login mantém uma sessão no navegador. Entrar novamente na mesma conta gera uma nova sessão e invalida o token anterior. Não há recuperação de senha pela aplicação. Contas antigas sem senha podem defini-la se ainda tiverem sua sessão válida.

## Usar o painel administrativo

Abra **Painel administrativo** e informe a AdminKey definida no terminal da API.

- **Resumo** apresenta quantidades de clientes, produtos, pedidos e unidades em estoque.
- **Produtos** reúne pesquisa por nome, filtros Todos/Ativos/Desativados, cadastro, edição, categorias e movimentações.
- Para cadastrar com estoque inicial, selecione Dar entrada, informe o nome e os campos do produto novo. Se já houver um produto com esse nome, selecione-o ou use o reconhecimento por nome para somar ao saldo existente.
- Nas reposições de um produto existente, o valor unitário usado é seu preço cadastrado. Essa é a regra escolhida para esta versão. Ela não separa custo de aquisição e preço de venda. Alterações de preço são feitas em Editar.
- Para diminuir o saldo manualmente, escolha Retirar unidades, selecione o produto e informe data e quantidade. A API impede saldo negativo. O histórico registra a retirada com quantidade negativa.
- **Gerenciar categorias**, na mesma aba, permite consultar por ID, cadastrar, editar e excluir. Uma categoria vinculada a produtos não pode ser excluída nem transferida para outra aba.
- Produtos podem ser desativados e reativados. Os desativados deixam de aparecer no cardápio, preservando os pedidos já registrados.
- **Clientes** permite pesquisar por nome, listar e consultar por ID.
- **Pedidos** oferece abas separadas por status, ordenação por data e alteração de status diretamente na lista ou nos detalhes.
- Os status são Em Processamento, Sendo preparado, Saiu para entrega, Pronto para retirada, Concluído e Cancelado. Novos pedidos começam em Em Processamento.
- Saiu para entrega é usado em pedidos de entrega. Pronto para retirada é usado em pedidos de retirada. Pedidos antigos sem modalidade aceitam as duas opções.
- Pedidos cancelados e concluídos são finais. O administrador pode cancelar pedidos ativos em outras etapas, enquanto o cancelamento pelo cliente fica restrito a Em Processamento.
- A aba Pedidos do administrador também atualiza automaticamente a cada 5 segundos.

## Conferência do item 7 da atividade

A tabela abaixo descreve a implementação atual e as adaptações solicitadas durante o desenvolvimento. Serviços foram retirados do escopo da pizzaria a pedido da autora. Por isso, a versão final não atende literalmente ao RF11 e às referências a serviços em outros requisitos.

| Requisito | Implementação e observações |
| --- | --- |
| RF01 | Catálogo de pizzas, bebidas e outros produtos ativos. Parte de serviços retirada do escopo. |
| RF02 | Consulta de item por ID na API e janela Ver detalhes. |
| RF03 | Pesquisa parcial por nome, filtro por categoria e abas por tipo de produto. Filtro de serviços retirado. |
| RF04 | Cadastro de clientes com validação, CPF/e-mail únicos e senha. |
| RF05 | Adição de produtos ao carrinho, incluindo pizzas por tamanho. Serviços retirados. |
| RF06 | Quantidade, remoção, subtotal e total do carrinho. |
| RF07 | Confirmação persiste pedido e itens no MySQL. |
| RF08 | Histórico e detalhes restritos ao cliente conectado. |
| RF09 | Administrador lista clientes e consulta por ID. |
| RF10 | Cadastro, listagem, consulta por ID, edição e desativação de produtos. Inclui reativação. |
| RF11 | Não implementado na versão final. A gestão de serviços foi removida por solicitação da autora. |
| RF12 | Categorias por aba, com listagem, consulta por ID, cadastro, edição e exclusão protegida por vínculo. |
| RF13 | Administrador lista pedidos e consulta detalhes por ID. |
| RF14 | Abas filtram os status. Em Processamento substitui Aberto, conforme adaptação solicitada. |
| RF15 | Atualização de status validada na API e refletida nas duas áreas. |
| RF16 | Entradas registram produto, quantidade, data e valor unitário. Em reposições, usa o preço cadastrado conforme regra solicitada. |
| RF17 | Entrada atualiza saldo e histórico na mesma transação. |
| RF18 | Listagem de produtos com saldo, consulta por ID e endpoints de estoque. |
| RF19 | Pedido verifica disponibilidade e baixa saldo de forma atômica. Cancelamento estorna o saldo. |
| RF20 | Mensagens de sucesso e erro. Adição ao carrinho é indicada pelo contador, conforme solicitação. |
| RF21 | Formulários validam os dados e a API valida os campos e regras de negócio. |
| RF22 | Tratamento de erro HTTP, indisponibilidade, ausência de registros e conflitos. |
| RF23 | Navegação entre loja e painel administrativo. |
| RF24 | Resumo de clientes, produtos, pedidos e estoque. Contagem de serviços retirada do escopo. |

Funcionários são mencionados em outras partes do PDF, mas não há cadastro de funcionários nesta versão. O acesso administrativo usa uma chave única definida na execução. A entrega registra modalidade e frete, mas não há formulário de endereço nesta versão.

## Verificar antes da apresentação

1. Importe o banco novo e confira os produtos iniciais.
2. Inicie a API e o Angular.
3. Cadastre uma conta pela loja.
4. Adicione uma pizza de cada tamanho e uma bebida.
5. Escolha entrega e confira o acréscimo de R$ 5,00.
6. Confirme e consulte o pedido em Meus pedidos.
7. Em outra janela, abra o painel, confira o saldo e altere o pedido para Sendo preparado.
8. Aguarde a atualização no cliente e confira que o botão Cancelar pedido desaparece.
9. Faça outro pedido e cancele enquanto estiver em Em Processamento. Confira o estorno do estoque no painel.
10. Demonstre entrada, retirada manual, filtros de produtos, desativação, reativação e as abas de status.

Para conferir a compilação e os testes locais, execute a partir da raiz.

```bash
dotnet build PizzariaApi/PizzariaApi.csproj
cd pizza-frontend
npm run build
npm test -- --watch=false
```

## Problemas frequentes

- **npm não encontra package.json**. Entre na pasta `pizza-frontend` antes de executar `npm start`.
- **dotnet não encontra o projeto**. Na raiz, use `--project PizzariaApi/PizzariaApi.csproj`. Dentro de PizzariaApi, use apenas `dotnet run`.
- **Coluna desconhecida no MySQL**. O banco está em uma versão anterior ao código. Use o script consolidado na instalação nova ou confira as migrações pendentes no banco existente.
- **Painel não autentica**. A variável AdminKey precisa estar definida no terminal que iniciou a API, e a chave digitada deve ser igual. Confira também o log da API, pois um erro de banco pode impedir o carregamento do painel.
- **Cardápio não carrega**. Confira se API e MySQL estão rodando, a conexão do banco e a porta 5217.
- **Não é possível adicionar**. Confira se o produto está ativo, tem saldo e, no caso de pizza, se o tamanho foi escolhido.
- **Alteração de código não aparece**. Reinicie a API depois de alterações no backend e recarregue o Angular.
