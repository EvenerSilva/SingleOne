# 🚀 Início Rápido - SingleOne Integrator

## ⚡ Setup em 5 Minutos

### 1️⃣ **Criar Tabelas no Banco**

```bash
# PostgreSQL
psql -U postgres -d singleone -f Database/01_CREATE_TABLES.sql

# Ou execute manualmente:
# Abra Database/01_CREATE_TABLES.sql e execute no seu cliente SQL
```

### 2️⃣ **Gerar Credenciais de Teste**

Execute o código abaixo ou use o tool:

```csharp
using SingleOneIntegrator.Helpers;

var apiKey = ApiKeyGenerator.GenerateApiKey(isProduction: false);
var apiSecret = ApiKeyGenerator.GenerateApiSecret();

Console.WriteLine($"API Key: {apiKey}");
Console.WriteLine($"API Secret: {apiSecret}");

// Exemplo de saída:
// API Key: sk_test_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
// API Secret: whsec_x1y2z3a4b5c6d7e8f9g0h1i2j3k4l5m6n7o8
```

### 3️⃣ **Inserir Credenciais no Banco**

```sql
INSERT INTO "ClienteIntegracao" 
("ClienteId", "ApiKey", "ApiSecret", "Ativo", "DataCriacao", "Observacoes")
VALUES 
(1, 'sk_test_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6', 'whsec_x1y2z3a4b5c6d7e8f9g0h1i2j3k4l5m6n7o8', true, NOW(), 'Cliente de teste');
```

### 4️⃣ **Configurar `appsettings.json`**

```json
{
  "DatabaseOptions": {
    "ProviderName": "Npgsql",
    "ConnectionString": "Host=localhost;Database=singleone;Username=postgres;Password=sua_senha"
  }
}
```

### 5️⃣ **Executar o Sistema**

```bash
cd SingleOneIntegrator
dotnet restore
dotnet run
```

Acesse: http://localhost:5000

---

## ✅ Testar a API

### Opção 1: Usar o Exemplo em C#

```bash
cd SingleOneIntegrator
dotnet run --project Examples/TesteIntegracaoSimples.cs
```

### Opção 2: Usar cURL

**⚠️ Importante**: Você precisa gerar a assinatura HMAC primeiro!

#### Passo 1: Gerar Assinatura

Execute este código C# para gerar a assinatura:

```csharp
using System;
using System.Security.Cryptography;
using System.Text;

var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
var body = @"{""timestamp"":""2025-10-28T10:30:00Z"",""tipoOperacao"":""INCREMENTAL"",""colaboradores"":[{""nomeCompleto"":""João Silva"",""cpf"":""12345678901"",""email"":""joao@empresa.com"",""cargo"":""Analista"",""status"":""ATIVO""}]}";
var apiSecret = "whsec_x1y2z3a4b5c6d7e8f9g0h1i2j3k4l5m6n7o8";

var payload = $"{timestamp}.{body}";
var keyBytes = Encoding.UTF8.GetBytes(apiSecret);
var payloadBytes = Encoding.UTF8.GetBytes(payload);

using (var hmac = new HMACSHA256(keyBytes))
{
    var hash = hmac.ComputeHash(payloadBytes);
    var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
    Console.WriteLine($"Timestamp: {timestamp}");
    Console.WriteLine($"Signature: sha256={signature}");
}
```

#### Passo 2: Fazer Requisição

```bash
curl -X POST http://localhost:5000/api/integracao/folha \
  -H "X-SingleOne-ApiKey: sk_test_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6" \
  -H "X-SingleOne-Timestamp: [TIMESTAMP_GERADO]" \
  -H "X-SingleOne-Signature: sha256=[SIGNATURE_GERADA]" \
  -H "Content-Type: application/json" \
  -d '{
    "timestamp": "2025-10-28T10:30:00Z",
    "tipoOperacao": "INCREMENTAL",
    "colaboradores": [
      {
        "nomeCompleto": "João Silva",
        "cpf": "12345678901",
        "email": "joao@empresa.com",
        "cargo": "Analista",
        "status": "ATIVO"
      }
    ]
  }'
```

### Opção 3: Usar Swagger UI

1. Acesse: http://localhost:5000
2. Vá em `POST /api/integracao/folha`
3. Clique em "Try it out"
4. ⚠️ **ATENÇÃO**: Swagger não suporta autenticação HMAC nativamente
5. Use Postman ou código customizado

