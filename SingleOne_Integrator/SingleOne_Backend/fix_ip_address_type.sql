-- =====================================================
-- CORREÇÃO DO TIPO DE DADOS DA COLUNA IP_ADDRESS
-- =====================================================
-- Script para corrigir o tipo de dados da coluna ip_address
-- de inet para VARCHAR(45) para compatibilidade com Entity Framework

-- Verificar o tipo atual da coluna
SELECT 
    column_name, 
    data_type, 
    character_maximum_length
FROM information_schema.columns 
WHERE table_name = 'sinalizacoes_suspeitas' 
AND column_name = 'ip_address';

-- Alterar o tipo da coluna de inet para VARCHAR(45)
ALTER TABLE sinalizacoes_suspeitas 
ALTER COLUMN ip_address TYPE VARCHAR(45) USING ip_address::VARCHAR(45);

-- Verificar se a alteração foi aplicada
SELECT 
    column_name, 
    data_type, 
    character_maximum_length
FROM information_schema.columns 
WHERE table_name = 'sinalizacoes_suspeitas' 
AND column_name = 'ip_address';

-- Verificar se existem dados na tabela
SELECT COUNT(*) as total_registros FROM sinalizacoes_suspeitas;

-- Comentário da coluna
COMMENT ON COLUMN sinalizacoes_suspeitas.ip_address IS 'Endereço IP do usuário que fez a sinalização (suporta IPv4 e IPv6)';
