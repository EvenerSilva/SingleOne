-- Script para adicionar as configurações de 2FA na tabela parametros existente
-- Este script deve ser executado APÓS o script add-brevo-params.sql

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

-- Adicionar coluna para habilitar 2FA
SELECT add_column_if_not_exists('parametros', 'two_factor_enabled', 'BOOLEAN DEFAULT false');

-- Adicionar coluna para tipo de 2FA (email, app, ambos)
SELECT add_column_if_not_exists('parametros', 'two_factor_type', 'VARCHAR(20) DEFAULT ''email''');

-- Adicionar coluna para tempo de expiração do código 2FA (em minutos)
SELECT add_column_if_not_exists('parametros', 'two_factor_expiration_minutes', 'INTEGER DEFAULT 5');

-- Adicionar coluna para número máximo de tentativas 2FA
SELECT add_column_if_not_exists('parametros', 'two_factor_max_attempts', 'INTEGER DEFAULT 3');

-- Adicionar coluna para tempo de bloqueio após falhas (em minutos)
SELECT add_column_if_not_exists('parametros', 'two_factor_lockout_minutes', 'INTEGER DEFAULT 15');

-- Adicionar coluna para template de email 2FA
SELECT add_column_if_not_exists('parametros', 'two_factor_email_template', 'TEXT DEFAULT ''Seu código de verificação é: {CODE}. Este código expira em {EXPIRATION} minutos.''');

-- Adicionar comentários para as novas colunas
COMMENT ON COLUMN parametros.two_factor_enabled IS 'Habilitar autenticação de duplo fator (2FA) para usuários do sistema';
COMMENT ON COLUMN parametros.two_factor_type IS 'Tipo de 2FA: email, app, ambos (email+app)';
COMMENT ON COLUMN parametros.two_factor_expiration_minutes IS 'Tempo de expiração do código 2FA em minutos';
COMMENT ON COLUMN parametros.two_factor_max_attempts IS 'Número máximo de tentativas de 2FA antes do bloqueio';
COMMENT ON COLUMN parametros.two_factor_lockout_minutes IS 'Tempo de bloqueio após exceder tentativas máximas (em minutos)';
COMMENT ON COLUMN parametros.two_factor_email_template IS 'Template do email para envio do código 2FA. Use {CODE} para o código e {EXPIRATION} para o tempo de expiração';

-- Remover a função auxiliar
DROP FUNCTION add_column_if_not_exists(text, text, text);

-- Inserir valores padrão para clientes existentes (se necessário)
-- UPDATE parametros SET 
--     two_factor_enabled = false,
--     two_factor_type = 'email',
--     two_factor_expiration_minutes = 5,
--     two_factor_max_attempts = 3,
--     two_factor_lockout_minutes = 15,
--     two_factor_email_template = 'Seu código de verificação é: {CODE}. Este código expira em {EXPIRATION} minutos.'
-- WHERE two_factor_enabled IS NULL;

