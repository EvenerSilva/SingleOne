#!/bin/bash
# =====================================================
# CORRIGIR HANGFIRE NO CONTABO
# =====================================================
# Este script corrige as tabelas do Hangfire removendo
# todas e permitindo que o Hangfire as recrie automaticamente

echo "🔧 Corrigindo tabelas do Hangfire..."
echo ""

# Verificar se PostgreSQL está rodando em Docker
if docker ps | grep -q singleone-postgres; then
    echo "✅ PostgreSQL encontrado no Docker"
    echo ""
    echo "📋 Executando script de correção..."
    docker exec -i singleone-postgres psql -U postgres -d singleone < corrigir_hangfire_contabo.sql
    
    if [ $? -eq 0 ]; then
        echo ""
        echo "✅ Tabelas do Hangfire removidas com sucesso!"
        echo ""
        echo "🔄 Próximos passos:"
        echo "   1. Reinicie o backend: cd SingleOne_Backend && docker compose restart backend"
        echo "   2. O Hangfire criará automaticamente todas as tabelas com a estrutura correta"
    else
        echo ""
        echo "❌ Erro ao executar script de correção"
        exit 1
    fi
else
    echo "❌ Container PostgreSQL não encontrado"
    echo "   Verifique se o container está rodando: docker ps"
    exit 1
fi

