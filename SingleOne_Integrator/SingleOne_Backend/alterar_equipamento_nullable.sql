-- Script para alterar a coluna equipamento da tabela requisicoesitens para permitir valores NULL
-- Isso é necessário para permitir linhas telefônicas sem equipamento associado

BEGIN;

-- 1. Remover a constraint de chave estrangeira existente
ALTER TABLE requisicoesitens DROP CONSTRAINT IF EXISTS fkriequipamento;

-- 2. Alterar a coluna equipamento para permitir NULL
ALTER TABLE requisicoesitens ALTER COLUMN equipamento DROP NOT NULL;

-- 3. Recriar a constraint de chave estrangeira permitindo NULL
ALTER TABLE requisicoesitens 
ADD CONSTRAINT fkriequipamento 
FOREIGN KEY (equipamento) 
REFERENCES equipamentos(id) 
ON DELETE SET NULL;

-- 4. Criar constraint para linhas telefônicas se não existir
ALTER TABLE requisicoesitens DROP CONSTRAINT IF EXISTS fkrilinhatelefonica;
ALTER TABLE requisicoesitens 
ADD CONSTRAINT fkrilinhatelefonica 
FOREIGN KEY (linhatelefonica) 
REFERENCES telefonialinhas(id) 
ON DELETE SET NULL;

COMMIT;

-- 5. Verificar se a alteração foi aplicada
SELECT 
    column_name, 
    is_nullable, 
    data_type 
FROM information_schema.columns 
WHERE table_name = 'requisicoesitens' 
AND column_name IN ('equipamento', 'linhatelefonica');

-- 6. Verificar as constraints da tabela
SELECT 
    constraint_name, 
    constraint_type 
FROM information_schema.table_constraints 
WHERE table_name = 'requisicoesitens';
