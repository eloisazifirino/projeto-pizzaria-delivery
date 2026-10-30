DROP PROCEDURE IF EXISTS sp_inserir_item;

CREATE PROCEDURE sp_inserir_item(
    IN p_idcategoria INT,
    IN p_nome VARCHAR(100),
    IN p_descricao VARCHAR(200),
    IN p_preco DECIMAL(10,2)
)
INSERT INTO item (idcategoria, nome, descricao_item, preco, tipo_item, ativo_produto_servico)
VALUES (p_idcategoria, p_nome, p_descricao, p_preco, 'P', 'S');