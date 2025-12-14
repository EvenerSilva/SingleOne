#!/bin/bash
# Script para forçar rebuild completo da API

echo "=== Limpando e reconstruindo API ==="

cd /opt/SingleOne/SingleOne_Backend

# 1. Atualizar código
echo "1. Atualizando código do Git..."
git pull origin main

# 2. Limpar completamente
echo "2. Limpando build anterior..."
cd SingleOneAPI
dotnet clean
rm -rf bin/ obj/

# 3. Restaurar dependências
echo "3. Restaurando dependências..."
dotnet restore

# 4. Verificar modelo PatrimonioContestacao
echo "4. Verificando modelo PatrimonioContestacao..."
grep -n "EquipamentoId" Models/PatrimonioContestacao.cs | head -2

# 5. Compilar
echo "5. Compilando..."
dotnet build -c Release 2>&1 | tee /tmp/build_output.log

# Verificar erros
if grep -q "error CS0037" /tmp/build_output.log; then
    echo "❌ ERRO ENCONTRADO: Verificando linha 392 de PatrimonioNegocio.cs..."
    sed -n '390,395p' Negocios/PatrimonioNegocio.cs
    echo ""
    echo "Verificando tipo de EquipamentoId no modelo:"
    grep -A 1 "EquipamentoId" Models/PatrimonioContestacao.cs
    exit 1
fi

# 6. Publicar
echo "6. Publicando..."
dotnet publish -c Release -o /opt/singleone-api-publish

# 7. Reiniciar serviço
echo "7. Reiniciando serviço..."
sudo systemctl restart singleone-api

# 8. Verificar logs
echo "8. Verificando logs..."
sleep 3
journalctl -u singleone-api -n 20 --no-pager | tail -10

echo "✅ Rebuild completo finalizado!"
