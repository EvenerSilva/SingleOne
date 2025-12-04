-- 🔍 Script para verificar dados na tabela de sinalizações de suspeitas

-- 1. Verificar se a tabela existe
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name = 'sinalizacoes_suspeitas';

-- 2. Verificar estrutura da tabela
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'sinalizacoes_suspeitas'
ORDER BY ordinal_position;

-- 3. Contar total de registros
SELECT 
    COUNT(*) as total_registros,
    COUNT(CASE WHEN status = 'pendente' THEN 1 END) as pendentes,
    COUNT(CASE WHEN status = 'em_investigacao' THEN 1 END) as em_investigacao,
    COUNT(CASE WHEN status = 'resolvida' THEN 1 END) as resolvidas,
    COUNT(CASE WHEN status = 'arquivada' THEN 1 END) as arquivadas
FROM sinalizacoes_suspeitas;

-- 4. Verificar registros mais recentes
SELECT 
    id,
    numero_protocolo,
    colaborador_nome,
    cpf_consultado,
    motivo_suspeita,
    status,
    prioridade,
    data_sinalizacao,
    vigilante_nome
FROM sinalizacoes_suspeitas 
ORDER BY data_sinalizacao DESC 
LIMIT 10;

-- 5. Verificar registros do último mês
SELECT 
    COUNT(*) as registros_ultimo_mes
FROM sinalizacoes_suspeitas 
WHERE data_sinalizacao >= CURRENT_DATE - INTERVAL '30 days';

-- 6. Verificar se há dados de teste
SELECT 
    'Tabela sinalizacoes_suspeitas' as tabela,
    COUNT(*) as total
FROM sinalizacoes_suspeitas
UNION ALL
SELECT 
    'Tabela motivos_suspeita' as tabela,
    COUNT(*) as total
FROM motivos_suspeita
UNION ALL
SELECT 
    'Tabela historico_investigacoes' as tabela,
    COUNT(*) as total
FROM historico_investigacoes;
