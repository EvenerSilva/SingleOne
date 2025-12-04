-- Script para verificar a tabela tinone_config e seus dados
-- Execute no PostgreSQL/pgAdmin

-- 1. Verificar se a tabela existe
SELECT EXISTS (
   SELECT FROM information_schema.tables 
   WHERE  table_schema = 'public'
   AND    table_name   = 'tinone_config'
);

-- 2. Ver estrutura da tabela
SELECT 
    column_name, 
    data_type, 
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'tinone_config'
ORDER BY ordinal_position;

-- 3. Ver todos os registros da tabela
SELECT 
    id,
    cliente,
    chave,
    valor,
    descricao,
    ativo,
    created_at,
    updated_at
FROM tinone_config
ORDER BY id;

-- 4. Contar registros
SELECT COUNT(*) as total_configuracoes FROM tinone_config;

-- 5. Ver apenas configurações ativas
SELECT 
    id,
    chave,
    valor,
    ativo,
    updated_at
FROM tinone_config
WHERE ativo = true
ORDER BY chave;

-- 6. Se a tabela não existir, criar com o script abaixo:
/*
CREATE TABLE IF NOT EXISTS tinone_config (
    id SERIAL PRIMARY KEY,
    cliente INTEGER NULL,
    chave VARCHAR(100) NOT NULL,
    valor TEXT NULL,
    descricao TEXT NULL,
    ativo BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NULL,
    CONSTRAINT uk_tinone_config_cliente_chave UNIQUE (cliente, chave)
);

-- Criar índice para melhor performance
CREATE INDEX IF NOT EXISTS idx_tinone_config_chave ON tinone_config(chave);
CREATE INDEX IF NOT EXISTS idx_tinone_config_cliente ON tinone_config(cliente);

-- Inserir configurações padrão
INSERT INTO tinone_config (cliente, chave, valor, descricao, ativo) VALUES
(NULL, 'TINONE_HABILITADO', 'true', 'Habilita/desabilita o assistente TinOne globalmente', true),
(NULL, 'TINONE_CHAT_HABILITADO', 'true', 'Habilita funcionalidade de chat do TinOne', true),
(NULL, 'TINONE_TOOLTIPS_HABILITADO', 'true', 'Habilita tooltips contextuais nos campos', true),
(NULL, 'TINONE_GUIAS_HABILITADO', 'false', 'Habilita guias passo-a-passo (em desenvolvimento)', true),
(NULL, 'TINONE_SUGESTOES_PROATIVAS', 'false', 'Habilita sugestões proativas (beta)', true),
(NULL, 'TINONE_IA_HABILITADA', 'false', 'Habilita processamento com IA/NLP (requer Ollama local)', true),
(NULL, 'TINONE_ANALYTICS', 'true', 'Habilita coleta de analytics de uso do TinOne', true),
(NULL, 'TINONE_DEBUG_MODE', 'false', 'Modo debug para desenvolvimento do TinOne', true),
(NULL, 'TINONE_POSICAO', 'bottom-right', 'Posição do widget: bottom-right, bottom-left', true),
(NULL, 'TINONE_COR_PRIMARIA', '#4a90e2', 'Cor primária do TinOne (hex)', true)
ON CONFLICT (cliente, chave) DO NOTHING;
*/

