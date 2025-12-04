-- Script para corrigir todos os problemas da telefonia
BEGIN;

-- 1. Garantir que a constraint fkrilinhatelefonica existe
DO $$
BEGIN
    -- Remover constraint se existir (para recriar corretamente)
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'fkrilinhatelefonica'
    ) THEN
        ALTER TABLE requisicoesitens DROP CONSTRAINT fkrilinhatelefonica;
        RAISE NOTICE 'Constraint fkrilinhatelefonica removida para recriação';
    END IF;
    
    -- Criar constraint corretamente
    ALTER TABLE requisicoesitens
    ADD CONSTRAINT fkrilinhatelefonica
    FOREIGN KEY (linhatelefonica)
    REFERENCES telefonialinhas(id)
    ON DELETE SET NULL;
    
    RAISE NOTICE 'Constraint fkrilinhatelefonica criada com sucesso!';
END $$;

-- 2. Garantir que a constraint fkriequipamento está correta
DO $$
BEGIN
    -- Remover constraint se existir
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'fkriequipamento'
    ) THEN
        ALTER TABLE requisicoesitens DROP CONSTRAINT fkriequipamento;
        RAISE NOTICE 'Constraint fkriequipamento removida para recriação';
    END IF;
    
    -- Criar constraint corretamente
    ALTER TABLE requisicoesitens
    ADD CONSTRAINT fkriequipamento
    FOREIGN KEY (equipamento)
    REFERENCES equipamentos(id)
    ON DELETE SET NULL;
    
    RAISE NOTICE 'Constraint fkriequipamento criada com sucesso!';
END $$;

-- 3. Verificar se a coluna equipamento é nullable
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'requisicoesitens' 
        AND column_name = 'equipamento' 
        AND is_nullable = 'NO'
    ) THEN
        ALTER TABLE requisicoesitens ALTER COLUMN equipamento DROP NOT NULL;
        RAISE NOTICE 'Coluna equipamento tornada nullable';
    ELSE
        RAISE NOTICE 'Coluna equipamento já é nullable';
    END IF;
END $$;

-- 4. Verificar se a coluna linhatelefonica é nullable
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'requisicoesitens' 
        AND column_name = 'linhatelefonica' 
        AND is_nullable = 'NO'
    ) THEN
        ALTER TABLE requisicoesitens ALTER COLUMN linhatelefonica DROP NOT NULL;
        RAISE NOTICE 'Coluna linhatelefonica tornada nullable';
    ELSE
        RAISE NOTICE 'Coluna linhatelefonica já é nullable';
    END IF;
END $$;

-- 5. Verificar se a tabela equipamentohistorico tem equipamento nullable
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'equipamentohistorico' 
        AND column_name = 'equipamento' 
        AND is_nullable = 'NO'
    ) THEN
        ALTER TABLE equipamentohistorico ALTER COLUMN equipamento DROP NOT NULL;
        RAISE NOTICE 'Coluna equipamento em equipamentohistorico tornada nullable';
    ELSE
        RAISE NOTICE 'Coluna equipamento em equipamentohistorico já é nullable';
    END IF;
END $$;

-- 6. Verificar se a constraint fkeqphistoricoequipamento está correta
DO $$
BEGIN
    -- Remover constraint se existir
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'fkeqphistoricoequipamento'
    ) THEN
        ALTER TABLE equipamentohistorico DROP CONSTRAINT fkeqphistoricoequipamento;
        RAISE NOTICE 'Constraint fkeqphistoricoequipamento removida para recriação';
    END IF;
    
    -- Criar constraint corretamente
    ALTER TABLE equipamentohistorico
    ADD CONSTRAINT fkeqphistoricoequipamento
    FOREIGN KEY (equipamento)
    REFERENCES equipamentos(id)
    ON DELETE SET NULL;
    
    RAISE NOTICE 'Constraint fkeqphistoricoequipamento criada com sucesso!';
END $$;

-- 7. Verificar estrutura final
\echo '=== ESTRUTURA FINAL DA TABELA requisicoesitens ==='
\d requisicoesitens

\echo '=== ESTRUTURA FINAL DA TABELA equipamentohistorico ==='
\d equipamentohistorico

\echo '=== CONSTRAINTS FINAIS ==='
SELECT 
    tc.constraint_name, 
    tc.table_name, 
    kcu.column_name, 
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM 
    information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
      ON tc.constraint_name = kcu.constraint_name
      AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
      ON ccu.constraint_name = tc.constraint_name
      AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_name IN ('requisicoesitens', 'equipamentohistorico');

COMMIT;
