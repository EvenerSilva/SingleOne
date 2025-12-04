-- Script para adicionar as configurações de SMTP na tabela parametros existente
-- Execute este script no banco de dados para adicionar as novas colunas

-- Verificar se as colunas já existem antes de adicionar
DO $$
BEGIN
    -- Adicionar coluna email_descontos_enabled se não existir
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'email_descontos_enabled') THEN
        ALTER TABLE parametros ADD COLUMN email_descontos_enabled BOOLEAN DEFAULT false;
    END IF;
    
    -- Adicionar coluna smtp_enabled se não existir
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_enabled') THEN
        ALTER TABLE parametros ADD COLUMN smtp_enabled BOOLEAN DEFAULT false;
    END IF;
    
    -- Adicionar coluna smtp_host se não existir
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_host') THEN
        ALTER TABLE parametros ADD COLUMN smtp_host VARCHAR(200);
    END IF;
    
    -- Adicionar coluna smtp_port se não existir
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_port') THEN
        ALTER TABLE parametros ADD COLUMN smtp_port INTEGER;
    END IF;
    
    -- Adicionar coluna smtp_login se não existir
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_login') THEN
        ALTER TABLE parametros ADD COLUMN smtp_login VARCHAR(200);
    END IF;
    
    -- Adicionar coluna smtp_password se não existir
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_password') THEN
        ALTER TABLE parametros ADD COLUMN smtp_password VARCHAR(200);
    END IF;
    
    -- Adicionar coluna smtp_enable_ssl se não existir
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_enable_ssl') THEN
        ALTER TABLE parametros ADD COLUMN smtp_enable_ssl BOOLEAN DEFAULT false;
    END IF;
    
    -- Adicionar coluna smtp_email_from se não existir
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_email_from') THEN
        ALTER TABLE parametros ADD COLUMN smtp_email_from VARCHAR(200);
    END IF;
END $$;

-- Comentários para documentar as novas colunas
COMMENT ON COLUMN parametros.email_descontos_enabled IS 'Habilitar funcionalidade de e-mail para notificações de descontos de sinistros';
COMMENT ON COLUMN parametros.smtp_enabled IS 'Habilitar funcionalidade SMTP para envio de e-mails';
COMMENT ON COLUMN parametros.smtp_host IS 'Host SMTP do servidor de e-mail (ex: smtp.gmail.com, smtp-relay.brevo.com)';
COMMENT ON COLUMN parametros.smtp_port IS 'Porta SMTP do servidor de e-mail (ex: 587, 465)';
COMMENT ON COLUMN parametros.smtp_login IS 'Login/usuário SMTP do servidor de e-mail';
COMMENT ON COLUMN parametros.smtp_password IS 'Senha SMTP do servidor de e-mail';
COMMENT ON COLUMN parametros.smtp_enable_ssl IS 'Habilitar SSL para conexão SMTP';
COMMENT ON COLUMN parametros.smtp_email_from IS 'E-mail de origem para envio de e-mails';
