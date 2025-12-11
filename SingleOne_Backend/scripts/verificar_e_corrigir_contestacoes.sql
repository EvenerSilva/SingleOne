-- ========================================
-- Script para verificar e corrigir tabela de contestações
-- Execute no servidor: sudo -u postgres psql -d singleone -f verificar_e_corrigir_contestacoes.sql
-- ========================================

-- Verificar estrutura da tabela
SELECT 
    column_name, 
    data_type, 
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'patrimonio_contestoes'
ORDER BY ordinal_position;

-- Verificar se a coluna tipo_contestacao existe
SELECT EXISTS (
    SELECT 1 
    FROM information_schema.columns 
    WHERE table_name = 'patrimonio_contestoes' 
    AND column_name = 'tipo_contestacao'
) AS coluna_existe;

-- Se a coluna não existir, criar
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'patrimonio_contestoes' 
        AND column_name = 'tipo_contestacao'
    ) THEN
        ALTER TABLE patrimonio_contestoes 
        ADD COLUMN tipo_contestacao VARCHAR(50) DEFAULT 'contestacao';
        
        RAISE NOTICE 'Coluna tipo_contestacao criada com sucesso';
    ELSE
        RAISE NOTICE 'Coluna tipo_contestacao já existe';
    END IF;
END $$;

-- Atualizar registros existentes sem tipo_contestacao
UPDATE patrimonio_contestoes 
SET tipo_contestacao = 'contestacao' 
WHERE tipo_contestacao IS NULL OR tipo_contestacao = '';

-- Verificar resultado
SELECT COUNT(*) AS total_registros,
       COUNT(CASE WHEN tipo_contestacao IS NOT NULL THEN 1 END) AS com_tipo_contestacao,
       COUNT(DISTINCT tipo_contestacao) AS tipos_distintos
FROM patrimonio_contestoes;

