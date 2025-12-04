-- 🔍 VERIFICAR LOCALIDADES INATIVAS
-- Script para verificar se existem localidades inativas no banco

-- 1. Verificar total de localidades
SELECT 
    'Total de localidades' as tipo,
    COUNT(*) as quantidade
FROM localidades;

-- 2. Verificar localidades por status
SELECT 
    'Localidades ativas' as tipo,
    COUNT(*) as quantidade
FROM localidades 
WHERE ativo = true;

SELECT 
    'Localidades inativas' as tipo,
    COUNT(*) as quantidade
FROM localidades 
WHERE ativo = false;

-- 3. Verificar localidades por cliente (substitua pelo ID do cliente correto)
SELECT 
    'Localidades por cliente' as tipo,
    cliente,
    COUNT(*) as total,
    SUM(CASE WHEN ativo = true THEN 1 ELSE 0 END) as ativas,
    SUM(CASE WHEN ativo = false THEN 1 ELSE 0 END) as inativas
FROM localidades 
GROUP BY cliente
ORDER BY cliente;

-- 4. Listar algumas localidades inativas para verificar
SELECT 
    id,
    descricao,
    cidade,
    estado,
    cliente,
    ativo,
    created_at,
    updated_at
FROM localidades 
WHERE ativo = false
ORDER BY id
LIMIT 10;

-- 5. Verificar se há localidades com status NULL
SELECT 
    'Localidades com status NULL' as tipo,
    COUNT(*) as quantidade
FROM localidades 
WHERE ativo IS NULL;

-- 6. Verificar estrutura da tabela localidades
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'localidades'
ORDER BY ordinal_position;
