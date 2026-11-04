USE pizzaria_db;

UPDATE atendimento
SET status_atendimento = 'Em Processamento'
WHERE LOWER(TRIM(status_atendimento)) = 'aberto';

ALTER TABLE atendimento
    ALTER COLUMN status_atendimento SET DEFAULT 'Em Processamento';
