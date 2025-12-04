-- Verificar se a view vwplanostelefonia existe
SELECT 'Verificando views existentes...' as status;

-- Listar todas as views
SELECT table_name, table_type 
FROM information_schema.tables 
WHERE table_type = 'VIEW'
ORDER BY table_name;

-- Verificar especificamente vwplanostelefonia
SELECT 'Verificando vwplanostelefonia...' as status;
SELECT table_name, table_type 
FROM information_schema.tables 
WHERE table_name = 'vwplanostelefonia';

-- Se não existir, verificar se existe planosvm
SELECT 'Verificando planosvm...' as status;
SELECT table_name, table_type 
FROM information_schema.tables 
WHERE table_name = 'planosvm';
