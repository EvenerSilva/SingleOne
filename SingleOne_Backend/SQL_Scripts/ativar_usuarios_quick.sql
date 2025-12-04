-- ========================================
-- Script Rápido: Ativar Todos os Usuários
-- PostgreSQL Version
-- ========================================

-- Etapa 1: Ver usuários atuais
SELECT 
    id,
    nome,
    email,
    ativo,
    CASE WHEN ativo = true THEN '✅' ELSE '❌' END AS status
FROM public.usuarios
ORDER BY id;

-- Etapa 2: DESCOMENTE A LINHA ABAIXO para ativar TODOS os usuários
-- UPDATE public.usuarios SET ativo = true;

-- Etapa 3: Verificar resultado
-- SELECT COUNT(*) as total_ativos FROM public.usuarios WHERE ativo = true;
