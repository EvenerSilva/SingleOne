-- Script para verificar a view planosvm
-- Esta view pode ser usada pela API

-- 1. Verificar se a view planosvm existe
SELECT 'Verificando view planosvm...' as status;

SELECT EXISTS (
    SELECT 1 
    FROM information_schema.views 
    WHERE table_name = 'planosvm'
) as view_existe;

-- 2. Se existir, contar registros
SELECT 'Contando registros na view planosvm...' as status;

SELECT COUNT(*) as total_planosvm 
FROM planosvm;

-- 3. Mostrar alguns registros
SELECT 'Mostrando alguns registros da view planosvm...' as status;

SELECT * FROM planosvm LIMIT 5;

-- 4. Verificar estrutura da view
SELECT 'Estrutura da view planosvm...' as status;

SELECT column_name, data_type, is_nullable
FROM information_schema.columns 
WHERE table_name = 'planosvm'
ORDER BY ordinal_position;
