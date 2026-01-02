-- Script para adicionar coluna tipo_contestacao na tabela patrimonio_contestoes
-- Executa: sudo -u postgres psql -d singleone -f corrigir_coluna_tipo_contestacao.sql

-- Adicionar coluna se não existir
ALTER TABLE patrimonio_contestoes ADD COLUMN IF NOT EXISTS tipo_contestacao VARCHAR(50) DEFAULT 'contestacao';

-- Atualizar registros existentes sem tipo_contestacao
UPDATE patrimonio_contestoes 
SET tipo_contestacao = 'contestacao' 
WHERE tipo_contestacao IS NULL OR tipo_contestacao = '';

-- Verificar se foi criada
SELECT column_name, data_type, column_default 
FROM information_schema.columns 
WHERE table_name = 'patrimonio_contestoes' 
  AND column_name = 'tipo_contestacao';

