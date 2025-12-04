-- =============================================
-- Script: Criação de Tabelas para Campanhas de Assinaturas
-- Versão: 1.0
-- Data: 2025-10-20
-- Descrição: Estrutura completa para gerenciar campanhas de assinaturas de termos
-- =============================================

-- Tabela Principal: Campanhas de Assinaturas
CREATE TABLE IF NOT EXISTS campanhasassinaturas (
    id SERIAL PRIMARY KEY,
    cliente INTEGER NOT NULL,
    usuariocriacao INTEGER NOT NULL,
    nome VARCHAR(200) NOT NULL,
    descricao TEXT,
    datacriacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    datainicio TIMESTAMP,
    datafim TIMESTAMP,
    status CHAR(1) NOT NULL DEFAULT 'A', -- A=Ativa, I=Inativa, C=Concluída, G=Agendada
    filtrosjson TEXT, -- JSON com filtros aplicados
    totalcolaboradores INTEGER NOT NULL DEFAULT 0,
    totalenviados INTEGER NOT NULL DEFAULT 0,
    totalassinados INTEGER NOT NULL DEFAULT 0,
    totalpendentes INTEGER NOT NULL DEFAULT 0,
    percentualadesao DECIMAL(5,2),
    dataultimoenvio TIMESTAMP,
    dataconclusao TIMESTAMP,
    
    CONSTRAINT fk_campanhaassinatura_cliente FOREIGN KEY (cliente) REFERENCES clientes(id),
    CONSTRAINT fk_campanhaassinatura_usuario FOREIGN KEY (usuariocriacao) REFERENCES usuarios(id),
    CONSTRAINT chk_campanhaassinatura_status CHECK (status IN ('A', 'I', 'C', 'G'))
);

-- Índices para melhor performance
CREATE INDEX IF NOT EXISTS idx_campanhasassinaturas_cliente ON campanhasassinaturas(cliente);
CREATE INDEX IF NOT EXISTS idx_campanhasassinaturas_status ON campanhasassinaturas(status);
CREATE INDEX IF NOT EXISTS idx_campanhasassinaturas_datacriacao ON campanhasassinaturas(datacriacao);

-- Comentários
COMMENT ON TABLE campanhasassinaturas IS 'Campanhas de assinaturas de termos de responsabilidade';
COMMENT ON COLUMN campanhasassinaturas.status IS 'A=Ativa, I=Inativa, C=Concluída, G=Agendada';
COMMENT ON COLUMN campanhasassinaturas.filtrosjson IS 'JSON com empresas, localidades, tipos de colaborador, etc';

-- =============================================

-- Tabela de Associação: Campanhas e Colaboradores
CREATE TABLE IF NOT EXISTS campanhascolaboradores (
    id SERIAL PRIMARY KEY,
    campanhaid INTEGER NOT NULL,
    colaboradorid INTEGER NOT NULL,
    datainclusao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    statusassinatura CHAR(1) NOT NULL DEFAULT 'P', -- P=Pendente, E=Enviado, A=Assinado, R=Recusado
    dataenvio TIMESTAMP,
    dataassinatura TIMESTAMP,
    totalenvios INTEGER DEFAULT 0,
    dataultimoenvio TIMESTAMP,
    ipenvio VARCHAR(50),
    localizacaoenvio VARCHAR(500),
    
    CONSTRAINT fk_campanhacolaborador_campanha FOREIGN KEY (campanhaid) REFERENCES campanhasassinaturas(id) ON DELETE CASCADE,
    CONSTRAINT fk_campanhacolaborador_colaborador FOREIGN KEY (colaboradorid) REFERENCES colaboradores(id),
    CONSTRAINT chk_campanhacolaborador_status CHECK (statusassinatura IN ('P', 'E', 'A', 'R')),
    CONSTRAINT uk_campanhacolaborador UNIQUE (campanhaid, colaboradorid)
);

