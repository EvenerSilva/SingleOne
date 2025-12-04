# =====================================================
# SCRIPT: Executar SQL para Criação de Categorias
# DESCRIÇÃO: Executa o script SQL para criar tabela categorias
# DATA: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
# =====================================================

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "EXECUTANDO SCRIPT SQL PARA CATEGORIAS" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se o PostgreSQL está instalado
try {
    $psqlVersion = psql --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ PostgreSQL encontrado:" -ForegroundColor Green
        Write-Host $psqlVersion -ForegroundColor White
    } else {
        throw "PostgreSQL não encontrado"
    }
} catch {
    Write-Host "❌ ERRO: PostgreSQL não está instalado ou não está no PATH" -ForegroundColor Red
    Write-Host "Por favor, instale o PostgreSQL ou adicione ao PATH" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Configurações do banco
$DB_HOST = "127.0.0.1"
$DB_NAME = "singleone"
$DB_USER = "postgres"
$DB_PASSWORD = "Admin@2025"

Write-Host "📊 Configurações do Banco:" -ForegroundColor Yellow
Write-Host "  Host: $DB_HOST" -ForegroundColor White
Write-Host "  Database: $DB_NAME" -ForegroundColor White
Write-Host "  User: $DB_USER" -ForegroundColor White
Write-Host ""

# Verificar se o banco existe
Write-Host "🔍 Verificando se o banco '$DB_NAME' existe..." -ForegroundColor Yellow
$checkDb = psql -h $DB_HOST -U $DB_USER -d postgres -t -c "SELECT 1 FROM pg_database WHERE datname='$DB_NAME';" 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ ERRO: Não foi possível conectar ao PostgreSQL" -ForegroundColor Red
    Write-Host "Verifique se o serviço está rodando e as credenciais estão corretas" -ForegroundColor Yellow
    exit 1
}

if ($checkDb -eq "") {
    Write-Host "❌ ERRO: Banco '$DB_NAME' não encontrado" -ForegroundColor Red
    Write-Host "Por favor, crie o banco primeiro" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Banco '$DB_NAME' encontrado!" -ForegroundColor Green
Write-Host ""

# Executar o script SQL
Write-Host "🚀 Executando script SQL..." -ForegroundColor Yellow

$sqlScript = @"
-- =====================================================
-- SCRIPT: Criar Tabela Categorias e Alterar TiposEquipamento
-- DESCRIÇÃO: Implementa sistema de categorias para recursos
-- DATA: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
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
"@

# Salvar script em arquivo temporário
$tempFile = [System.IO.Path]::GetTempFileName() + ".sql"
$sqlScript | Out-File -FilePath $tempFile -Encoding UTF8

try {
    # Executar o script
    $env:PGPASSWORD = $DB_PASSWORD
    $result = psql -h $DB_HOST -U $DB_USER -d $DB_NAME -f $tempFile 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Script SQL executado com sucesso!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Resultado da execução:" -ForegroundColor Cyan
        Write-Host $result -ForegroundColor White
    } else {
        Write-Host "❌ ERRO ao executar script SQL:" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ ERRO ao executar script:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
} finally {
    # Limpar arquivo temporário
    if (Test-Path $tempFile) {
        Remove-Item $tempFile -Force
    }
    # Limpar variável de ambiente
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "SCRIPT EXECUTADO COM SUCESSO!" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "🎯 Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Compilar o projeto .NET" -ForegroundColor White
Write-Host "2. Executar o backend" -ForegroundColor White
Write-Host "3. Testar as APIs de categoria" -ForegroundColor White
Write-Host ""
Write-Host "Pressione qualquer tecla para continuar..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
