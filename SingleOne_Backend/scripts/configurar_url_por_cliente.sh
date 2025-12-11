#!/bin/bash
# ========================================
# Script para CONFIGURAR URL POR CLIENTE
# Adiciona campo site_url na tabela clientes e configura cliente demo
# Execute: sudo bash configurar_url_por_cliente.sh
# ========================================

echo "=========================================="
echo "🔧 CONFIGURAÇÃO DE URL POR CLIENTE"
echo "=========================================="
echo ""

# 1. Aplicar migração SQL
echo "📋 Aplicando migração SQL..."
cd /opt/SingleOne/SingleOne_Backend/scripts

if [ -f "adicionar_site_url_clientes.sql" ]; then
    sudo -u postgres psql -d singleone -f adicionar_site_url_clientes.sql
    if [ $? -eq 0 ]; then
        echo "✅ Migração aplicada com sucesso"
    else
        echo "❌ Erro ao aplicar migração SQL"
        exit 1
    fi
else
    echo "⚠️  Arquivo adicionar_site_url_clientes.sql não encontrado"
    echo "📝 Aplicando SQL diretamente..."
    
    sudo -u postgres psql -d singleone << EOF
-- Adicionar coluna site_url se não existir
ALTER TABLE clientes ADD COLUMN IF NOT EXISTS site_url VARCHAR(500);

-- Comentário na coluna
COMMENT ON COLUMN clientes.site_url IS 'URL do site do cliente (ex: https://demo.singleone.com.br). Se não preenchido, usa a URL padrão do sistema.';

-- Atualizar cliente demo (ID 1) com a URL do domínio
UPDATE clientes 
SET site_url = 'https://demo.singleone.com.br' 
WHERE id = 1 AND (site_url IS NULL OR site_url = '');

-- Mostrar resultado
SELECT id, razaosocial, site_url 
FROM clientes 
ORDER BY id;
EOF
    
    if [ $? -eq 0 ]; then
        echo "✅ Migração aplicada com sucesso"
    else
        echo "❌ Erro ao aplicar migração SQL"
        exit 1
    fi
fi
echo ""

# 2. Verificar resultado
echo "📋 Verificando clientes configurados:"
sudo -u postgres psql -d singleone -c "SELECT id, razaosocial, site_url FROM clientes ORDER BY id;"
echo ""

# 3. Atualizar código
echo "📥 Atualizando código do Git..."
cd /opt/SingleOne
git pull origin main
echo "✅ Código atualizado"
echo ""

# 4. Parar API
echo "⏹️  Parando API..."
systemctl stop singleone-api
sleep 2

# 5. Publicar API
echo "📦 Publicando API..."
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI
rm -rf /opt/singleone-api-publish/*
dotnet publish -c Release -o /opt/singleone-api-publish
echo "✅ API publicada"
echo ""

# 6. Iniciar API
echo "▶️  Iniciando API..."
systemctl start singleone-api
sleep 3
echo "✅ API iniciada"
echo ""

# 7. Verificar logs
echo "=========================================="
echo "📋 Logs de inicialização:"
echo "=========================================="
journalctl -u singleone-api -n 30 --no-pager | grep -E "STARTUP|OBTER_URL" || echo "Nenhum log relevante encontrado ainda"
echo ""

echo "=========================================="
echo "✅ CONFIGURAÇÃO CONCLUÍDA!"
echo "=========================================="
echo ""
echo "📋 Para configurar URL de outros clientes:"
echo "   sudo -u postgres psql -d singleone -c \"UPDATE clientes SET site_url = 'https://seudominio.com.br' WHERE id = CLIENTE_ID;\""
echo ""
echo "📋 Para ver URLs configuradas:"
echo "   sudo -u postgres psql -d singleone -c \"SELECT id, razaosocial, site_url FROM clientes;\""
echo ""

