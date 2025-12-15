-- Script para adicionar colunas faltantes na tabela clientes
-- Executa: sudo -u postgres psql -d singleone -f corrigir_colunas_clientes.sql

-- Adicionar colunas se não existirem
ALTER TABLE clientes ADD COLUMN IF NOT EXISTS logo_bytes bytea;
ALTER TABLE clientes ADD COLUMN IF NOT EXISTS logo_content_type varchar(100);

-- Verificar se foram criadas
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'clientes' 
  AND column_name IN ('logo_bytes', 'logo_content_type');

