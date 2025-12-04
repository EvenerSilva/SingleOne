-- Script para verificar a view vwplanostelefonia
-- Esta view é necessária para o funcionamento da API de planos

-- Verificar se a view existe
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name = 'vwplanostelefonia';

-- Verificar estrutura da view
SELECT 
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'vwplanostelefonia'
ORDER BY ordinal_position;

-- Verificar dados da view
SELECT * FROM vwplanostelefonia LIMIT 5;

-- Verificar se os campos de contagem estão funcionando
SELECT 
    plano,
    contlinhas,
    contlinhasemuso,
    contlinhaslivres
FROM vwplanostelefonia 
LIMIT 10;
