-- =====================================================
-- RESOLVER PROBLEMA DE PARAMETROS PARA CLIENTE 2
-- =====================================================

-- Problema identificado:
-- O usuário "Evener Silva" está no Cliente 2, mas não existe
-- configuração de parâmetros para este cliente, causando erro
-- na validação de 2FA inteligente.

-- Solução: Criar configuração padrão para Cliente 2

-- 1. Verificar se já existe configuração para Cliente 2
SELECT 
    cliente,
    two_factor_enabled,
    two_factor_type,
    two_factor_expiration_minutes,
    two_factor_max_attempts,
    two_factor_lockout_minutes
FROM parametros 
WHERE cliente = 2;

-- 2. Se não existir, criar configuração padrão
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

-- 3. Verificar se foi criado
SELECT 
    cliente,
    two_factor_enabled,
    two_factor_type,
    two_factor_expiration_minutes,
    two_factor_max_attempts,
    two_factor_lockout_minutes
FROM parametros 
WHERE cliente = 2;

-- 4. Verificar todas as configurações existentes
SELECT 
    cliente,
    two_factor_enabled,
    two_factor_type
FROM parametros 
ORDER BY cliente;

-- =====================================================
-- EXPLICAÇÃO DA SOLUÇÃO
-- =====================================================

-- O problema estava ocorrendo porque:
-- 1. Usuário Evener (Cliente 2) tentava salvar alterações
-- 2. Sistema detectava alteração de 2FA (TwoFactorEnabled: False)
-- 3. Validação buscava configuração global para Cliente 2
-- 4. Configuração não existia, causando erro 400
-- 5. Frontend interpretava como "Falha de comunicação"

-- Com esta configuração:
-- 1. Cliente 2 terá configuração padrão
-- 2. 2FA estará desabilitado por padrão
-- 3. Validação funcionará corretamente
-- 4. Usuários poderão salvar alterações normalmente
-- 5. 2FA poderá ser habilitado posteriormente se necessário

-- =====================================================
