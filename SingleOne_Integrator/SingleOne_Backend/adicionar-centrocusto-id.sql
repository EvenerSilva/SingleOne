-- Script para adicionar a coluna CentrocustoId à tabela equipamentos
-- Esta coluna é necessária para o Entity Framework funcionar corretamente

-- Adicionar a coluna CentrocustoId
ALTER TABLE equipamentos 
ADD COLUMN "CentrocustoId" INTEGER;

-- Criar índice para a nova coluna
CREATE INDEX IF NOT EXISTS idx_equipamentos_centrocusto_id ON equipamentos("CentrocustoId");

-- Atualizar registros existentes para usar a nova coluna
UPDATE equipamentos 
SET "CentrocustoId" = centrocusto 
WHERE centrocusto IS NOT NULL;

-- Verificar se a coluna foi adicionada corretamente
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
AND column_name IN ('centrocusto', 'CentrocustoId')
ORDER BY column_name;



