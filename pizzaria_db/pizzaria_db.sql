-- Instalação nova. Execute este arquivo completo uma única vez.
-- Para um banco já existente, veja a seção de migrações no README.
CREATE DATABASE IF NOT EXISTS pizzaria_db
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE pizzaria_db;

CREATE TABLE categoria (
    idcategoria INT AUTO_INCREMENT PRIMARY KEY,
    categoria VARCHAR(45) NOT NULL,
    grupo_cardapio VARCHAR(10) NOT NULL DEFAULT 'outros'
) ENGINE=InnoDB;

CREATE TABLE item (
    iditem INT AUTO_INCREMENT PRIMARY KEY,
    idcategoria INT NULL,
    nome VARCHAR(100) NOT NULL,
    descricao_item VARCHAR(200) NULL,
    detalhes TEXT NULL,
    preco DECIMAL(10,2) NOT NULL,
    tipo_item CHAR(1) NOT NULL DEFAULT 'P',
    ativo_produto_servico CHAR(1) NOT NULL DEFAULT 'S',
    grupo_cardapio VARCHAR(10) NOT NULL DEFAULT 'outros',
    saldo INT NOT NULL DEFAULT 0,
    FOREIGN KEY (idcategoria) REFERENCES categoria(idcategoria),
    INDEX ix_item_nome (nome)
) ENGINE=InnoDB;

CREATE TABLE pessoa (
    idpessoa INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    cpf VARCHAR(11) NULL UNIQUE,
    email VARCHAR(254) NULL UNIQUE,
    telefone VARCHAR(11) NULL,
    tipo_pessoa CHAR(1) NOT NULL DEFAULT 'C',
    token_pedidos CHAR(64) NULL UNIQUE,
    senha_hash VARCHAR(255) NULL
) ENGINE=InnoDB;

CREATE TABLE atendimento (
    idatendimento INT AUTO_INCREMENT PRIMARY KEY,
    idpessoa INT NULL,
    data_atendimento DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status_atendimento VARCHAR(30) NOT NULL DEFAULT 'Em Processamento',
    observacao TEXT NULL,
    estoque_baixado BOOLEAN NOT NULL DEFAULT FALSE,
    modalidade VARCHAR(10) NULL,
    taxa_entrega DECIMAL(10,2) NOT NULL DEFAULT 0,
    FOREIGN KEY (idpessoa) REFERENCES pessoa(idpessoa)
) ENGINE=InnoDB;

CREATE TABLE atendimentoitem (
    idatendimentoitem INT AUTO_INCREMENT PRIMARY KEY,
    idatendimento INT NOT NULL,
    iditem INT NOT NULL,
    quantidade_itens INT NOT NULL,
    valor_unitario DECIMAL(10,2) NOT NULL,
    fatias INT NULL,
    FOREIGN KEY (idatendimento) REFERENCES atendimento(idatendimento),
    FOREIGN KEY (iditem) REFERENCES item(iditem)
) ENGINE=InnoDB;

-- O histórico usa quantidade positiva para entrada e negativa para retirada manual.
CREATE TABLE entrada_produto (
    id INT AUTO_INCREMENT PRIMARY KEY,
    iditem INT NOT NULL,
    quantidade INT NOT NULL,
    data_entrada DATE NOT NULL,
    valor_unitario DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (iditem) REFERENCES item(iditem)
) ENGINE=InnoDB;

-- Cardápio inicial para apresentação.
INSERT INTO categoria (idcategoria, categoria, grupo_cardapio) VALUES
    (1, 'Tradicionais', 'pizza'),
    (2, 'Refrigerantes', 'bebidas'),
    (3, 'Acompanhamentos', 'outros');

INSERT INTO item
    (iditem, idcategoria, nome, descricao_item, detalhes, preco, grupo_cardapio, saldo)
VALUES
    (1, 1, 'Pizza Calabresa', 'Calabresa, cebola e muçarela.',
     'Pizza de calabresa com molho de tomate, muçarela, cebola e orégano. Escolha o tamanho antes de adicionar.', 39.90, 'pizza', 10),
    (2, 1, 'Pizza Marguerita', 'Muçarela, tomate e manjericão.',
     'Pizza com molho de tomate, muçarela, tomate e manjericão. Disponível em família, média e pequena.', 42.90, 'pizza', 10),
    (3, 2, 'Refrigerante 2 litros', 'Garrafa de refrigerante de 2 litros.',
     'Uma garrafa de refrigerante de 2 litros para acompanhar seu pedido.', 12.50, 'bebidas', 20),
    (4, 3, 'Pão de alho', 'Porção de pão de alho.',
     'Uma porção de pão de alho para acompanhar a pizza.', 8.00, 'outros', 15);

INSERT INTO entrada_produto (iditem, quantidade, data_entrada, valor_unitario) VALUES
    (1, 10, CURRENT_DATE, 39.90),
    (2, 10, CURRENT_DATE, 42.90),
    (3, 20, CURRENT_DATE, 12.50),
    (4, 15, CURRENT_DATE, 8.00);
