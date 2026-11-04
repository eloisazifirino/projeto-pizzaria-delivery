USE pizzaria_db;

-- Execute uma vez após as migrações anteriores.
ALTER TABLE categoria ADD COLUMN grupo_cardapio VARCHAR(10) NOT NULL DEFAULT 'outros';

-- Preserva a identificação das pizzas cadastradas antes da separação por abas.
UPDATE item SET grupo_cardapio = 'pizza'
WHERE tipo_item = 'P' AND LOWER(TRIM(nome)) LIKE 'pizza %';

-- Categorias existentes explicitamente identificadas como bebidas.
UPDATE item i JOIN categoria c ON c.idcategoria = i.idcategoria
SET i.grupo_cardapio = 'bebidas'
WHERE i.tipo_item = 'P' AND i.grupo_cardapio <> 'pizza'
  AND LOWER(c.categoria) LIKE '%bebida%';

-- Uma categoria compartilhada passa a ter uma versão para cada aba.
ALTER TABLE categoria ADD COLUMN origem_migracao INT NULL;
INSERT INTO categoria (categoria, grupo_cardapio, origem_migracao)
SELECT DISTINCT c.categoria,
       CASE WHEN i.tipo_item = 'S' THEN 'servicos' ELSE i.grupo_cardapio END,
       c.idcategoria
FROM item i JOIN categoria c ON c.idcategoria = i.idcategoria
WHERE (CASE WHEN i.tipo_item = 'S' THEN 'servicos' ELSE i.grupo_cardapio END) <> 'outros';

UPDATE item i
JOIN categoria c ON c.origem_migracao = i.idcategoria
    AND c.grupo_cardapio = CASE WHEN i.tipo_item = 'S' THEN 'servicos' ELSE i.grupo_cardapio END
SET i.idcategoria = c.idcategoria;

ALTER TABLE categoria DROP COLUMN origem_migracao;
