#!/bin/bash

# Script para diagnosticar e corrigir problemas com a API

echo "=========================================="
echo "🔍 DIAGNÓSTICO E CORREÇÃO DA API"
echo "=========================================="
echo ""

# 1. Verificar status do serviço
echo "📋 Verificando status do serviço..."
systemctl status singleone-api --no-pager -l | head -20
echo ""

# 2. Verificar logs recentes
echo "📋 Últimos logs do serviço (últimas 50 linhas):"
journalctl -u singleone-api -n 50 --no-pager
echo ""

# 3. Verificar se a porta está em uso
echo "📋 Verificando se a porta 5000 está em uso..."
if ss -tunlp | grep -q ":5000"; then
    echo "✅ Porta 5000 está em uso"
    ss -tunlp | grep ":5000"
else
    echo "❌ Porta 5000 NÃO está em uso - API não está escutando!"
fi
echo ""

# 4. Testar conexão local
echo "📋 Testando conexão local na API..."
if curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5000/api/health 2>/dev/null | grep -q "200\|404"; then
    echo "✅ API está respondendo (mesmo que com 404, significa que está rodando)"
else
    echo "❌ API NÃO está respondendo!"
    echo "   Tentando curl completo..."
    curl -v http://127.0.0.1:5000/api/health 2>&1 | head -20
fi
echo ""

# 5. Verificar arquivos publicados
echo "📋 Verificando arquivos publicados..."
if [ -f "/opt/singleone-api-publish/SingleOneAPI.dll" ]; then
    echo "✅ SingleOneAPI.dll encontrado"
    ls -lh /opt/singleone-api-publish/SingleOneAPI.dll
else
    echo "❌ SingleOneAPI.dll NÃO encontrado!"
    echo "   O publish pode ter falhado."
fi
echo ""

# 6. Verificar permissões
echo "📋 Verificando permissões do diretório..."
ls -ld /opt/singleone-api-publish/
echo ""

# 7. Verificar variáveis de ambiente
echo "📋 Verificando variáveis de ambiente do serviço..."
systemctl show singleone-api | grep -E "Environment|ExecStart"
echo ""

# 8. Tentar reiniciar o serviço
echo "🔄 Tentando reiniciar o serviço..."
systemctl restart singleone-api
sleep 3

# 9. Verificar status após reinício
echo "📋 Status após reinício:"
systemctl status singleone-api --no-pager -l | head -15
echo ""

# 10. Verificar logs após reinício
echo "📋 Logs após reinício (últimas 20 linhas):"
journalctl -u singleone-api -n 20 --no-pager
echo ""

# 11. Testar novamente
echo "📋 Testando conexão novamente..."
sleep 2
if curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5000/api/health 2>/dev/null | grep -q "200\|404"; then
    echo "✅ API está respondendo agora!"
else
    echo "❌ API ainda não está respondendo!"
    echo ""
    echo "🔧 TENTANDO CORREÇÕES AUTOMÁTICAS..."
    echo ""
    
    # Parar serviço
    systemctl stop singleone-api
    
    # Verificar se há processos travados
    pkill -f "SingleOneAPI.dll" || true
    sleep 2
    
    # Verificar se há arquivos travados
    lsof /opt/singleone-api-publish/SingleOneAPI.dll 2>/dev/null || echo "Nenhum processo usando o arquivo"
    
    # Limpar diretório de publish (cuidado!)
    # Não vamos fazer isso automaticamente, apenas sugerir
    
    # Reiniciar
    systemctl start singleone-api
    sleep 3
    
    # Verificar novamente
    systemctl status singleone-api --no-pager -l | head -15
    echo ""
    journalctl -u singleone-api -n 20 --no-pager
fi

echo ""
echo "=========================================="
echo "✅ DIAGNÓSTICO CONCLUÍDO"
echo "=========================================="
echo ""
echo "📋 Se a API ainda não estiver funcionando:"
echo "   1. Verifique os logs completos: journalctl -u singleone-api -f"
echo "   2. Verifique se há erros de conexão com o banco"
echo "   3. Verifique se o .NET runtime está instalado: dotnet --version"
echo "   4. Tente fazer um novo publish: cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI && dotnet publish -c Release -o /opt/singleone-api-publish"
echo ""


