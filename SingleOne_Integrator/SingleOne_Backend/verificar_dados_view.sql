-- Verificar se há dados na view
SELECT COUNT(*) as total_registros FROM vwUltimasRequisicaoNaoBYOD;

-- Verificar alguns registros da view
SELECT 
    requisicaoid,
    nomecolaboradorfinal,
    requisicaostatus,
    equipamentostatus,
    linhatelefonica
FROM vwUltimasRequisicaoNaoBYOD 
LIMIT 10;
