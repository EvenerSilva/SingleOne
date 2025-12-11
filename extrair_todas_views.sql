-- Extrair todas as views do banco
\o views_completas.sql

DO $$
DECLARE
    v_view_name TEXT;
    v_definition TEXT;
BEGIN
    FOR v_view_name IN 
        SELECT viewname 
        FROM pg_views 
        WHERE schemaname = 'public' 
        ORDER BY viewname
    LOOP
        BEGIN
            SELECT pg_get_viewdef('public.' || v_view_name, true) INTO v_definition;
            RAISE NOTICE '-- View: %', v_view_name;
            RAISE NOTICE 'DROP VIEW IF EXISTS public.% CASCADE;', v_view_name;
            RAISE NOTICE 'CREATE OR REPLACE VIEW public.% AS', v_view_name;
            RAISE NOTICE '%', v_definition;
            RAISE NOTICE ';';
            RAISE NOTICE '';
        EXCEPTION WHEN OTHERS THEN
            RAISE NOTICE '-- ERRO ao extrair view %: %', v_view_name, SQLERRM;
        END;
    END LOOP;
END $$;

\o

