-- Script simples para corrigir a tabela centrocusto
-- Adiciona a coluna filial_id se ela não existir

-- Verificar se a coluna existe
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'centrocusto' 
        AND column_name = 'filial_id'
    ) THEN
        -- Adicionar a coluna filial_id
        ALTER TABLE centrocusto ADD COLUMN filial_id INTEGER;
        
        -- Adicionar a constraint de chave estrangeira
        ALTER TABLE centrocusto 
        ADD CONSTRAINT fk_centrocusto_filial 
        FOREIGN KEY (filial_id) REFERENCES filiais(id);
        
        RAISE NOTICE 'Coluna filial_id adicionada com sucesso!';
    ELSE
        RAISE NOTICE 'Coluna filial_id já existe.';
    END IF;
END $$;
