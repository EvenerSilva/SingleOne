-- Verificar se a view contém dados do Raimundo
SELECT 
    requisicaoid,
    cliente,
    colaboradorfinal,
    nomecolaboradorfinal,
    requisicaostatus,
    equipamentostatus,
    linhatelefonica,
    numero,
    dtentrega,
    dtdevolucao
FROM vwUltimasRequisicaoNaoBYOD
WHERE nomecolaboradorfinal LIKE '%raimundo%'
ORDER BY dtentrega DESC;
