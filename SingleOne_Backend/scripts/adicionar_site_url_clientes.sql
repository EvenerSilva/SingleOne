-- ========================================
-- Script para adicionar campo site_url na tabela clientes
-- Permite que cada cliente tenha sua própria URL
-- Execute: sudo -u postgres psql -d singleone -f adicionar_site_url_clientes.sql
-- ========================================

-- Adicionar coluna site_url se não existir
ALTER TABLE clientes ADD COLUMN IF NOT EXISTS site_url VARCHAR(500);

-- Comentário na coluna
COMMENT ON COLUMN clientes.site_url IS 'URL do site do cliente (ex: https://demo.singleone.com.br). Se não preenchido, usa a URL padrão do sistema.';

-- Atualizar cliente demo (ID 1) com a URL do domínio
UPDATE clientes 
SET site_url = 'https://demo.singleone.com.br' 
WHERE id = 1 AND (site_url IS NULL OR site_url = '');

-- Mostrar resultado
SELECT id, razaosocial, site_url 
FROM clientes 
ORDER BY id;

