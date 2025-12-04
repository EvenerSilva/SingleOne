-- COMANDO SIMPLES PARA CLIENTE 2
-- Execute este comando no seu banco PostgreSQL

-- 1. Verificar se já existe:
SELECT cliente, two_factor_enabled FROM parametros WHERE cliente = 2;

-- 2. Se não retornar nada, execute este INSERT simples:
INSERT INTO parametros (cliente, two_factor_enabled) VALUES (2, false);

-- 3. Verificar se foi criado:
SELECT cliente, two_factor_enabled FROM parametros WHERE cliente = 2;
