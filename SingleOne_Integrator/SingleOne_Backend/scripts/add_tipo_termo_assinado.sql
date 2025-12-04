-- ========================================================================================================
-- Script: Adicionar coluna para identificar o tipo de termo assinado
-- Descrição: Adiciona coluna INT na tabela requisicoes para identificar se o snapshot é BYOD ou Corporativo
-- Data: 2025-10-17
-- ========================================================================================================

-- Verificar se a coluna já existe antes de adicionar
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'requisicoes' 
        AND column_name = 'tipo_termo_assinado'
    ) THEN
        ALTER TABLE requisicoes 
        ADD COLUMN tipo_termo_assinado INTEGER NULL;
        
        RAISE NOTICE 'Coluna tipo_termo_assinado adicionada com sucesso!';
    ELSE
        RAISE NOTICE 'Coluna tipo_termo_assinado já existe!';
    END IF;
END $$;

-- Verificar resultado
SELECT 
    column_name, 
    data_type, 
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'requisicoes' 
AND column_name = 'tipo_termo_assinado';

