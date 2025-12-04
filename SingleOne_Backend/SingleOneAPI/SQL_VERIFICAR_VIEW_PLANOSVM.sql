-- ========================================
-- VERIFICAR A VIEW PLANOSVM
-- ========================================

-- 1. VER A DEFINIÇÃO DA VIEW
SELECT 
    schemaname,
    viewname,
    definition
FROM pg_views
WHERE viewname LIKE '%planos%'
ORDER BY viewname;

-- 2. LISTAR TODAS AS VIEWS
SELECT 
    schemaname,
    viewname
FROM pg_views
WHERE schemaname = 'public'
ORDER BY viewname;

