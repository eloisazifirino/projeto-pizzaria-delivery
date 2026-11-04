USE pizzaria_db;

-- Execute uma vez depois de atividade04.sql.
ALTER TABLE pessoa ADD COLUMN senha_hash VARCHAR(255) NULL;
ALTER TABLE item
    ADD COLUMN grupo_cardapio VARCHAR(10) NOT NULL DEFAULT 'outros',
    ADD COLUMN detalhes TEXT NULL;

UPDATE item i
LEFT JOIN categoria c ON c.idcategoria = i.idcategoria
SET i.grupo_cardapio = 'pizza'
WHERE i.tipo_item = 'P'
  AND (LOWER(i.nome) LIKE 'pizza %' OR LOWER(c.categoria) LIKE 'pizza%');
