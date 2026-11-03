

USE pizzaria_db;

INSERT INTO item (nome, descricao_item, preco, tipo_item, ativo_produto_servico)
VALUES
  ('Pizza Calabresa', 'Calabresa, cebola e muçarela', 39.90, 'P', 'S'),
  ('Pizza Marguerita', 'Muçarela, tomate e manjericão', 42.90, 'P', 'S');

  USE pizzaria_db;
ALTER TABLE atendimento ADD COLUMN observacao TEXT NULL;