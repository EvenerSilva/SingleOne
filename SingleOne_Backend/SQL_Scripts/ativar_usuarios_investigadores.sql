-- =====================================================
-- Script: Ativar Usuários como Investigadores
-- PostgreSQL Version
-- Descrição: Verifica e ativa usuários no sistema para
--            que possam ser atribuídos como investigadores
-- =====================================================

-- 1. VERIFICAR USUÁRIOS EXISTENTES
DO $$ 
BEGIN
    RAISE NOTICE '=========================================';
    RAISE NOTICE '1. VERIFICANDO USUÁRIOS NO SISTEMA';
    RAISE NOTICE '=========================================';
END $$;

SELECT 
    id,
    nome,
    email,
    ativo,
    adm,
    operador,
    CASE 
        WHEN ativo = true THEN '✅ ATIVO'
        ELSE '❌ INATIVO'
    END AS status
FROM public.usuarios
ORDER BY ativo DESC NULLS LAST, nome;

DO $$ 
DECLARE
    total_usuarios INTEGER;
    usuarios_ativos INTEGER;
    usuarios_inativos INTEGER;
BEGIN
    SELECT COUNT(*) INTO total_usuarios FROM public.usuarios;
    SELECT COUNT(*) INTO usuarios_ativos FROM public.usuarios WHERE ativo = true;
    SELECT COUNT(*) INTO usuarios_inativos FROM public.usuarios WHERE ativo = false OR ativo IS NULL;
    
    RAISE NOTICE '';
    RAISE NOTICE 'Total de usuários: %', total_usuarios;
    RAISE NOTICE 'Usuários ativos: %', usuarios_ativos;
    RAISE NOTICE 'Usuários inativos: %', usuarios_inativos;
    RAISE NOTICE '';
END $$;

-- 2. ATIVAR TODOS OS USUÁRIOS (SE NECESSÁRIO)
-- Descomente as linhas abaixo para ATIVAR TODOS os usuários
/*
DO $$ 
DECLARE
    usuarios_ativados INTEGER;
BEGIN
    RAISE NOTICE '=========================================';
    RAISE NOTICE '2. ATIVANDO TODOS OS USUÁRIOS';
    RAISE NOTICE '=========================================';
    
    UPDATE public.usuarios 
    SET ativo = true
    WHERE ativo = false OR ativo IS NULL;
    
    GET DIAGNOSTICS usuarios_ativados = ROW_COUNT;
    RAISE NOTICE 'Usuários ativados: %', usuarios_ativados;
END $$;
*/

-- 3. ATIVAR USUÁRIOS ESPECÍFICOS (RECOMENDADO)
-- Descomente e modifique os IDs conforme necessário
/*
DO $$ 
DECLARE
    usuarios_ativados INTEGER;
BEGIN
    RAISE NOTICE '=========================================';
    RAISE NOTICE '3. ATIVANDO USUÁRIOS ESPECÍFICOS';
    RAISE NOTICE '=========================================';
    
    UPDATE public.usuarios 
    SET ativo = true
    WHERE id IN (1, 2, 3, 4, 5); -- Modifique os IDs conforme necessário
    
    GET DIAGNOSTICS usuarios_ativados = ROW_COUNT;
    RAISE NOTICE 'Usuários ativados: %', usuarios_ativados;
END $$;
*/

-- 4. ATIVAR APENAS ADMINISTRADORES
-- Descomente para ativar apenas usuários administradores
/*
DO $$ 
DECLARE
    usuarios_ativados INTEGER;
BEGIN
    RAISE NOTICE '=========================================';
    RAISE NOTICE '4. ATIVANDO APENAS ADMINISTRADORES';
    RAISE NOTICE '=========================================';
    
    UPDATE public.usuarios 
    SET ativo = true
    WHERE adm = true AND (ativo = false OR ativo IS NULL);
    
    GET DIAGNOSTICS usuarios_ativados = ROW_COUNT;
    RAISE NOTICE 'Administradores ativados: %', usuarios_ativados;
END $$;
*/

-- 5. VERIFICAR RESULTADO FINAL
DO $$ 
BEGIN
    RAISE NOTICE '';
    RAISE NOTICE '=========================================';
    RAISE NOTICE '5. RESULTADO FINAL';
    RAISE NOTICE '=========================================';
END $$;

SELECT 
    id,
    nome,
    email,
    ativo,
    adm,
    operador,
    CASE 
        WHEN ativo = true THEN '✅ DISPONÍVEL COMO INVESTIGADOR'
        ELSE '❌ NÃO DISPONÍVEL'
    END AS status_investigador
FROM public.usuarios
WHERE ativo = true
ORDER BY nome;

DO $$ 
DECLARE
    total_investigadores INTEGER;
BEGIN
    SELECT COUNT(*) INTO total_investigadores FROM public.usuarios WHERE ativo = true;
    
    RAISE NOTICE '';
    RAISE NOTICE '✅ Total de investigadores disponíveis: %', total_investigadores;
    RAISE NOTICE '';
    RAISE NOTICE '=========================================';
    RAISE NOTICE 'SCRIPT CONCLUÍDO';
    RAISE NOTICE '=========================================';
    RAISE NOTICE '';
    RAISE NOTICE '⚠️ IMPORTANTE: REINICIE A API DO BACKEND';
    RAISE NOTICE 'Para aplicar as mudanças!';
    RAISE NOTICE '=========================================';
END $$;
