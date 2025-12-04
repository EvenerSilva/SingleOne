-- =====================================================
-- SCRIPT FINAL PARA FUNCIONALIDADE DE SINALIZAÇÃO DE SUSPEITAS
-- =====================================================
-- Sistema para vigilantes da portaria sinalizarem suspeitas
-- e o time de segurança gerenciar investigações

-- =====================================================
-- TABELA 1: SINALIZAÇÕES DE SUSPEITAS
-- =====================================================
CREATE TABLE IF NOT EXISTS sinalizacoes_suspeitas (
    id SERIAL PRIMARY KEY,
    colaborador_id INTEGER NOT NULL REFERENCES colaboradores(id),
    vigilante_id INTEGER REFERENCES usuarios(id), -- Usuário que fez a sinalização
    cpf_consultado VARCHAR(14) NOT NULL, -- CPF que foi consultado no PassCheck
    motivo_suspeita VARCHAR(50) NOT NULL, -- Motivo da suspeita (pré-definido)
    descricao_detalhada TEXT, -- Descrição detalhada da suspeita
    observacoes_vigilante TEXT, -- Observações adicionais do vigilante
    status VARCHAR(20) DEFAULT 'pendente' CHECK (status IN ('pendente', 'em_investigacao', 'resolvida', 'arquivada')),
    prioridade VARCHAR(10) DEFAULT 'media' CHECK (prioridade IN ('baixa', 'media', 'alta', 'critica')),
    
    -- Dados da consulta no momento da sinalização
    dados_consulta JSONB, -- Dados que foram consultados no PassCheck
    ip_address VARCHAR(45), -- Suporta IPv4 e IPv6
    user_agent TEXT,
    
    -- Evidências
    evidencia_urls TEXT[], -- Array de URLs de evidências (fotos, documentos)
    
    -- Timestamps
    data_sinalizacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    data_investigacao TIMESTAMP NULL,
    data_resolucao TIMESTAMP NULL,
    
    -- Usuário responsável pela investigação
    investigador_id INTEGER REFERENCES usuarios(id),
    
    -- Resultado da investigação
    resultado_investigacao TEXT,
    acoes_tomadas TEXT,
    observacoes_finais TEXT,
    
    -- Auditoria
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- TABELA 2: HISTÓRICO DE INVESTIGAÇÕES
-- =====================================================
CREATE TABLE IF NOT EXISTS historico_investigacoes (
    id SERIAL PRIMARY KEY,
    sinalizacao_id INTEGER NOT NULL REFERENCES sinalizacoes_suspeitas(id) ON DELETE CASCADE,
    usuario_id INTEGER NOT NULL REFERENCES usuarios(id),
    acao VARCHAR(50) NOT NULL, -- 'criada', 'atribuida', 'em_investigacao', 'resolvida', 'arquivada'
    descricao TEXT,
    dados_antes JSONB, -- Estado antes da ação
    dados_depois JSONB, -- Estado depois da ação
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- TABELA 3: MOTIVOS DE SUSPEITA (CONFIGURÁVEIS)
-- =====================================================
CREATE TABLE IF NOT EXISTS motivos_suspeita (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) UNIQUE NOT NULL,
    descricao VARCHAR(100) NOT NULL,
    descricao_detalhada TEXT,
    ativo BOOLEAN DEFAULT true,
    prioridade_padrao VARCHAR(10) DEFAULT 'media' CHECK (prioridade_padrao IN ('baixa', 'media', 'alta', 'critica')),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- ÍNDICES PARA PERFORMANCE
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_colaborador ON sinalizacoes_suspeitas(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_vigilante ON sinalizacoes_suspeitas(vigilante_id);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_status ON sinalizacoes_suspeitas(status);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_prioridade ON sinalizacoes_suspeitas(prioridade);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_data ON sinalizacoes_suspeitas(data_sinalizacao);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_investigador ON sinalizacoes_suspeitas(investigador_id);

CREATE INDEX IF NOT EXISTS idx_historico_sinalizacao ON historico_investigacoes(sinalizacao_id);
CREATE INDEX IF NOT EXISTS idx_historico_usuario ON historico_investigacoes(usuario_id);
CREATE INDEX IF NOT EXISTS idx_historico_data ON historico_investigacoes(created_at);

CREATE INDEX IF NOT EXISTS idx_motivos_ativo ON motivos_suspeita(ativo);

-- =====================================================
-- TRIGGER PARA ATUALIZAR updated_at
-- =====================================================
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_sinalizacoes_updated_at 
    BEFORE UPDATE ON sinalizacoes_suspeitas 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =====================================================
-- TRIGGER PARA HISTÓRICO AUTOMÁTICO
-- =====================================================
CREATE OR REPLACE FUNCTION criar_historico_sinalizacao()
RETURNS TRIGGER AS $$
BEGIN
    -- Inserir no histórico quando uma sinalização é criada
    IF TG_OP = 'INSERT' THEN
        INSERT INTO historico_investigacoes (sinalizacao_id, usuario_id, acao, descricao)
        VALUES (NEW.id, COALESCE(NEW.vigilante_id, 0), 'criada', 'Sinalização de suspeita criada');
    END IF;
    
    -- Inserir no histórico quando status muda
    IF TG_OP = 'UPDATE' AND OLD.status != NEW.status THEN
        INSERT INTO historico_investigacoes (sinalizacao_id, usuario_id, acao, descricao, dados_antes, dados_depois)
        VALUES (NEW.id, COALESCE(NEW.investigador_id, NEW.vigilante_id, 0), 
                'status_alterado', 
                'Status alterado de ' || OLD.status || ' para ' || NEW.status,
                json_build_object('status', OLD.status),
                json_build_object('status', NEW.status));
    END IF;
    
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER trigger_historico_sinalizacao
    AFTER INSERT OR UPDATE ON sinalizacoes_suspeitas
    FOR EACH ROW EXECUTE FUNCTION criar_historico_sinalizacao();

-- =====================================================
-- DADOS INICIAIS - MOTIVOS DE SUSPEITA
-- =====================================================
INSERT INTO motivos_suspeita (codigo, descricao, descricao_detalhada, prioridade_padrao) VALUES 
('comportamento_estranho', 'Comportamento Estranho', 'Colaborador apresentou comportamento suspeito ou atípico', 'media'),
('documentos_inconsistentes', 'Documentos Inconsistentes', 'Documentos apresentados não conferem com os dados do sistema', 'alta'),
('equipamentos_nao_reconhecidos', 'Equipamentos Não Reconhecidos', 'Colaborador não reconhece equipamentos listados no sistema', 'alta'),
('tentativa_evasao', 'Tentativa de Evasão', 'Colaborador tentou evitar procedimentos de verificação', 'critica'),
('acompanhante_suspeito', 'Acompanhante Suspeito', 'Pessoa acompanhando o colaborador apresentou comportamento suspeito', 'alta'),
('horario_atipico', 'Horário Atípico', 'Acesso em horário não usual ou fora do expediente', 'baixa'),
('equipamentos_em_excesso', 'Equipamentos em Excesso', 'Quantidade de equipamentos superior ao esperado', 'media'),
('nervosismo_excessivo', 'Nervosismo Excessivo', 'Colaborador demonstrou nervosismo ou ansiedade excessiva', 'media'),
('outros', 'Outros Motivos', 'Outros motivos não listados', 'media')
ON CONFLICT (codigo) DO NOTHING;

-- =====================================================
-- COMENTÁRIOS DAS TABELAS
-- =====================================================
COMMENT ON TABLE sinalizacoes_suspeitas IS 'Tabela para sinalizações de suspeitas feitas pelos vigilantes da portaria';
COMMENT ON TABLE historico_investigacoes IS 'Histórico de todas as ações realizadas nas investigações';
COMMENT ON TABLE motivos_suspeita IS 'Motivos pré-definidos para sinalizações de suspeitas';

COMMENT ON COLUMN sinalizacoes_suspeitas.colaborador_id IS 'ID do colaborador que passou pela portaria';
COMMENT ON COLUMN sinalizacoes_suspeitas.vigilante_id IS 'ID do usuário (vigilante) que fez a sinalização';
COMMENT ON COLUMN sinalizacoes_suspeitas.cpf_consultado IS 'CPF que foi consultado no PassCheck';
COMMENT ON COLUMN sinalizacoes_suspeitas.motivo_suspeita IS 'Motivo da suspeita (código do motivo)';
COMMENT ON COLUMN sinalizacoes_suspeitas.status IS 'Status da investigação: pendente, em_investigacao, resolvida, arquivada';
COMMENT ON COLUMN sinalizacoes_suspeitas.prioridade IS 'Prioridade da investigação: baixa, media, alta, critica';
COMMENT ON COLUMN sinalizacoes_suspeitas.dados_consulta IS 'Dados que foram consultados no PassCheck no momento da sinalização';
COMMENT ON COLUMN sinalizacoes_suspeitas.evidencia_urls IS 'Array de URLs de evidências (fotos, documentos)';

-- =====================================================
-- VERIFICAÇÃO FINAL
-- =====================================================
-- Verificar se as tabelas foram criadas corretamente
SELECT 
    'sinalizacoes_suspeitas' as tabela,
    COUNT(*) as colunas
FROM information_schema.columns 
WHERE table_name = 'sinalizacoes_suspeitas'

UNION ALL

SELECT 
    'historico_investigacoes' as tabela,
    COUNT(*) as colunas
FROM information_schema.columns 
WHERE table_name = 'historico_investigacoes'

UNION ALL

SELECT 
    'motivos_suspeita' as tabela,
    COUNT(*) as colunas
FROM information_schema.columns 
WHERE table_name = 'motivos_suspeita';

-- =====================================================
-- INSTRUÇÕES DE USO
-- =====================================================
/*
INSTRUÇÕES:

1. Execute este script no banco de dados SingleOne
2. As tabelas serão criadas com todos os índices e triggers necessários
3. Os motivos de suspeita padrão serão inseridos automaticamente
4. Os parâmetros do sistema serão configurados via interface administrativa

FUNCIONALIDADES IMPLEMENTADAS:

SINALIZAÇÃO DE SUSPEITAS:
- Vigilantes podem sinalizar suspeitas durante consulta no PassCheck
- Sistema de prioridades (baixa, média, alta, crítica)
- Anexo de evidências (fotos, documentos)
- Histórico completo de todas as ações

GERENCIAMENTO DE INVESTIGAÇÕES:
- Time de segurança pode visualizar todas as sinalizações
- Atribuir investigações para membros específicos do time
- Acompanhar status e progresso das investigações
- Resolver investigações com conclusões e ações tomadas

AUDITORIA E RELATÓRIOS:
- Histórico completo de todas as ações
- Logs de IP e User-Agent
- Dados da consulta no momento da sinalização
- Relatórios de estatísticas e tendências

PRÓXIMOS PASSOS:
1. Implementar controllers no backend
2. Criar interface no frontend
3. Implementar notificações por email
4. Testar funcionalidades
*/
