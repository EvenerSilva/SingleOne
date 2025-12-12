-- Script para corrigir o tipo das colunas ip_address de inet para varchar(45)
-- Isso resolve o problema de conversão automática do Entity Framework

-- 1. Tabela sinalizacoes_suspeitas
ALTER TABLE sinalizacoes_suspeitas 
    ALTER COLUMN ip_address TYPE varchar(45) USING ip_address::text;

-- 2. Tabela geolocalizacao_assinatura
ALTER TABLE geolocalizacao_assinatura 
    ALTER COLUMN ip_address TYPE varchar(45) USING ip_address::text;

-- 3. Tabela patrimonio_logs_acesso
ALTER TABLE patrimonio_logs_acesso 
    ALTER COLUMN ip_address TYPE varchar(45) USING ip_address::text;

-- Verificar as alterações
SELECT 
    table_name, 
    column_name, 
    data_type 
FROM information_schema.columns 
WHERE column_name = 'ip_address' 
    AND table_name IN ('sinalizacoes_suspeitas', 'geolocalizacao_assinatura', 'patrimonio_logs_acesso')
ORDER BY table_name;

