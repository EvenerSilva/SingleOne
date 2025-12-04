-- Script para adicionar colunas de 2FA por email que estão faltando na tabela usuarios
-- Execute este script no seu banco PostgreSQL

-- Adicionar coluna para código de email 2FA (temporário)
ALTER TABLE "usuarios" ADD COLUMN "two_factor_email_code" VARCHAR(10);

-- Adicionar coluna para expiração do código de email 2FA
ALTER TABLE "usuarios" ADD COLUMN "two_factor_email_code_expiry" TIMESTAMP;

-- Verificar se as colunas foram criadas
SELECT column_name, data_type FROM information_schema.columns 
WHERE table_name = 'usuarios' 
AND column_name IN ('two_factor_email_code', 'two_factor_email_code_expiry')
ORDER BY column_name;

-- Verificar todas as colunas de 2FA existentes
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'usuarios' 
AND column_name LIKE 'two_factor%'
ORDER BY column_name;
