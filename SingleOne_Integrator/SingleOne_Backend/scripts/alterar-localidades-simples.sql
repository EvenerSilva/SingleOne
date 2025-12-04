-- Script simples para adicionar campos cidade e estado na tabela localidades

-- Adicionar campo cidade (se não existir)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM information_schema.columns WHERE table_name = 'localidades' AND column_name = 'cidade') THEN
        ALTER TABLE localidades ADD COLUMN cidade VARCHAR(100);
        RAISE NOTICE 'Campo cidade adicionado com sucesso!';
    ELSE
        RAISE NOTICE 'Campo cidade já existe';
    END IF;
END $$;

-- Adicionar campo estado (se não existir)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM information_schema.columns WHERE table_name = 'localidades' AND column_name = 'estado') THEN
        ALTER TABLE localidades ADD COLUMN estado VARCHAR(50);
        RAISE NOTICE 'Campo estado adicionado com sucesso!';
    ELSE
        RAISE NOTICE 'Campo estado já existe';
    END IF;
END $$;

-- Mostrar estrutura atual da tabela
SELECT column_name, data_type, is_nullable, character_maximum_length
FROM information_schema.columns 
WHERE table_name = 'localidades'
ORDER BY ordinal_position;
