USE pizzaria_db;

-- Execute uma vez após adicionar_observacao_pedido.sql.
ALTER TABLE pessoa ADD COLUMN email VARCHAR(254) NULL UNIQUE;
ALTER TABLE pessoa ADD COLUMN token_pedidos CHAR(64) NULL UNIQUE;
ALTER TABLE item ADD COLUMN saldo INT NOT NULL DEFAULT 0;
ALTER TABLE atendimento ADD COLUMN estoque_baixado BOOLEAN NOT NULL DEFAULT FALSE;

CREATE TABLE entrada_produto (
    id INT AUTO_INCREMENT PRIMARY KEY,
    iditem INT NOT NULL,
    quantidade INT NOT NULL,
    data_entrada DATE NOT NULL,
    valor_unitario DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (iditem) REFERENCES item(iditem)
);

-- A API controla a situação do pedido.
DROP TRIGGER IF EXISTS trg_atualiza_status_item;

