#!/bin/bash

# Script para verificar e configurar certificado SSL

echo "=========================================="
echo "🔒 VERIFICANDO CERTIFICADO SSL"
echo "=========================================="
echo ""

DOMAIN="demo.singleone.com.br"
NGINX_CONFIG="/etc/nginx/sites-available/singleone"
CERT_PATH="/etc/letsencrypt/live/$DOMAIN"

# 1. Verificar se Let's Encrypt está instalado
echo "📋 [1/6] Verificando Certbot/Let's Encrypt..."
if command -v certbot > /dev/null 2>&1; then
    echo "   ✅ Certbot instalado"
    certbot --version
else
    echo "   ❌ Certbot NÃO está instalado"
    echo "   Execute: sudo apt update && sudo apt install -y certbot python3-certbot-nginx"
fi
echo ""

# 2. Verificar se há certificado existente
echo "📋 [2/6] Verificando certificado existente..."
if [ -d "$CERT_PATH" ]; then
    echo "   ✅ Diretório de certificado encontrado: $CERT_PATH"
    
    if [ -f "$CERT_PATH/fullchain.pem" ]; then
        echo "   ✅ Arquivo fullchain.pem encontrado"
        
        # Verificar data de expiração
        if command -v openssl > /dev/null 2>&1; then
            EXPIRY_DATE=$(openssl x509 -enddate -noout -in "$CERT_PATH/fullchain.pem" 2>/dev/null | cut -d= -f2)
            EXPIRY_EPOCH=$(date -d "$EXPIRY_DATE" +%s 2>/dev/null)
            CURRENT_EPOCH=$(date +%s)
            DAYS_LEFT=$(( ($EXPIRY_EPOCH - $CURRENT_EPOCH) / 86400 ))
            
            echo "   📅 Data de expiração: $EXPIRY_DATE"
            if [ "$DAYS_LEFT" -gt 0 ]; then
                echo "   ✅ Certificado válido por mais $DAYS_LEFT dias"
            else
                echo "   ❌ Certificado EXPIRADO!"
            fi
        fi
    else
        echo "   ❌ Arquivo fullchain.pem NÃO encontrado"
    fi
    
    if [ -f "$CERT_PATH/privkey.pem" ]; then
        echo "   ✅ Arquivo privkey.pem encontrado"
    else
        echo "   ❌ Arquivo privkey.pem NÃO encontrado"
    fi
else
    echo "   ❌ Certificado NÃO encontrado em $CERT_PATH"
    echo "   O certificado precisa ser gerado"
fi
echo ""

# 3. Verificar configuração do Nginx para HTTPS
echo "📋 [3/6] Verificando configuração do Nginx..."
if [ -f "$NGINX_CONFIG" ]; then
    if grep -q "listen 443" "$NGINX_CONFIG"; then
        echo "   ✅ Porta 443 (HTTPS) configurada"
        echo "   Configuração HTTPS:"
        grep -A 5 "listen 443" "$NGINX_CONFIG" | head -10
    else
        echo "   ❌ Porta 443 (HTTPS) NÃO está configurada"
    fi
    
    if grep -q "ssl_certificate" "$NGINX_CONFIG"; then
        echo "   ✅ SSL certificate configurado no Nginx"
        echo "   Caminho do certificado:"
        grep "ssl_certificate" "$NGINX_CONFIG" | head -2
    else
        echo "   ❌ SSL certificate NÃO está configurado no Nginx"
    fi
else
    echo "   ❌ Arquivo de configuração do Nginx não encontrado"
fi
echo ""

# 4. Verificar se a porta 443 está aberta
echo "📋 [4/6] Verificando porta 443..."
if ss -tunlp | grep -q ":443"; then
    echo "   ✅ Porta 443 está em uso"
    ss -tunlp | grep ":443" | head -1
else
    echo "   ❌ Porta 443 NÃO está em uso"
    echo "   HTTPS não está ativo"
fi
echo ""

# 5. Verificar renovação automática
echo "📋 [5/6] Verificando renovação automática..."
if systemctl list-timers | grep -q certbot; then
    echo "   ✅ Timer do Certbot encontrado"
    systemctl list-timers | grep certbot
else
    echo "   ⚠️  Timer do Certbot não encontrado"
    echo "   Verificando crontab..."
    if crontab -l 2>/dev/null | grep -q certbot; then
        echo "   ✅ Certbot encontrado no crontab"
        crontab -l 2>/dev/null | grep certbot
    else
        echo "   ❌ Renovação automática NÃO configurada"
    fi
fi
echo ""

# 6. Testar acesso HTTPS
echo "📋 [6/6] Testando acesso HTTPS..."
HTTPS_CODE=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 --max-time 10 https://$DOMAIN/ 2>/dev/null)
if [ "$HTTPS_CODE" = "200" ]; then
    echo "   ✅ HTTPS funcionando (HTTP $HTTPS_CODE)"
elif [ "$HTTPS_CODE" = "000" ]; then
    echo "   ❌ HTTPS não está acessível (timeout ou não configurado)"
else
    echo "   ⚠️  HTTPS retornou HTTP $HTTPS_CODE"
fi

# Verificar certificado via curl
echo "   Verificando certificado SSL..."
if curl -v https://$DOMAIN/ 2>&1 | grep -q "SSL certificate problem"; then
    echo "   ❌ Problema com certificado SSL detectado"
elif curl -v https://$DOMAIN/ 2>&1 | grep -q "SSL connection"; then
    echo "   ✅ Conexão SSL estabelecida"
fi
echo ""

# Resumo e recomendações
echo "=========================================="
echo "📊 RESUMO E RECOMENDAÇÕES"
echo "=========================================="
echo ""

if [ ! -d "$CERT_PATH" ] || [ ! -f "$CERT_PATH/fullchain.pem" ]; then
    echo "❌ CERTIFICADO NÃO ENCONTRADO"
    echo ""
    echo "🔧 Para gerar certificado SSL:"
    echo "   sudo bash /opt/SingleOne/SingleOne_Backend/scripts/configurar_ssl.sh"
    echo ""
elif ! grep -q "listen 443" "$NGINX_CONFIG" 2>/dev/null; then
    echo "❌ HTTPS NÃO CONFIGURADO NO NGINX"
    echo ""
    echo "🔧 Para configurar HTTPS:"
    echo "   sudo bash /opt/SingleOne/SingleOne_Backend/scripts/configurar_ssl.sh"
    echo ""
elif [ "$HTTPS_CODE" != "200" ]; then
    echo "⚠️  HTTPS NÃO ESTÁ FUNCIONANDO"
    echo ""
    echo "🔧 Para corrigir:"
    echo "   1. Verificar certificado: sudo certbot certificates"
    echo "   2. Renovar certificado: sudo certbot renew"
    echo "   3. Reiniciar Nginx: sudo systemctl restart nginx"
    echo ""
else
    echo "✅ HTTPS CONFIGURADO E FUNCIONANDO"
    echo ""
    echo "💡 Para garantir renovação automática:"
    echo "   sudo systemctl enable certbot.timer"
    echo "   sudo systemctl start certbot.timer"
    echo ""
fi

