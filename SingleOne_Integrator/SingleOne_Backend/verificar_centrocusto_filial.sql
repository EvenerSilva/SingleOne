-- Script para verificar e corrigir a estrutura da tabela centrocusto
-- Verificar se a coluna filial_id existe
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'centrocusto' 
        AND column_name = 'filial_id'
    ) THEN
        -- Adicionar a coluna filial_id se ela não existir
        ALTER TABLE centrocusto ADD COLUMN filial_id INTEGER;
        
        -- Adicionar a constraint de chave estrangeira
        ALTER TABLE centrocusto 
        ADD CONSTRAINT fk_centrocusto_filial 
        FOREIGN KEY (filial_id) REFERENCES filiais(id);
        
        RAISE NOTICE 'Coluna filial_id adicionada à tabela centrocusto';
    ELSE
        RAISE NOTICE 'Coluna filial_id já existe na tabela centrocusto';
    END IF;
END $$;

-- Verificar a estrutura atual da tabela
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'centrocusto' 
ORDER BY ordinal_position;

-- Verificar as constraints existentes
SELECT conname, contype, pg_get_constraintdef(oid) as definition
FROM pg_constraint 
WHERE conrelid = 'centrocusto'::regclass;
