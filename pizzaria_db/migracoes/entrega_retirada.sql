USE pizzaria_db;

ALTER TABLE atendimento
    ADD COLUMN modalidade VARCHAR(10) NULL,
    ADD COLUMN taxa_entrega DECIMAL(10,2) NOT NULL DEFAULT 0,
    MODIFY COLUMN status_atendimento VARCHAR(30) DEFAULT 'Em Processamento';
