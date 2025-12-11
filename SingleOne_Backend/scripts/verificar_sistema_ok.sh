#!/bin/bash

# Script para verificar se o sistema está funcionando corretamente

echo "=========================================="
echo "✅ VERIFICAÇÃO DO SISTEMA"
echo "=========================================="
echo ""

# 1. Verificar serviços
echo "📋 Status dos serviços:"
echo "   PostgreSQL: $(systemctl is-active postgresql)"
echo "   API:        $(systemctl is-active singleone-api)"
echo "   Nginx:      $(systemctl is-active nginx)"
echo ""

# 2. Testar API localmente
echo "📋 Testando API localmente..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5000/api/health 2>/dev/null || echo "000")

if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "404" ]; then
    echo "✅ API está respondendo (HTTP $HTTP_CODE)"
else
    echo "⚠️  API pode não estar respondendo (HTTP $HTTP_CODE)"
fi
echo ""

# 3. Testar endpoint específico
echo "📋 Testando endpoint de login..."
LOGIN_TEST=$(curl -s -o /dev/null -w "%{http_code}" -X POST http://127.0.0.1:5000/api/usuarios/Login 2>/dev/null || echo "000")
echo "   Login endpoint: HTTP $LOGIN_TEST (esperado 400 ou 200)"
echo ""

# 4. Verificar portas
echo "📋 Portas em uso:"
if ss -tunlp | grep -q ":5000"; then
    echo "✅ Porta 5000 (API): OK"
    ss -tunlp | grep ":5000" | head -1
else
    echo "❌ Porta 5000 (API): NÃO está em uso"
fi

if ss -tunlp | grep -q ":80"; then
    echo "✅ Porta 80 (Nginx): OK"
    ss -tunlp | grep ":80" | head -1
else
    echo "❌ Porta 80 (Nginx): NÃO está em uso"
fi

if ss -tunlp | grep -q ":5432"; then
    echo "✅ Porta 5432 (PostgreSQL): OK"
else
    echo "⚠️  Porta 5432 (PostgreSQL): pode não estar escutando externamente (normal)"
fi
echo ""

# 5. Verificar conexão com banco
echo "📋 Testando conexão com banco de dados..."
if sudo -u postgres psql -d singleone -c "SELECT 1;" > /dev/null 2>&1; then
    echo "✅ Conexão com banco OK"
else
    echo "❌ Erro ao conectar com banco!"
fi
echo ""

# 6. Verificar arquivos do frontend
echo "📋 Verificando frontend..."
if [ -f "/opt/SingleOne/SingleOne_Frontend/dist/SingleOne/index.html" ]; then
    echo "✅ Frontend buildado encontrado"
    echo "   Tamanho: $(du -sh /opt/SingleOne/SingleOne_Frontend/dist/SingleOne 2>/dev/null | cut -f1)"
else
    echo "⚠️  Frontend não encontrado (pode precisar fazer build)"
fi
echo ""

# 7. Verificar logs recentes da API
echo "📋 Últimas linhas dos logs da API (sem erros):"
journalctl -u singleone-api -n 10 --no-pager | grep -v "error\|Error\|ERROR\|exception\|Exception\|EXCEPTION\|fail\|Fail\|FAIL" || echo "   (nenhum erro recente)"
echo ""

# 8. Verificar se há erros nos logs
echo "📋 Verificando erros nos logs (últimas 50 linhas):"
ERROR_COUNT=$(journalctl -u singleone-api -n 50 --no-pager | grep -i "error\|exception\|fail" | wc -l)
if [ "$ERROR_COUNT" -eq 0 ]; then
    echo "✅ Nenhum erro encontrado nos logs recentes"
else
    echo "⚠️  Encontrados $ERROR_COUNT possíveis erros:"
    journalctl -u singleone-api -n 50 --no-pager | grep -i "error\|exception\|fail" | tail -5
fi
echo ""

# 9. Teste de acesso externo
echo "📋 Informações de acesso:"
EXTERNAL_IP=$(hostname -I | awk '{print $1}')
echo "   IP do servidor: $EXTERNAL_IP"
echo "   URL da API: http://$EXTERNAL_IP:5000/api/"
echo "   URL do frontend: http://$EXTERNAL_IP"
echo "   URL do cliente: https://demo.singleone.com.br"
echo ""

# 10. Resumo final
echo "=========================================="
echo "📊 RESUMO"
echo "=========================================="
echo ""
if systemctl is-active --quiet postgresql && systemctl is-active --quiet singleone-api && systemctl is-active --quiet nginx; then
    echo "✅ Todos os serviços estão rodando!"
    echo ""
    echo "🧪 Teste acessando:"
    echo "   - Frontend: http://$EXTERNAL_IP ou https://demo.singleone.com.br"
    echo "   - API: http://$EXTERNAL_IP:5000/api/"
    echo ""
else
    echo "⚠️  Alguns serviços podem não estar rodando"
    echo "   Execute: sudo bash /opt/SingleOne/SingleOne_Backend/scripts/iniciar_sistema_completo.sh"
fi
echo ""


