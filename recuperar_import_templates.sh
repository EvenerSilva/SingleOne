#!/bin/bash
# =====================================================
# RECUPERAR import_templates.sql DO SERVIDOR CONTABO
# =====================================================
# Execute este script no servidor para copiar o arquivo

echo "📥 Recuperando import_templates.sql do servidor..."
echo ""

# Verificar se o arquivo existe no servidor
if [ -f "/opt/SingleOne/import_templates.sql" ]; then
    echo "✅ Arquivo encontrado no servidor"
    echo ""
    echo "📋 Conteúdo do arquivo (primeiras 20 linhas):"
    head -20 /opt/SingleOne/import_templates.sql
    echo ""
    echo "💡 Para copiar o conteúdo completo, execute:"
    echo "   cat /opt/SingleOne/import_templates.sql"
else
    echo "❌ Arquivo não encontrado em /opt/SingleOne/import_templates.sql"
    echo ""
    echo "🔍 Procurando em outros locais..."
    find /opt -name "import_templates.sql" 2>/dev/null || echo "Arquivo não encontrado"
fi

