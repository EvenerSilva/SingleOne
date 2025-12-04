-- Script para verificar a view vwtelefonia
BEGIN;

-- 1. Verificar se a view existe
\echo '=== VERIFICANDO SE A VIEW vwtelefonia EXISTE ==='
SELECT schemaname, viewname, definition 
FROM pg_views 
WHERE viewname = 'vwtelefonia';

-- 2. Se não existir, verificar outras views relacionadas
\echo '=== VERIFICANDO OUTRAS VIEWS DE TELEFONIA ==='
SELECT schemaname, viewname 
FROM pg_views 
WHERE viewname LIKE '%telefonia%' OR viewname LIKE '%plano%';

-- 3. Verificar se há alguma view planosvm
\echo '=== VERIFICANDO SE EXISTE planosvm ==='
SELECT schemaname, viewname 
FROM pg_views 
WHERE viewname = 'planosvm';

-- 4. Verificar estrutura da tabela telefoniaplano
\echo '=== ESTRUTURA DA TABELA telefoniaplano ==='
\d telefoniaplano

COMMIT;
