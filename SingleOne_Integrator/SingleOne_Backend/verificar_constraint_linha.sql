-- Script para verificar e corrigir constraint de linha telefônica
BEGIN;

-- 1. Verificar se a constraint existe
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
    AND tc.table_name='requisicoesitens'
    AND kcu.column_name='linhatelefonica';

-- 2. Se não existir, criar a constraint
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'fkrilinhatelefonica'
    ) THEN
        ALTER TABLE requisicoesitens
        ADD CONSTRAINT fkrilinhatelefonica
        FOREIGN KEY (linhatelefonica)
        REFERENCES telefonialinhas(id)
        ON DELETE SET NULL;
        
        RAISE NOTICE 'Constraint fkrilinhatelefonica criada com sucesso!';
    ELSE
        RAISE NOTICE 'Constraint fkrilinhatelefonica já existe!';
    END IF;
END $$;

-- 3. Verificar estrutura final da tabela
\d requisicoesitens

COMMIT;
