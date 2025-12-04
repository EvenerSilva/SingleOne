-- Verificar dados da view sem aspas
SELECT COUNT(*) as total_registros FROM vwUltimasRequisicaoNaoBYOD;

-- Verificar alguns registros
SELECT 
    requisicaoid,
    nomecolaboradorfinal,
    requisicaostatus,
    equipamentostatus,
    linhatelefonica
FROM vwUltimasRequisicaoNaoBYOD 
WHERE nomecolaboradorfinal LIKE '%raimundo%'
LIMIT 5;
