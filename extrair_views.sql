-- Script para extrair definições de views do banco
-- Execute: psql -U postgres -d singleone -f extrair_views.sql > views_definicoes.txt

SELECT 
    '-- View: ' || table_name || E'\n' ||
    'DROP VIEW IF EXISTS ' || table_name || ' CASCADE;' || E'\n' ||
    pg_get_viewdef('public.' || table_name, true) || ';' || E'\n'
FROM information_schema.views 
WHERE table_schema = 'public' 
ORDER BY table_name;

