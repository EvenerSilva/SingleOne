-- =====================================================
-- Script de criação das tabelas de Integração de Folha
-- SingleOne Integrator
-- =====================================================

-- Tabela: ClienteIntegracao
-- Armazena as configurações de integração de cada cliente
CREATE TABLE IF NOT EXISTS "ClienteIntegracao" (
    "Id" SERIAL PRIMARY KEY,
    "ClienteId" INT NOT NULL,
    "ApiKey" VARCHAR(100) NOT NULL UNIQUE,
    "ApiSecret" VARCHAR(100) NOT NULL,
    "IpWhitelist" VARCHAR(500),
    "WebhookUrl" VARCHAR(500),
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "DataCriacao" TIMESTAMP NOT NULL DEFAULT NOW(),
    "DataAtualizacao" TIMESTAMP,
    "UltimaSincronizacao" TIMESTAMP,
    "Observacoes" VARCHAR(1000),
    
    CONSTRAINT "UK_ClienteIntegracao_ClienteId" UNIQUE ("ClienteId")
);

-- Índices
CREATE INDEX IF NOT EXISTS "IX_ClienteIntegracao_ApiKey" ON "ClienteIntegracao"("ApiKey");
CREATE INDEX IF NOT EXISTS "IX_ClienteIntegracao_ClienteId" ON "ClienteIntegracao"("ClienteId");
CREATE INDEX IF NOT EXISTS "IX_ClienteIntegracao_Ativo" ON "ClienteIntegracao"("Ativo");

-- Comentários
COMMENT ON TABLE "ClienteIntegracao" IS 'Configurações de integração de folha por cliente';
COMMENT ON COLUMN "ClienteIntegracao"."ApiKey" IS 'Chave pública de API (formato: sk_live_... ou sk_test_...)';
COMMENT ON COLUMN "ClienteIntegracao"."ApiSecret" IS 'Chave secreta para HMAC (formato: whsec_...)';
COMMENT ON COLUMN "ClienteIntegracao"."IpWhitelist" IS 'IPs permitidos separados por vírgula (ex: 203.0.113.50,198.51.100.0/24)';

-- =====================================================

-- Tabela: IntegracaoFolhaLog
-- Armazena logs de auditoria de todas as integrações
CREATE TABLE IF NOT EXISTS "IntegracaoFolhaLog" (
    "Id" SERIAL PRIMARY KEY,
    "IntegracaoId" VARCHAR(50) NOT NULL,
    "ClienteId" INT NOT NULL,
    "DataHora" TIMESTAMP NOT NULL DEFAULT NOW(),
    "IpOrigem" VARCHAR(50),
    "ApiKey" VARCHAR(100),
    "TipoOperacao" VARCHAR(20),
    "ColaboradoresEnviados" INT NOT NULL DEFAULT 0,
    "ColaboradoresProcessados" INT NOT NULL DEFAULT 0,
    "ColaboradoresErro" INT NOT NULL DEFAULT 0,
    "Erros" TEXT,
    "TempoProcessamento" INT NOT NULL DEFAULT 0,
    "StatusCode" INT NOT NULL DEFAULT 200,
    "Sucesso" BOOLEAN NOT NULL DEFAULT true,
    "Mensagem" VARCHAR(500),
    
    CONSTRAINT "UK_IntegracaoFolhaLog_IntegracaoId" UNIQUE ("IntegracaoId")
);

-- Índices
CREATE INDEX IF NOT EXISTS "IX_IntegracaoFolhaLog_ClienteId" ON "IntegracaoFolhaLog"("ClienteId");
CREATE INDEX IF NOT EXISTS "IX_IntegracaoFolhaLog_DataHora" ON "IntegracaoFolhaLog"("DataHora" DESC);
CREATE INDEX IF NOT EXISTS "IX_IntegracaoFolhaLog_Sucesso" ON "IntegracaoFolhaLog"("Sucesso");
CREATE INDEX IF NOT EXISTS "IX_IntegracaoFolhaLog_IntegracaoId" ON "IntegracaoFolhaLog"("IntegracaoId");

-- Comentários
COMMENT ON TABLE "IntegracaoFolhaLog" IS 'Logs de auditoria das integrações de folha';
COMMENT ON COLUMN "IntegracaoFolhaLog"."IntegracaoId" IS 'ID único da integração (formato: int-YYYYMMDD-XXXXXXXX)';
COMMENT ON COLUMN "IntegracaoFolhaLog"."TipoOperacao" IS 'FULL_SYNC ou INCREMENTAL';
COMMENT ON COLUMN "IntegracaoFolhaLog"."TempoProcessamento" IS 'Tempo em milissegundos';

-- =====================================================

-- Dados de exemplo (DESENVOLVIMENTO APENAS)
-- Para produção, gerar API Keys via código ou script específico

-- Exemplo: Cliente de teste
-- INSERT INTO "ClienteIntegracao" 
-- ("ClienteId", "ApiKey", "ApiSecret", "Ativo", "DataCriacao", "Observacoes")
-- VALUES 
-- (1, 'sk_test_abcd1234efgh5678ijkl9012mnop', 'whsec_test1234567890abcdefghijklmnopqrstuvwxyz', true, NOW(), 'Cliente de teste');

-- =====================================================
-- Fim do script
-- =====================================================


