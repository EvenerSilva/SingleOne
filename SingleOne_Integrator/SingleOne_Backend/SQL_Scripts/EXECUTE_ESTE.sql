-- ======================================================
-- 🚀 SCRIPT RÁPIDO: Execute no pgAdmin ou DBeaver
-- ======================================================
-- Este script irá:
-- 1. Mostrar todos os usuários
-- 2. Ativar TODOS os usuários
-- 3. Confirmar o resultado
-- ======================================================

-- PASSO 1: Ver status atual
SELECT 
    id,
    nome,
    email,
    ativo,
    CASE WHEN ativo THEN '✅ ATIVO' ELSE '❌ INATIVO' END as status
FROM public.usuarios
ORDER BY id;

-- PASSO 2: ATIVAR TODOS
UPDATE public.usuarios 
SET ativo = true;

-- PASSO 3: Confirmar
SELECT 
    COUNT(*) as total_ativos,
    '✅ SUCESSO! Agora reinicie o backend!' as mensagem
FROM public.usuarios 
WHERE ativo = true;

-- ⚠️ IMPORTANTE: REINICIE O BACKEND APÓS EXECUTAR!

