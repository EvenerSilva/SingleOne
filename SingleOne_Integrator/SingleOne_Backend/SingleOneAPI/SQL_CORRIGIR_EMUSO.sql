-- ========================================
-- SCRIPT PARA CORRIGIR LINHAS COM EMUSO = TRUE
-- ========================================

-- 1. BACKUP: Criar tabela temporária com o estado atual
CREATE TEMP TABLE backup_linhas_emuso AS
SELECT id, numero, emuso, ativo
FROM telefonialinhas
WHERE emuso = true;

SELECT COUNT(*) as linhas_backup FROM backup_linhas_emuso;

-- 2. CORRIGIR TODAS AS LINHAS IMPORTADAS (últimas 4500)
UPDATE telefonialinhas
SET emuso = false
WHERE id IN (
    SELECT id 
    FROM telefonialinhas 
    ORDER BY id DESC 
    LIMIT 4500
);

-- 3. VERIFICAR QUANTAS FORAM CORRIGIDAS
SELECT 
    COUNT(*) as total_linhas,
    SUM(CASE WHEN emuso = true THEN 1 ELSE 0 END) as em_uso,
    SUM(CASE WHEN emuso = false THEN 1 ELSE 0 END) as livres
FROM telefonialinhas;

-- 4. REMOVER DEFAULT VALUE DO BANCO (OPCIONAL - RECOMENDADO!)
-- DESCOMENTE SE QUISER PREVENIR O PROBLEMA NO FUTURO:
/*
ALTER TABLE telefonialinhas 
ALTER COLUMN emuso SET DEFAULT false;
*/

-- 5. VERIFICAR O DEFAULT VALUE ATUAL
SELECT 
    column_name, 
    column_default
FROM information_schema.columns
WHERE table_name = 'telefonialinhas' 
  AND column_name = 'emuso';

