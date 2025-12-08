-- Script para consultar a senha SMTP configurada
-- Execute: psql -h localhost -U postgres -d singleone -f consultar_senha_smtp.sql

SELECT 
    id,
    cliente,
    smtp_enabled,
    smtp_host,
    smtp_port,
    smtp_login,
    smtp_password,  -- Senha SMTP em texto plano
    smtp_email_from,
    smtp_enable_ssl
FROM parametros
WHERE smtp_enabled = true
ORDER BY cliente;