-- Índices para melhor performance
CREATE INDEX IF NOT EXISTS idx_campanhascolaboradores_campanha ON campanhascolaboradores(campanhaid);
CREATE INDEX IF NOT EXISTS idx_campanhascolaboradores_colaborador ON campanhascolaboradores(colaboradorid);
CREATE INDEX IF NOT EXISTS idx_campanhascolaboradores_status ON campanhascolaboradores(statusassinatura);
CREATE INDEX IF NOT EXISTS idx_campanhascolaboradores_datainclusao ON campanhascolaboradores(datainclusao);

-- Comentários
COMMENT ON TABLE campanhascolaboradores IS 'Associação entre campanhas e colaboradores com controle de status';
COMMENT ON COLUMN campanhascolaboradores.statusassinatura IS 'P=Pendente, E=Enviado, A=Assinado, R=Recusado';

-- =============================================
-- Views para facilitar consultas
-- =============================================

-- View: Resumo de Campanhas
CREATE OR REPLACE VIEW vw_campanhas_resumo AS
SELECT 
    c.id,
    c.cliente,
    c.nome,
    c.descricao,
    c.datacriacao,
    c.datainicio,
    c.datafim,
    c.status,
    c.totalcolaboradores,
    c.totalenviados,
    c.totalassinados,
    c.totalpendentes,
    c.percentualadesao,
    c.dataultimoenvio,
    c.dataconclusao,
    u.nome AS usuariocriacao_nome,
    CASE c.status
        WHEN 'A' THEN 'Ativa'
        WHEN 'I' THEN 'Inativa'
        WHEN 'C' THEN 'Concluída'
        WHEN 'G' THEN 'Agendada'
    END AS status_descricao,
    COUNT(cc.id) AS total_colaboradores_cadastrados,
    COUNT(CASE WHEN cc.statusassinatura = 'A' THEN 1 END) AS total_assinados_real,
    COUNT(CASE WHEN cc.statusassinatura IN ('P', 'E') THEN 1 END) AS total_pendentes_real
FROM campanhasassinaturas c
LEFT JOIN usuarios u ON c.usuariocriacao = u.id
LEFT JOIN campanhascolaboradores cc ON c.id = cc.campanhaid
GROUP BY c.id, c.cliente, c.nome, c.descricao, c.datacriacao, c.datainicio, c.datafim, 
         c.status, c.totalcolaboradores, c.totalenviados, c.totalassinados, 
         c.totalpendentes, c.percentualadesao, c.dataultimoenvio, c.dataconclusao, u.nome;

COMMENT ON VIEW vw_campanhas_resumo IS 'Visão resumida das campanhas com estatísticas atualizadas';

-- =============================================

-- View: Detalhes de Campanhas com Colaboradores
CREATE OR REPLACE VIEW vw_campanhas_colaboradores_detalhado AS
SELECT 
    c.id AS campanha_id,
    c.nome AS campanha_nome,
    c.status AS campanha_status,
    cc.id AS associacao_id,
    cc.colaboradorid,
    col.nome AS colaborador_nome,
    col.cpf AS colaborador_cpf,
    col.email AS colaborador_email,
    col.cargo AS colaborador_cargo,
    e.nome AS empresa_nome,
    l.descricao AS localidade_nome,
    cc.statusassinatura,
    CASE cc.statusassinatura
        WHEN 'P' THEN 'Pendente'
        WHEN 'E' THEN 'Enviado'
        WHEN 'A' THEN 'Assinado'
        WHEN 'R' THEN 'Recusado'
    END AS status_descricao,
    cc.datainclusao,
    cc.dataenvio,
    cc.dataassinatura,
    cc.totalenvios,
    cc.dataultimoenvio,
    cc.ipenvio,
    cc.localizacaoenvio
FROM campanhasassinaturas c
INNER JOIN campanhascolaboradores cc ON c.id = cc.campanhaid
INNER JOIN colaboradores col ON cc.colaboradorid = col.id
LEFT JOIN empresas e ON col.empresa = e.id
LEFT JOIN localidades l ON col.localidade = l.id;

