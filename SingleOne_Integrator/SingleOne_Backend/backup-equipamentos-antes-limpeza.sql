-- Script de backup da tabela equipamentos antes da limpeza
-- Execute este script ANTES de executar a limpeza dos campos duplicados

-- 1. Criar backup completo da tabela equipamentos
CREATE TABLE IF NOT EXISTS equipamentos_backup_antes_limpeza AS 
SELECT * FROM equipamentos;

-- 2. Verificar se o backup foi criado
SELECT 
    'Backup criado com sucesso' as status,
    COUNT(*) as total_registros_backup
FROM equipamentos_backup_antes_limpeza;

-- 3. Verificar estrutura do backup
SELECT 
    'Estrutura do backup' as info,
    COUNT(*) as total_colunas
FROM information_schema.columns 
WHERE table_name = 'equipamentos_backup_antes_limpeza';

-- 4. Comparar contagem de registros
SELECT 
    'Comparação de registros' as info,
    (SELECT COUNT(*) FROM equipamentos) as tabela_original,
    (SELECT COUNT(*) FROM equipamentos_backup_antes_limpeza) as tabela_backup,
    CASE 
        WHEN (SELECT COUNT(*) FROM equipamentos) = (SELECT COUNT(*) FROM equipamentos_backup_antes_limpeza) 
        THEN 'OK - Mesma quantidade de registros'
        ELSE 'ERRO - Quantidades diferentes'
    END as status;

-- 5. Verificar se o backup tem todos os campos
SELECT 
    'Campos no backup' as info,
    column_name,
    data_type
FROM information_schema.columns 
WHERE table_name = 'equipamentos_backup_antes_limpeza'
ORDER BY ordinal_position;

-- 6. Mostrar resumo do backup
SELECT 
    'Resumo do backup' as info,
    'Backup criado em: ' || NOW() as data_backup,
    'Total de registros: ' || (SELECT COUNT(*) FROM equipamentos_backup_antes_limpeza) as registros,
    'Total de campos: ' || (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'equipamentos_backup_antes_limpeza') as campos;

-- 7. Instruções para restaurar (se necessário)
SELECT 
    'Para restaurar o backup (se necessário):' as instrucoes,
    'DROP TABLE equipamentos;' as passo1,
    'CREATE TABLE equipamentos AS SELECT * FROM equipamentos_backup_antes_limpeza;' as passo2,
    '-- Reconfigurar índices e constraints' as passo3;
