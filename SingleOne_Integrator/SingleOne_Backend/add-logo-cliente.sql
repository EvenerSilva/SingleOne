-- =====================================================
-- SCRIPT PARA ADICIONAR CAMPO LOGO NA TABELA CLIENTES
-- =====================================================

-- Adicionar coluna para armazenar o caminho/nome do arquivo da logo
ALTER TABLE clientes ADD COLUMN logo VARCHAR(500);

-- Adicionar comentário explicativo
COMMENT ON COLUMN clientes.logo IS 'Caminho ou nome do arquivo da logo personalizada do cliente';

-- Verificar se a coluna foi criada
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'clientes' AND column_name = 'logo';

-- Exemplo de uso:
-- UPDATE clientes SET logo = 'cliente1_logo.png' WHERE id = 1;
-- UPDATE clientes SET logo = 'cliente2_logo.jpg' WHERE id = 2;
