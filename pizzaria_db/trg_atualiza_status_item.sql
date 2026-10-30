DROP TRIGGER IF EXISTS trg_atualiza_status_item;

CREATE TRIGGER trg_atualiza_status_item
AFTER INSERT ON atendimentoitem
FOR EACH ROW
UPDATE atendimento 
SET status_atendimento = 'Em Processamento' 
WHERE idatendimento = NEW.idatendimento;