-- Verificar se a IA está habilitada no SingleOne
SELECT 
    chave, 
    valor, 
    ativo, 
    descricao,
    updated_at
FROM tinone_config 
WHERE chave = 'TINONE_IA_HABILITADA'
ORDER BY updated_at DESC;

-- Ver TODAS as configurações do TinOne
SELECT 
    chave, 
    valor, 
    ativo
FROM tinone_config 
ORDER BY chave;

