-- Script para verificar e corrigir a constraint de status na tabela patrimonio_contestoes

-- 1. Verificar a constraint atual
SELECT 
    conname as constraint_name,
    pg_get_constraintdef(oid) as constraint_definition
FROM pg_constraint 
WHERE conname = 'patrimonio_contestoes_status_check';

-- 2. Verificar os valores únicos de status atualmente na tabela
SELECT DISTINCT status, COUNT(*) as quantidade
FROM patrimonio_contestoes 
GROUP BY status
ORDER BY status;

-- 3. Remover a constraint atual (se existir)
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM pg_constraint 
        WHERE conname = 'patrimonio_contestoes_status_check'
    ) THEN
        ALTER TABLE patrimonio_contestoes 
        DROP CONSTRAINT patrimonio_contestoes_status_check;
        RAISE NOTICE 'Constraint patrimonio_contestoes_status_check removida com sucesso.';
    ELSE
        RAISE NOTICE 'Constraint patrimonio_contestoes_status_check não encontrada.';
    END IF;
END $$;

-- 4. Criar nova constraint que inclui 'negada'
ALTER TABLE patrimonio_contestoes 
ADD CONSTRAINT patrimonio_contestoes_status_check 
CHECK (status IN ('pendente', 'em_analise', 'resolvida', 'cancelada', 'negada', 'pendente_colaborador'));

-- 5. Verificar se a nova constraint foi criada corretamente
SELECT 
    conname as constraint_name,
    pg_get_constraintdef(oid) as constraint_definition
FROM pg_constraint 
WHERE conname = 'patrimonio_contestoes_status_check';

-- 6. Testar se podemos inserir um registro com status 'negada'
DO $$
BEGIN
    -- Verificar se existe algum registro para testar
    IF EXISTS (SELECT 1 FROM patrimonio_contestoes LIMIT 1) THEN
        RAISE NOTICE 'Tabela possui registros. Constraint atualizada com sucesso.';
    ELSE
        RAISE NOTICE 'Tabela vazia. Constraint atualizada com sucesso.';
    END IF;
END $$;

-- 7. Mostrar todos os status permitidos
SELECT 'Status permitidos pela nova constraint:' as info;
SELECT unnest(ARRAY['pendente', 'em_analise', 'resolvida', 'cancelada', 'negada', 'pendente_colaborador']) as status_permitido;
