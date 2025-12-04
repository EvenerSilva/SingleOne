-- Script para verificar planos de telefonia
-- Verificar se a view existe e tem dados

-- 1. Verificar se a view vwplanostelefonia existe
SELECT 'Verificando view vwplanostelefonia...' as status;

SELECT EXISTS (
    SELECT 1 
    FROM information_schema.views 
    WHERE table_name = 'vwplanostelefonia'
) as view_existe;

-- 2. Verificar se a tabela telefoniaplanos existe
SELECT 'Verificando tabela telefoniaplanos...' as status;

SELECT EXISTS (
    SELECT 1 
    FROM information_schema.tables 
    WHERE table_name = 'telefoniaplanos'
) as tabela_existe;

-- 3. Contar registros na tabela telefoniaplanos
SELECT 'Contando registros na tabela telefoniaplanos...' as status;

SELECT COUNT(*) as total_planos 
FROM telefoniaplanos 
WHERE ativo = true;

-- 4. Verificar se a view tem dados
SELECT 'Verificando dados na view vwplanostelefonia...' as status;

SELECT COUNT(*) as total_view 
FROM vwplanostelefonia;

-- 5. Mostrar alguns registros da view
SELECT 'Mostrando alguns registros da view...' as status;

SELECT * FROM vwplanostelefonia LIMIT 5;

-- 6. Verificar estrutura da tabela telefoniaplanos
SELECT 'Estrutura da tabela telefoniaplanos...' as status;

SELECT column_name, data_type, is_nullable
FROM information_schema.columns 
WHERE table_name = 'telefoniaplanos'
ORDER BY ordinal_position;
