-- HABILITAR 2FA GLOBALMENTE PARA CLIENTE 2
-- Execute este comando no seu banco PostgreSQL

-- 1. Verificar configuração atual:
SELECT cliente, two_factor_enabled FROM parametros WHERE cliente = 2;

-- 2. HABILITAR 2FA globalmente:
UPDATE parametros SET two_factor_enabled = true WHERE cliente = 2;

-- 3. Verificar se foi alterado:
SELECT cliente, two_factor_enabled FROM parametros WHERE cliente = 2;

-- Resultado esperado:
-- cliente | two_factor_enabled
-- --------+-------------------
--    2    |      true
