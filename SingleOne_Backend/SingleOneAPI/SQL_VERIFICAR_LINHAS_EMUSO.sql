-- ========================================
-- SCRIPT DE VERIFICAÇÃO E CORREÇÃO
-- ========================================

-- 1. VERIFICAR ESTRUTURA DA TABELA (incluindo defaults)
SELECT 
    column_name, 
    data_type, 
    column_default, 
    is_nullable
FROM information_schema.columns
WHERE table_name = 'telefonialinhas'
ORDER BY ordinal_position;

-- 2. VERIFICAR QUANTAS LINHAS ESTÃO COM EMUSO = TRUE
SELECT 
    COUNT(*) as total_linhas,
    SUM(CASE WHEN emuso = true THEN 1 ELSE 0 END) as em_uso,
    SUM(CASE WHEN emuso = false THEN 1 ELSE 0 END) as livres
FROM telefonialinhas;

-- 3. VERIFICAR SE HÁ TRIGGER NA TABELA
SELECT 
    trigger_name, 
    event_manipulation, 
    action_statement
FROM information_schema.triggers
WHERE event_object_table = 'telefonialinhas';

-- 4. CORRIGIR TODAS AS LINHAS IMPORTADAS HOJE (ajuste a data se necessário)
-- DESCOMENTE AS LINHAS ABAIXO SE QUISER CORRIGIR:
/*
UPDATE telefonialinhas
SET emuso = false
WHERE id IN (
    -- Selecionar IDs das linhas importadas recentemente
    -- Ajuste o filtro conforme necessário
    SELECT id FROM telefonialinhas
    ORDER BY id DESC
    LIMIT 4500
);
*/

-- 5. VERIFICAR RESULTADO
SELECT 
    COUNT(*) as total_linhas,
    SUM(CASE WHEN emuso = true THEN 1 ELSE 0 END) as em_uso,
    SUM(CASE WHEN emuso = false THEN 1 ELSE 0 END) as livres
FROM telefonialinhas;

