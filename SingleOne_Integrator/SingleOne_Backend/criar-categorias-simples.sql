-- =====================================================
-- SCRIPT SIMPLIFICADO: Criar Tabela Categorias
-- DESCRIÇÃO: Implementa sistema de categorias para recursos
-- DATA: 2025-01-15
-- =====================================================

-- 1. Criar tabela 'categorias'
CREATE TABLE IF NOT EXISTS categorias (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL UNIQUE,
    descricao TEXT,
    ativo BOOLEAN DEFAULT true,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Adicionar campo 'categoria_id' na tabela 'tipoequipamento'
ALTER TABLE tipoequipamento 
ADD COLUMN IF NOT EXISTS categoria_id INTEGER;

-- 3. Adicionar constraint de chave estrangeira
ALTER TABLE tipoequipamento 
ADD CONSTRAINT fk_tipoequipamento_categoria 
FOREIGN KEY (categoria_id) REFERENCES categorias(id);

-- 4. Criar índice para melhor performance
CREATE INDEX IF NOT EXISTS idx_tipoequipamento_categoria_id 
ON tipoequipamento(categoria_id);

-- 5. Inserir algumas categorias padrão
INSERT INTO categorias (nome, descricao, ativo) VALUES
('Computadores', 'Equipamentos de computação como desktops, notebooks e tablets', true),
('Periféricos', 'Dispositivos auxiliares como mouses, teclados e monitores', true),
('Rede', 'Equipamentos de infraestrutura de rede', true),
('Impressão', 'Impressoras, scanners e equipamentos relacionados', true),
('Móveis', 'Móveis e acessórios para escritório', true)
ON CONFLICT (nome) DO NOTHING;

-- 6. Verificar estrutura criada
SELECT 
    'Tabela categorias criada com sucesso!' as status,
    COUNT(*) as total_categorias
FROM categorias;

-- 7. Verificar alteração na tabela tipoequipamento
SELECT 
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'tipoequipamento' 
AND column_name = 'categoria_id';