COMMENT ON VIEW vw_campanhas_colaboradores_detalhado IS 'Visão detalhada de colaboradores por campanha';

-- =============================================
-- Funções para atualizar estatísticas
-- =============================================

CREATE OR REPLACE FUNCTION atualizar_estatisticas_campanha(p_campanha_id INTEGER)
RETURNS VOID AS $$
BEGIN
    UPDATE campanhasassinaturas
    SET 
        totalcolaboradores = (
            SELECT COUNT(*) 
            FROM campanhascolaboradores 
            WHERE campanhaid = p_campanha_id
        ),
        totalenviados = (
            SELECT COUNT(*) 
            FROM campanhascolaboradores 
            WHERE campanhaid = p_campanha_id 
            AND statusassinatura IN ('E', 'A')
        ),
        totalassinados = (
            SELECT COUNT(*) 
            FROM campanhascolaboradores 
            WHERE campanhaid = p_campanha_id 
            AND statusassinatura = 'A'
        ),
        totalpendentes = (
            SELECT COUNT(*) 
            FROM campanhascolaboradores 
            WHERE campanhaid = p_campanha_id 
            AND statusassinatura IN ('P', 'E')
        ),
        percentualadesao = (
            SELECT 
                CASE 
                    WHEN COUNT(*) > 0 THEN 
                        ROUND((COUNT(CASE WHEN statusassinatura = 'A' THEN 1 END)::DECIMAL / COUNT(*)::DECIMAL) * 100, 2)
                    ELSE 0 
                END
            FROM campanhascolaboradores 
            WHERE campanhaid = p_campanha_id
        ),
        dataultimoenvio = (
            SELECT MAX(dataultimoenvio)
            FROM campanhascolaboradores
            WHERE campanhaid = p_campanha_id
        )
    WHERE id = p_campanha_id;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION atualizar_estatisticas_campanha(INTEGER) IS 'Atualiza estatísticas de uma campanha específica';

-- =============================================
-- Triggers para atualização automática
-- =============================================

CREATE OR REPLACE FUNCTION trigger_atualizar_campanha()
RETURNS TRIGGER AS $$
BEGIN
    -- Atualizar estatísticas da campanha
    PERFORM atualizar_estatisticas_campanha(
        CASE 
            WHEN TG_OP = 'DELETE' THEN OLD.campanhaid
            ELSE NEW.campanhaid
        END
    );
    
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- Trigger para INSERT/UPDATE/DELETE em campanhascolaboradores
DROP TRIGGER IF EXISTS trg_atualizar_campanha_colaboradores ON campanhascolaboradores;
CREATE TRIGGER trg_atualizar_campanha_colaboradores
AFTER INSERT OR UPDATE OR DELETE ON campanhascolaboradores
FOR EACH ROW
EXECUTE FUNCTION trigger_atualizar_campanha();

-- =============================================
-- Dados iniciais / exemplos (opcional)
-- =============================================

-- Inserir campanha de exemplo (comentado por padrão)
/*
INSERT INTO campanhasassinaturas (cliente, usuariocriacao, nome, descricao, datacriacao, status)
VALUES (1, 1, 'Campanha Inicial 2025', 'Primeira campanha de assinaturas do ano', CURRENT_TIMESTAMP, 'A');
*/

-- =============================================
-- Grants de permissão (ajustar conforme necessário)
-- =============================================

-- GRANT SELECT, INSERT, UPDATE, DELETE ON campanhasassinaturas TO seu_usuario;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON campanhascolaboradores TO seu_usuario;
-- GRANT SELECT ON vw_campanhas_resumo TO seu_usuario;
-- GRANT SELECT ON vw_campanhas_colaboradores_detalhado TO seu_usuario;

-- =============================================
-- FIM DO SCRIPT
-- =============================================

-- Para verificar se as tabelas foram criadas:
-- SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name LIKE 'campanha%';

