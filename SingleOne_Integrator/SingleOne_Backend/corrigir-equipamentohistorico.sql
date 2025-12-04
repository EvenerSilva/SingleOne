-- Script para corrigir a constraint da tabela equipamentohistorico
-- Permitir NULL no campo equipamento para suportar linhas telefônicas

BEGIN;

-- 1. Remover a constraint de chave estrangeira existente
ALTER TABLE equipamentohistorico DROP CONSTRAINT IF EXISTS fkeqphistoricoequipamento;

-- 2. Alterar a coluna equipamento para permitir NULL
ALTER TABLE equipamentohistorico ALTER COLUMN equipamento DROP NOT NULL;

-- 3. Recriar a constraint de chave estrangeira permitindo NULL
ALTER TABLE equipamentohistorico 
ADD CONSTRAINT fkeqphistoricoequipamento 
FOREIGN KEY (equipamento) 
REFERENCES equipamentos(id) 
ON DELETE SET NULL;

-- 4. Verificar se a alteração foi aplicada
SELECT 
    column_name, 
    is_nullable, 
    data_type 
FROM information_schema.columns 
WHERE table_name = 'equipamentohistorico' 
AND column_name = 'equipamento';

-- 5. Verificar as constraints da tabela
SELECT 
    constraint_name, 
    constraint_type 
FROM information_schema.table_constraints 
WHERE table_name = 'equipamentohistorico';

COMMIT;
