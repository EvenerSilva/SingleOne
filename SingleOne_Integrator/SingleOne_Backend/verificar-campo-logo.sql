-- =====================================================
-- SCRIPT PARA VERIFICAR SE O CAMPO LOGO FOI CRIADO
-- =====================================================

-- Verificar se a coluna logo existe na tabela clientes
SELECT 
    column_name, 
    data_type, 
    is_nullable,
    column_default,
    character_maximum_length
FROM information_schema.columns 
WHERE table_name = 'clientes' AND column_name = 'logo';

-- Verificar estrutura atual da tabela clientes
SELECT column_name, data_type, is_nullable
FROM information_schema.columns 
WHERE table_name = 'clientes'
ORDER BY ordinal_position;

-- Verificar se há dados na tabela
SELECT id, razaosocial, logo, ativo
FROM clientes
LIMIT 5;
