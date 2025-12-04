-- 🎯 SCRIPT PARA CORRIGIR CAMPO EQUIPAMENTO NA TABELA EQUIPAMENTOHISTORICO
-- ✅ TORNAR O CAMPO EQUIPAMENTO NULLABLE PARA PERMITIR LINHAS TELEFÔNICAS

-- 1. Verificar a estrutura atual da tabela
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentohistorico' 
AND column_name = 'equipamento';

-- 2. Verificar se há constraints que impedem a mudança
SELECT 
    tc.constraint_name,
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
AND tc.table_name = 'equipamentohistorico'
AND kcu.column_name = 'equipamento';

-- 3. Remover a foreign key constraint se existir
-- ALTER TABLE equipamentohistorico DROP CONSTRAINT IF EXISTS fkeqphistoricoequipamento;

-- 4. Alterar o campo equipamento para nullable
ALTER TABLE equipamentohistorico ALTER COLUMN equipamento DROP NOT NULL;

-- 5. Verificar se a alteração foi aplicada
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentohistorico' 
AND column_name = 'equipamento';

-- 6. Recriar a foreign key constraint se necessário
-- ALTER TABLE equipamentohistorico 
-- ADD CONSTRAINT fkeqphistoricoequipamento 
-- FOREIGN KEY (equipamento) REFERENCES equipamentos(id) 
-- ON DELETE SET NULL;

-- ✅ RESULTADO ESPERADO:
-- O campo 'equipamento' deve estar 'is_nullable = YES'
-- Isso permitirá salvar registros de histórico para linhas telefônicas sem equipamento
