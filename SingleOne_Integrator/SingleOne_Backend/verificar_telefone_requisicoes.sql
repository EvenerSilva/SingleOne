-- Script para verificar se há linhas telefônicas em requisições
-- e testar a busca por número de telefone

-- 1. Verificar se existem linhas telefônicas
SELECT 
    'Linhas telefônicas cadastradas' as tipo,
    COUNT(*) as total
FROM telefonialinhas 
WHERE ativo = true;

-- 2. Verificar se existem requisições com linhas telefônicas
SELECT 
    'Requisições com linhas telefônicas' as tipo,
    COUNT(*) as total
FROM requisicoesitens 
WHERE linhatelefonica IS NOT NULL;

-- 3. Verificar números de telefone existentes
SELECT 
    'Números de telefone disponíveis' as tipo,
    numero,
    id
FROM telefonialinhas 
WHERE ativo = true
ORDER BY numero
LIMIT 10;

-- 4. Verificar requisições com linhas telefônicas e seus números
SELECT 
    ri.requisicao,
    ri.linhatelefonica,
    tl.numero as numero_telefone,
    tl.iccid
FROM requisicoesitens ri
JOIN telefonialinhas tl ON ri.linhatelefonica = tl.id
WHERE ri.linhatelefonica IS NOT NULL
LIMIT 10;

-- 5. Testar busca por número específico (exemplo: 8590987654)
SELECT 
    'Busca por 8590987654' as tipo,
    ri.requisicao,
    tl.numero as numero_telefone
FROM requisicoesitens ri
JOIN telefonialinhas tl ON ri.linhatelefonica = tl.id
WHERE ri.linhatelefonica IS NOT NULL
AND tl.numero::text LIKE '%8590987654%';
