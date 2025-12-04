-- 🎯 VERIFICAR SE EXISTE EQUIPAMENTO COM ID 0 PARA LINHAS TELEFÔNICAS

-- 1. Verificar se existe equipamento com ID 0
SELECT * FROM equipamentos WHERE id = 0;

-- 2. Verificar quantos registros de histórico já existem com equipamento = 0
SELECT COUNT(*) as total_registros_equipamento_zero 
FROM equipamentohistorico 
WHERE equipamento = 0;

-- 3. Verificar se há registros de histórico com linhas telefônicas
SELECT COUNT(*) as total_registros_com_linha 
FROM equipamentohistorico 
WHERE linhatelefonica IS NOT NULL;

-- 4. Verificar os últimos registros de histórico para entender o padrão
SELECT 
    id,
    equipamento,
    linhatelefonica,
    equipamentostatus,
    dtregistro
FROM equipamentohistorico 
WHERE linhatelefonica IS NOT NULL
ORDER BY dtregistro DESC
LIMIT 10;
