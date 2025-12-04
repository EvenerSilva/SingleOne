-- =====================================================
-- SCRIPT PARA SIMPLIFICAÇÃO DOS CADASTROS
-- Implementa herança automática para reduzir campos obrigatórios
-- =====================================================

-- 1. ALTERAR CONSTRAINTS - Tornar campos opcionais
-- =====================================================

-- Colaboradores: tornar cliente opcional (herda da empresa)
ALTER TABLE colaboradores ALTER COLUMN cliente DROP NOT NULL;

-- Equipamentos: tornar cliente opcional (herda da empresa)
ALTER TABLE equipamentos ALTER COLUMN cliente DROP NOT NULL;

-- 2. CRIAR FUNÇÕES PARA HERANÇA AUTOMÁTICA
-- =====================================================

-- Função para preencher cliente automaticamente em colaboradores
CREATE OR REPLACE FUNCTION preencher_cliente_colaborador()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.cliente IS NULL THEN
        NEW.cliente = (SELECT cliente FROM empresas WHERE id = NEW.empresa);
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Função para preencher cliente automaticamente em equipamentos
CREATE OR REPLACE FUNCTION preencher_cliente_equipamento()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.cliente IS NULL AND NEW.empresa IS NOT NULL THEN
        NEW.cliente = (SELECT cliente FROM empresas WHERE id = NEW.empresa);
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 3. CRIAR TRIGGERS
-- =====================================================

-- Trigger para colaboradores
DROP TRIGGER IF EXISTS trigger_cliente_colaborador ON colaboradores;
CREATE TRIGGER trigger_cliente_colaborador
    BEFORE INSERT OR UPDATE ON colaboradores
    FOR EACH ROW
    EXECUTE FUNCTION preencher_cliente_colaborador();

-- Trigger para equipamentos
DROP TRIGGER IF EXISTS trigger_cliente_equipamento ON equipamentos;
CREATE TRIGGER trigger_cliente_equipamento
    BEFORE INSERT OR UPDATE ON equipamentos
    FOR EACH ROW
    EXECUTE FUNCTION preencher_cliente_equipamento();

-- 4. CRIAR VIEWS SIMPLIFICADAS
-- =====================================================

-- View para colaboradores com herança automática
CREATE OR REPLACE VIEW vw_colaboradores_simples AS
SELECT 
    c.id,
    c.nome,
    c.cpf,
    c.matricula,
    c.email,
    c.cargo,
    c.setor,
    c.dtadmissao,
    c.situacao,
    c.empresa,
    e.nome as empresa_nome,
    c.centrocusto,
    cc.nome as centro_custo_nome,
    c.filial_id,
    f.nome as filial_nome,
    c.localidade_id,
    l.descricao as localidade_nome,
    -- Cliente herdado automaticamente
    COALESCE(c.cliente, e.cliente) as cliente,
    cl.razaosocial as cliente_nome
FROM colaboradores c
INNER JOIN empresas e ON c.empresa = e.id
INNER JOIN centrocusto cc ON c.centrocusto = cc.id
LEFT JOIN filiais f ON c.filial_id = f.id
LEFT JOIN localidades l ON c.localidade_id = l.id
INNER JOIN clientes cl ON COALESCE(c.cliente, e.cliente) = cl.id;

-- View para equipamentos com herança automática
CREATE OR REPLACE VIEW vw_equipamentos_simples AS
SELECT 
    e.id,
    e.numeroserie,
    e.patrimonio,
    e.dtcadastro,
    e.ativo,
    e.empresa,
    emp.nome as empresa_nome,
    e.centrocusto,
    cc.nome as centro_custo_nome,
    e.filial_id,
    f.nome as filial_nome,
    e.localidade_id,
    l.descricao as localidade_nome,
    -- Cliente herdado automaticamente
    COALESCE(e.cliente, emp.cliente) as cliente,
    cl.razaosocial as cliente_nome
FROM equipamentos e
LEFT JOIN empresas emp ON e.empresa = emp.id
LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
LEFT JOIN filiais f ON e.filial_id = f.id
LEFT JOIN localidades l ON e.localidade_id = l.id
LEFT JOIN clientes cl ON COALESCE(e.cliente, emp.cliente) = cl.id;

-- 5. VERIFICAR IMPLEMENTAÇÃO
-- =====================================================

-- Verificar se os triggers foram criados
SELECT 
    trigger_name, 
    event_manipulation, 
    action_statement 
FROM information_schema.triggers 
WHERE trigger_name IN ('trigger_cliente_colaborador', 'trigger_cliente_equipamento');

-- Verificar se as views foram criadas
SELECT table_name, table_type 
FROM information_schema.tables 
WHERE table_name IN ('vw_colaboradores_simples', 'vw_equipamentos_simples');

-- Verificar estrutura das tabelas modificadas
SELECT column_name, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'colaboradores' 
AND column_name IN ('cliente', 'empresa', 'centrocusto', 'filial_id', 'localidade_id')
ORDER BY ordinal_position;

SELECT column_name, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
AND column_name IN ('cliente', 'empresa', 'centrocusto', 'filial_id', 'localidade_id')
ORDER BY ordinal_position;

-- 6. TESTAR HERANÇA AUTOMÁTICA
-- =====================================================

-- Teste: Inserir colaborador sem cliente (deve herdar da empresa)
-- INSERT INTO colaboradores (nome, cpf, matricula, email, cargo, setor, dtadmissao, empresa, centrocusto, situacao)
-- VALUES ('Teste Herança', '12345678901', 'TEST001', 'teste@teste.com', 'Analista', 'TI', NOW(), 1, 8, 'A');

-- Verificar se o cliente foi preenchido automaticamente
-- SELECT id, nome, empresa, cliente FROM colaboradores WHERE nome = 'Teste Herança';
