-- =====================================================
-- SCRIPT PARA CRIAR TABELAS DE ESTADOS E CIDADES
-- Sistema de referência para localidades
-- =====================================================

-- Criar tabela de estados
CREATE TABLE IF NOT EXISTS estados (
    id SERIAL PRIMARY KEY,
    sigla VARCHAR(2) NOT NULL UNIQUE,
    nome VARCHAR(50) NOT NULL,
    ativo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Criar tabela de cidades
CREATE TABLE IF NOT EXISTS cidades (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    estado_id INTEGER NOT NULL,
    ativo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_cidade_estado FOREIGN KEY (estado_id) REFERENCES estados(id)
);

-- Criar índices para melhor performance
CREATE INDEX IF NOT EXISTS idx_cidades_estado_id ON cidades(estado_id);
CREATE INDEX IF NOT EXISTS idx_cidades_nome ON cidades(nome);
CREATE INDEX IF NOT EXISTS idx_estados_sigla ON estados(sigla);

-- Inserir estados brasileiros
INSERT INTO estados (sigla, nome) VALUES
('AC', 'Acre'),
('AL', 'Alagoas'),
('AP', 'Amapá'),
('AM', 'Amazonas'),
('BA', 'Bahia'),
('CE', 'Ceará'),
('DF', 'Distrito Federal'),
('ES', 'Espírito Santo'),
('GO', 'Goiás'),
('MA', 'Maranhão'),
('MT', 'Mato Grosso'),
('MS', 'Mato Grosso do Sul'),
('MG', 'Minas Gerais'),
('PA', 'Pará'),
('PB', 'Paraíba'),
('PR', 'Paraná'),
('PE', 'Pernambuco'),
('PI', 'Piauí'),
('RJ', 'Rio de Janeiro'),
('RN', 'Rio Grande do Norte'),
('RS', 'Rio Grande do Sul'),
('RO', 'Rondônia'),
('RR', 'Roraima'),
('SC', 'Santa Catarina'),
('SP', 'São Paulo'),
('SE', 'Sergipe'),
('TO', 'Tocantins')
ON CONFLICT (sigla) DO NOTHING;

-- Inserir algumas cidades principais (exemplos)
INSERT INTO cidades (nome, estado_id) VALUES
-- São Paulo
('São Paulo', (SELECT id FROM estados WHERE sigla = 'SP')),
('Campinas', (SELECT id FROM estados WHERE sigla = 'SP')),
('Santos', (SELECT id FROM estados WHERE sigla = 'SP')),
('Ribeirão Preto', (SELECT id FROM estados WHERE sigla = 'SP')),
('Sorocaba', (SELECT id FROM estados WHERE sigla = 'SP')),

-- Rio de Janeiro
('Rio de Janeiro', (SELECT id FROM estados WHERE sigla = 'RJ')),
('Niterói', (SELECT id FROM estados WHERE sigla = 'RJ')),
('Petrópolis', (SELECT id FROM estados WHERE sigla = 'RJ')),

-- Minas Gerais
('Belo Horizonte', (SELECT id FROM estados WHERE sigla = 'MG')),
('Uberlândia', (SELECT id FROM estados WHERE sigla = 'MG')),
('Contagem', (SELECT id FROM estados WHERE sigla = 'MG')),

-- Paraná
('Curitiba', (SELECT id FROM estados WHERE sigla = 'PR')),
('Londrina', (SELECT id FROM estados WHERE sigla = 'PR')),
('Maringá', (SELECT id FROM estados WHERE sigla = 'PR')),

-- Rio Grande do Sul
('Porto Alegre', (SELECT id FROM estados WHERE sigla = 'RS')),
('Caxias do Sul', (SELECT id FROM estados WHERE sigla = 'RS')),
('Pelotas', (SELECT id FROM estados WHERE sigla = 'RS'))
ON CONFLICT DO NOTHING;

-- Verificar estrutura criada
SELECT 'ESTADOS' as tabela, COUNT(*) as total_registros FROM estados
UNION ALL
SELECT 'CIDADES' as tabela, COUNT(*) as total_registros FROM cidades;

-- Mostrar alguns exemplos
SELECT 'EXEMPLOS DE ESTADOS:' as info;
SELECT sigla, nome FROM estados ORDER BY nome LIMIT 5;

SELECT 'EXEMPLOS DE CIDADES:' as info;
SELECT c.nome as cidade, e.sigla as estado 
FROM cidades c 
JOIN estados e ON c.estado_id = e.id 
ORDER BY e.nome, c.nome 
LIMIT 10;