---

## 📊 Verificar Logs

### Logs de Integração

```sql
-- Últimas 10 integrações
SELECT 
    "IntegracaoId",
    "ClienteId",
    "DataHora",
    "ColaboradoresProcessados",
    "Sucesso",
    "Mensagem"
FROM "IntegracaoFolhaLog"
ORDER BY "DataHora" DESC
LIMIT 10;
```

### Estatísticas

```sql
-- Estatísticas por cliente
SELECT 
    "ClienteId",
    COUNT(*) as "TotalIntegracoes",
    SUM("ColaboradoresProcessados") as "TotalColaboradores",
    AVG("TempoProcessamento") as "TempoMedio_ms",
    SUM(CASE WHEN "Sucesso" THEN 1 ELSE 0 END)::float / COUNT(*) * 100 as "TaxaSucesso_%"
FROM "IntegracaoFolhaLog"
GROUP BY "ClienteId";
```

---

## 🔍 Troubleshooting Rápido

### ❌ Erro: "Timestamp expirado"
**Solução**: Sincronize o relógio do servidor
```bash
# Linux
sudo ntpdate pool.ntp.org

# Windows
w32tm /resync
```

### ❌ Erro: "Assinatura HMAC inválida"
**Solução**: Verifique:
1. API Secret está correto
2. Payload está idêntico (sem espaços extras)
3. Formato: `timestamp.body`

### ❌ Erro: "API Key inválida"
**Solução**: Verifique no banco:
```sql
SELECT * FROM "ClienteIntegracao" WHERE "ApiKey" = 'sua_api_key';
```

### ❌ Worker não funciona
**Solução**: Verifique VIEW no banco:
```sql
SELECT * FROM "VW_INVENTARIO_USUARIOS" LIMIT 5;
```

### ❌ RabbitMQ não conecta
**Solução**: Verifique se está rodando:
```bash
# Status
rabbitmq-server status

# Iniciar
rabbitmq-server start
```

---

## 📚 Próximos Passos

1. ✅ **Leia a documentação completa**: [GUIA_INTEGRACAO.md](Documentation/GUIA_INTEGRACAO.md)
2. ✅ **Configure autenticação de produção**: Gere API Keys com `--production`
3. ✅ **Configure IP Whitelist**: Atualize campo `IpWhitelist` na tabela
4. ✅ **Monitore logs**: Configure alertas para integrações falhadas
5. ✅ **Teste carga**: Simule envio de 1000 colaboradores

---

## 📞 Ajuda

- 📖 **Documentação Completa**: [README.md](README.md)
- 🔐 **Guia de Segurança**: [GUIA_INTEGRACAO.md](Documentation/GUIA_INTEGRACAO.md)
- 💬 **Suporte**: suporte@singleone.com.br
- 📱 **WhatsApp**: (11) 98765-4321

---

## ✨ Dica Pro

Para facilitar testes, crie um script que gera automaticamente a assinatura HMAC:

```bash
# test-api.sh
#!/bin/bash

API_KEY="sk_test_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
API_SECRET="whsec_x1y2z3a4b5c6d7e8f9g0h1i2j3k4l5m6n7o8"
TIMESTAMP=$(date +%s)
BODY='{"timestamp":"2025-10-28T10:30:00Z","tipoOperacao":"INCREMENTAL","colaboradores":[...]}'

# Gerar HMAC (requer OpenSSL)
PAYLOAD="$TIMESTAMP.$BODY"
SIGNATURE=$(echo -n "$PAYLOAD" | openssl dgst -sha256 -hmac "$API_SECRET" | sed 's/^.* //')

echo "Timestamp: $TIMESTAMP"
echo "Signature: sha256=$SIGNATURE"

# Fazer requisição
curl -X POST http://localhost:5000/api/integracao/folha \
  -H "X-SingleOne-ApiKey: $API_KEY" \
  -H "X-SingleOne-Timestamp: $TIMESTAMP" \
  -H "X-SingleOne-Signature: sha256=$SIGNATURE" \
  -H "Content-Type: application/json" \
  -d "$BODY"
```

---

**🎉 Pronto! Seu SingleOne Integrator está configurado e funcionando!**


