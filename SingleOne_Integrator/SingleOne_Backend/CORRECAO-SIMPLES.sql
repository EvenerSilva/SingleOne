-- CORREÇÃO SIMPLES PARA CLIENTE 2
-- Execute este comando no seu banco PostgreSQL

-- 1. Primeiro, verificar se já existe configuração para Cliente 2
SELECT cliente, two_factor_enabled FROM parametros WHERE cliente = 2;

-- 2. Se não retornar nada, execute este INSERT:
INSERT INTO parametros (
    cliente,
    two_factor_enabled,
    two_factor_type,
    two_factor_expiration_minutes,
    two_factor_max_attempts,
    two_factor_lockout_minutes,
    two_factor_email_template
) VALUES (
    2,                           -- Cliente 2
    false,                       -- 2FA desabilitado por padrão
    'email',                     -- Tipo de 2FA
    5,                           -- Expiração em minutos
    3,                           -- Máximo de tentativas
    15,                          -- Bloqueio em minutos
    'Código de verificação: {code}' -- Template de email
);

-- 3. Verificar se foi criado:
SELECT cliente, two_factor_enabled FROM parametros WHERE cliente = 2;
