CREATE DATABASE IF NOT EXISTS pizzaria_db;
USE pizzaria_db;

-- 1. Categorias (Pizzas Salgadas, Doce, Bebidas)
CREATE TABLE categoria (
    idcategoria INT AUTO_INCREMENT PRIMARY KEY,
    categoria VARCHAR(45) NOT NULL
);

-- 2. Menu (Pizzas e Bebidas)
CREATE TABLE item (
    iditem INT AUTO_INCREMENT PRIMARY KEY,
    idcategoria INT,
    nome VARCHAR(100) NOT NULL,
    descricao_item VARCHAR(200),
    preco DECIMAL(10,2) NOT NULL,
    tipo_item CHAR(1) DEFAULT 'P',
    ativo_produto_servico CHAR(1) DEFAULT 'S',
    FOREIGN KEY (idcategoria) REFERENCES categoria(idcategoria)
);

-- 3. Clientes e Funcionários
CREATE TABLE pessoa (
    idpessoa INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    cpf VARCHAR(11) UNIQUE,
    telefone CHAR(11),
    tipo_pessoa CHAR(1) DEFAULT 'C'
);

-- 4. Pedidos
CREATE TABLE atendimento (
    idatendimento INT AUTO_INCREMENT PRIMARY KEY,
    idpessoa INT,
    data_atendimento DATETIME DEFAULT CURRENT_TIMESTAMP,
    status_atendimento VARCHAR(20) DEFAULT 'Em Processamento',
    FOREIGN KEY (idpessoa) REFERENCES pessoa(idpessoa)
);

-- 5. Itens do Pedido (Carrinho)
CREATE TABLE atendimentoitem (
    idatendimentoitem INT AUTO_INCREMENT PRIMARY KEY,
    idatendimento INT,
    iditem INT,
    quantidade_itens INT NOT NULL,
    valor_unitario DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (idatendimento) REFERENCES atendimento(idatendimento),
    FOREIGN KEY (iditem) REFERENCES item(iditem)
);