-- =====================================================
-- SCRIPT PARA ALTERAR TABELA LOCALIDADES (PostgreSQL)
-- Adicionar campos cidade e estado
-- =====================================================

-- Verificar se a tabela existe
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'localidades') THEN
        RAISE EXCEPTION 'Tabela localidades não encontrada! Verifique o nome correto da tabela no seu banco de dados.';
    END IF;
END $$;

-- Verificar se o campo cidade já existe
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.columns WHERE table_name = 'localidades' AND column_name = 'cidade') THEN
        RAISE NOTICE 'Campo "cidade" já existe na tabela localidades';
    ELSE
        -- Adicionar campo cidade
        ALTER TABLE localidades ADD COLUMN cidade VARCHAR(100);
        RAISE NOTICE 'Campo "cidade" adicionado com sucesso!';
    END IF;
END $$;

-- Verificar se o campo estado já existe
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.columns WHERE table_name = 'localidades' AND column_name = 'estado') THEN
        RAISE NOTICE 'Campo "estado" já existe na tabela localidades';
    ELSE
        -- Adicionar campo estado
        ALTER TABLE localidades ADD COLUMN estado VARCHAR(50);
        RAISE NOTICE 'Campo "estado" adicionado com sucesso!';
    END IF;
END $$;

-- Verificar estrutura final da tabela
SELECT 
    column_name as "Campo",
    data_type as "Tipo",
    is_nullable as "Permite_Nulo",
    character_maximum_length as "Tamanho_Max"
FROM information_schema.columns 
WHERE table_name = 'localidades'
ORDER BY ordinal_position;
