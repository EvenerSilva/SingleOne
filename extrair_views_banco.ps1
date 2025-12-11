# Script para extrair views do banco PostgreSQL
# Uso: .\extrair_views_banco.ps1

$dbHost = "localhost"
$dbPort = "5432"
$dbName = "singleone"
$dbUser = "postgres"
$dbPassword = Read-Host "Digite a senha do PostgreSQL (ou deixe vazio se não houver)" -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($dbPassword))

# Se senha vazia, tenta sem senha
if ([string]::IsNullOrWhiteSpace($plainPassword)) {
    $env:PGPASSWORD = ""
} else {
    $env:PGPASSWORD = $plainPassword
}

Write-Host "Conectando ao banco $dbName em $dbHost:$dbPort..."

# Query para extrair todas as views
$query = @"
SELECT 
    '-- View: ' || schemaname || '.' || viewname || E'\n' ||
    'DROP VIEW IF EXISTS ' || schemaname || '.' || viewname || ' CASCADE;' || E'\n' ||
    'CREATE OR REPLACE VIEW ' || schemaname || '.' || viewname || ' AS' || E'\n' ||
    pg_get_viewdef(schemaname || '.' || viewname, true) || ';' || E'\n'
FROM pg_views 
WHERE schemaname = 'public' 
ORDER BY viewname;
"@

# Executar query e salvar resultado
$query | psql -U $dbUser -d $dbName -h $dbHost -p $dbPort -t -A | Out-File -FilePath "views_do_banco_extraidas.sql" -Encoding utf8

Write-Host "Views extraídas para: views_do_banco_extraidas.sql"

