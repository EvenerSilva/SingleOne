-- ========================================
-- VERIFICAR STATUS ATUAL DAS LINHAS
-- ========================================

-- 1. CONTAR LINHAS POR STATUS
SELECT 
    COUNT(*) as total_linhas,
    SUM(CASE WHEN emuso = true THEN 1 ELSE 0 END) as em_uso,
    SUM(CASE WHEN emuso = false THEN 1 ELSE 0 END) as livres,
    SUM(CASE WHEN emuso IS NULL THEN 1 ELSE 0 END) as nulos
FROM telefonialinhas;

-- 2. VERIFICAR DEFAULT VALUE DA COLUNA
SELECT 
    column_name, 
    column_default,
    is_nullable
FROM information_schema.columns
WHERE table_name = 'telefonialinhas' 
  AND column_name = 'emuso';

-- 3. VERIFICAR SE HÁ TRIGGER
SELECT 
    trigger_name, 
    event_manipulation, 
    action_statement
FROM information_schema.triggers
WHERE event_object_table = 'telefonialinhas';

-- 4. VERIFICAR ÚLTIMAS 10 LINHAS CRIADAS
SELECT 
    id, 
    numero, 
    emuso, 
    ativo
FROM telefonialinhas
ORDER BY id DESC
LIMIT 10;

-- 5. ATUALIZAR **TODAS** AS LINHAS (não apenas últimas 4500)
UPDATE telefonialinhas
SET emuso = false;

-- 6. VERIFICAR RESULTADO
SELECT 
    COUNT(*) as total_linhas,
    SUM(CASE WHEN emuso = true THEN 1 ELSE 0 END) as em_uso,
    SUM(CASE WHEN emuso = false THEN 1 ELSE 0 END) as livres
FROM telefonialinhas;

-- 7. ALTERAR DEFAULT VALUE PARA false
ALTER TABLE telefonialinhas 
ALTER COLUMN emuso SET DEFAULT false;

-- 8. CONFIRMAR ALTERAÇÃO
SELECT 
    column_name, 
    column_default
FROM information_schema.columns
WHERE table_name = 'telefonialinhas' 
  AND column_name = 'emuso';

