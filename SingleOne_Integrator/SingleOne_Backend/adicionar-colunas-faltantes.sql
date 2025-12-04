-- Adicionar colunas que estão faltando no banco de dados
-- Estas colunas são necessárias para o Entity Framework funcionar corretamente

-- Verificar se as colunas existem antes de adicionar
DO $$
BEGIN
    -- Adicionar ClienteId se não existir
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'equipamentos' 
        AND column_name = 'ClienteId'
    ) THEN
        ALTER TABLE equipamentos ADD COLUMN "ClienteId" INTEGER;
        RAISE NOTICE 'Coluna ClienteId adicionada';
    END IF;
    
    -- Adicionar ContratoId se não existir
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'equipamentos' 
        AND column_name = 'ContratoId'
    ) THEN
        ALTER TABLE equipamentos ADD COLUMN "ContratoId" INTEGER;
        RAISE NOTICE 'Coluna ContratoId adicionada';
    END IF;
    
    -- Adicionar EmpresaId se não existir
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'equipamentos' 
        AND column_name = 'EmpresaId'
    ) THEN
        ALTER TABLE equipamentos ADD COLUMN "EmpresaId" INTEGER;
        RAISE NOTICE 'Coluna EmpresaId adicionada';
    END IF;
    
    -- Adicionar EquipamentosstatusId se não existir
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'equipamentos' 
        AND column_name = 'EquipamentosstatusId'
    ) THEN
        ALTER TABLE equipamentos ADD COLUMN "EquipamentosstatusId" INTEGER;
        RAISE NOTICE 'Coluna EquipamentosstatusId adicionada';
    END IF;
END $$;

-- Verificar estrutura final da tabela
SELECT 
    column_name, 
    data_type, 
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
ORDER BY ordinal_position;