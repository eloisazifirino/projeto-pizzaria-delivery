CREATE VIEW vw_pedidos_detalhados AS
SELECT 
    a.idatendimento AS numero_pedido,
    p.nome AS cliente,
    a.data_atendimento,
    a.status_atendimento
FROM atendimento a
INNER JOIN pessoa p ON a.idpessoa = p.idpessoa;