-- Script para adicionar as configurações de 2FA na tabela usuarios existente
-- Este script deve ser executado APÓS o script add-2fa-params.sql

-- Função para adicionar coluna se ela não existir
CREATE OR REPLACE FUNCTION add_column_if_not_exists(
    p_table_name text,
    p_column_name text,
    p_column_definition text
) RETURNS void AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = p_table_name AND column_name = p_column_name
    ) THEN
        EXECUTE format('ALTER TABLE %I ADD COLUMN %I %s', p_table_name, p_column_name, p_column_definition);
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Adicionar coluna para habilitar 2FA para o usuário
SELECT add_column_if_not_exists('usuarios', 'two_factor_enabled', 'BOOLEAN DEFAULT false');

-- Adicionar coluna para chave secreta 2FA (TOTP)
SELECT add_column_if_not_exists('usuarios', 'two_factor_secret', 'VARCHAR(255)');

-- Adicionar coluna para códigos de backup 2FA
SELECT add_column_if_not_exists('usuarios', 'two_factor_backup_codes', 'TEXT');

-- Adicionar coluna para último uso do 2FA
SELECT add_column_if_not_exists('usuarios', 'two_factor_last_used', 'TIMESTAMP');

-- Adicionar comentários para as novas colunas
COMMENT ON COLUMN usuarios.two_factor_enabled IS 'Habilitar autenticação de duplo fator (2FA) para este usuário específico';
COMMENT ON COLUMN usuarios.two_factor_secret IS 'Chave secreta para geração de códigos TOTP (Time-based One-Time Password)';
COMMENT ON COLUMN usuarios.two_factor_backup_codes IS 'Códigos de backup para recuperação de acesso 2FA (JSON array)';
COMMENT ON COLUMN usuarios.two_factor_last_used IS 'Data/hora do último uso do 2FA';

-- Remover a função auxiliar
DROP FUNCTION add_column_if_not_exists(text, text, text);

-- Inserir valores padrão para usuários existentes
UPDATE usuarios SET 
    two_factor_enabled = false
WHERE two_factor_enabled IS NULL;
