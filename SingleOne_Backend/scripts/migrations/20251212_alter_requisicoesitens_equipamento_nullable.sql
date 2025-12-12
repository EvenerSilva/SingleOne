-- Migration: Permitir NULL na coluna equipamento de requisicoesitens
-- Data: 2025-12-12
-- Motivo: Requisições de linhas telefônicas não têm equipamento, apenas linhatelefonica
-- Autor: Sistema

-- ✅ IMPORTANTE: Esta alteração DEVE ser aplicada no banco de produção
-- ✅ E incluída em qualquer script de criação do zero do banco

BEGIN;

-- Permitir NULL na coluna equipamento
ALTER TABLE requisicoesitens 
ALTER COLUMN equipamento DROP NOT NULL;

-- Verificar a mudança
SELECT 
    'requisicoesitens.equipamento' as coluna,
    is_nullable 
FROM information_schema.columns 
WHERE table_name = 'requisicoesitens' 
AND column_name = 'equipamento';

COMMIT;

-- Nota: Ao criar o banco do zero, a tabela deve ser criada assim:
-- CREATE TABLE requisicoesitens (
--     id SERIAL PRIMARY KEY,
--     requisicao INTEGER NOT NULL REFERENCES requisicoes(id),
--     equipamento INTEGER NULL,  -- ✅ NULL para suportar linhas telefônicas
--     linhatelefonica INTEGER NULL,
--     ...
-- );

