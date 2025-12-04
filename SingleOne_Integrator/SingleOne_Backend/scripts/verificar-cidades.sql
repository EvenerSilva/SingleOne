-- Verificar total de cidades
SELECT COUNT(*) as total_cidades FROM cidades;

-- Verificar cidades por estado
SELECT 
    e.sigla as estado,
    e.nome as nome_estado,
    COUNT(c.id) as total_cidades
FROM estados e
LEFT JOIN cidades c ON e.id = c.estado_id
GROUP BY e.id, e.sigla, e.nome
ORDER BY e.nome;

-- Mostrar algumas cidades de exemplo
SELECT nome, estado_id FROM cidades LIMIT 10;
