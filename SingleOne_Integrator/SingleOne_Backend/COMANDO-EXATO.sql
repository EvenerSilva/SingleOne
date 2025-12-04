-- COMANDO EXATO PARA EXECUTAR NO SEU BANCO POSTGRESQL
-- Copie e cole este comando no seu cliente PostgreSQL (pgAdmin, DBeaver, etc.)

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
) ON CONFLICT (cliente) DO NOTHING;

-- DEPOIS execute este comando para verificar:
SELECT cliente, two_factor_enabled FROM parametros WHERE cliente = 2;
