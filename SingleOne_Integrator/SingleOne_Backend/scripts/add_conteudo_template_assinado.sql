-- ========================================================================================================
-- Script: Adicionar coluna para snapshot do conteúdo do template assinado
-- Descrição: Adiciona coluna TEXT na tabela requisicoes para armazenar o conteúdo do template
--            no momento da assinatura, permitindo manter histórico mesmo quando template é atualizado
-- Data: 2025-10-17
-- ========================================================================================================

-- Verificar se a coluna já existe antes de adicionar
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'requisicoes' 
        AND column_name = 'conteudo_template_assinado'
    ) THEN
        ALTER TABLE requisicoes 
        ADD COLUMN conteudo_template_assinado TEXT NULL;
        
        RAISE NOTICE 'Coluna conteudo_template_assinado adicionada com sucesso!';
    ELSE
        RAISE NOTICE 'Coluna conteudo_template_assinado já existe!';
    END IF;
END $$;

-- Verificar resultado
SELECT 
    column_name, 
    data_type, 
    is_nullable,
    character_maximum_length
FROM information_schema.columns 
WHERE table_name = 'requisicoes' 
AND column_name = 'conteudo_template_assinado';

