-- Extrair todas as views do banco
\o views_extraidas.sql

SELECT 
    '-- View: ' || schemaname || '.' || viewname || E'\n' ||
    'DROP VIEW IF EXISTS ' || schemaname || '.' || viewname || ' CASCADE;' || E'\n' ||
    'CREATE OR REPLACE VIEW ' || schemaname || '.' || viewname || ' AS' || E'\n' ||
    pg_get_viewdef(schemaname || '.' || viewname, true) || ';' || E'\n'
FROM pg_views 
WHERE schemaname = 'public' 
ORDER BY viewname;

\o

