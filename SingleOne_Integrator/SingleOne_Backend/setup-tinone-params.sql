-- ============================================================
-- SETUP TinOne - Parâmetros de Configuração
-- ============================================================
-- Script para adicionar parâmetros do assistente TinOne
-- Pode ser desabilitado a qualquer momento sem impactar o sistema
-- ============================================================

-- Parâmetros globais do TinOne (cliente = NULL aplica para todos)
INSERT INTO parametros (cliente, chave, valor, descricao, ativo) VALUES 
-- Controle principal
(NULL, 'TINONE_HABILITADO', 'true', 'Habilita/desabilita o assistente TinOne globalmente', true),

-- Funcionalidades específicas
(NULL, 'TINONE_CHAT_HABILITADO', 'true', 'Habilita funcionalidade de chat do TinOne', true),
(NULL, 'TINONE_TOOLTIPS_HABILITADO', 'true', 'Habilita tooltips contextuais nos campos', true),
(NULL, 'TINONE_GUIAS_HABILITADO', 'false', 'Habilita guias passo-a-passo (em desenvolvimento)', true),
(NULL, 'TINONE_SUGESTOES_PROATIVAS', 'false', 'Habilita sugestões proativas (beta)', true),

-- IA/NLP (desabilitado por padrão - requer Ollama configurado)
(NULL, 'TINONE_IA_HABILITADA', 'false', 'Habilita processamento com IA/NLP (requer Ollama local)', true),

-- Analytics e Debug
(NULL, 'TINONE_ANALYTICS', 'true', 'Habilita coleta de analytics de uso do TinOne', true),
(NULL, 'TINONE_DEBUG_MODE', 'false', 'Modo debug para desenvolvimento do TinOne', true),

-- Configurações visuais
(NULL, 'TINONE_POSICAO', 'bottom-right', 'Posição do widget: bottom-right, bottom-left', true),
(NULL, 'TINONE_COR_PRIMARIA', '#4a90e2', 'Cor primária do TinOne (hex)', true)

ON CONFLICT DO NOTHING;

-- ============================================================
-- Exemplo: Desabilitar TinOne para um cliente específico
-- ============================================================
-- Descomente a linha abaixo para desabilitar para o cliente 1:
-- INSERT INTO parametros (cliente, chave, valor, descricao, ativo) VALUES 
-- (1, 'TINONE_HABILITADO', 'false', 'TinOne desabilitado para este cliente', true);

-- ============================================================
-- Verificar parâmetros inseridos
-- ============================================================
-- SELECT * FROM parametros WHERE chave LIKE 'TINONE_%' ORDER BY chave;

-- ============================================================
-- DESABILITAR RAPIDAMENTE (se necessário)
-- ============================================================
-- UPDATE parametros SET valor = 'false' WHERE chave = 'TINONE_HABILITADO';

-- ============================================================
-- REMOVER TinOne COMPLETAMENTE (rollback)
-- ============================================================
-- DELETE FROM parametros WHERE chave LIKE 'TINONE_%';
-- DROP TABLE IF EXISTS tinone_analytics CASCADE;

