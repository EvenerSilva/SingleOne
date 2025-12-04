# 🚀 SingleOne Integrator

Sistema híbrido (Worker + Web API) para integração de folha de pagamento com o SingleOne.

## 📋 Visão Geral

O **SingleOne Integrator** oferece duas formas de integração:

1. **🔄 Worker Service (VIEW)**: Leitura periódica de uma VIEW no banco de dados
2. **📡 Web API (REST)**: Recebe dados via API REST com autenticação HMAC

## 🏗️ Arquitetura

```
┌─────────────────────────────────────────────────┐
│         SingleOne Integrator                     │
│                                                  │
│  ┌──────────────┐         ┌─────────────────┐  │
│  │   Worker     │         │    Web API      │  │
│  │   Service    │         │  (Controller)   │  │
│  │              │         │                 │  │
│  │  Lê VIEW     │         │ Recebe POST     │  │
│  │  a cada 10s  │         │ com HMAC Auth   │  │
│  └──────┬───────┘         └────────┬────────┘  │
│         │                          │            │
│         └──────────┬───────────────┘            │
│                    ▼                             │
│           ┌─────────────────┐                   │
│           │   RabbitMQ      │                   │
│           │   (Fila)        │                   │
│           └─────────────────┘                   │
└─────────────────────────────────────────────────┘
                    │
                    ▼
        ┌──────────────────────┐
        │  SingleOne Backend   │
        │  (Consumidor)        │
        └──────────────────────┘
```

## ✨ Funcionalidades

### Worker Service (VIEW)
- ✅ Leitura automática de VIEW `VW_INVENTARIO_USUARIOS`
- ✅ Detecção de mudanças via cache
- ✅ Envio apenas de diferenças para RabbitMQ
- ✅ Suporte a PostgreSQL, MySQL e SQL Server

### Web API (REST)
- ✅ Endpoint REST `/api/integracao/folha`
- ✅ Autenticação HMAC-SHA256
- ✅ Validação de timestamp (anti-replay)
- ✅ Rate limiting (10 req/min)
- ✅ IP Whitelist (opcional)
- ✅ Logs de auditoria completos
- ✅ Swagger UI para documentação
- ✅ Suporte a FULL_SYNC e INCREMENTAL

## 🔐 Segurança

- 🔒 **HTTPS Obrigatório**
- 🔑 **Autenticação HMAC-SHA256**
- ⏱️ **Timestamp Validation** (janela de 5 minutos)
- 🚦 **Rate Limiting** (10 requisições/minuto)
- 🌐 **IP Whitelist** (opcional)
- 📝 **Logs de Auditoria** completos
- ✅ **Validação de CPF**
- 🗄️ **CPF criptografado no banco**

## 🛠️ Instalação

### Pré-requisitos

- .NET 7.0 SDK
- PostgreSQL (ou MySQL/SQL Server)
- RabbitMQ

### 1. Clonar repositório

```bash
git clone https://github.com/singleone/integrator.git
cd integrator/SingleOneIntegrator
```

### 2. Configurar `appsettings.json`

```json
{
  "DatabaseOptions": {
    "ProviderName": "Npgsql",
    "ConnectionString": "Host=localhost;Database=singleone;Username=postgres;Password=sua_senha"
  }
}
```

### 3. Criar tabelas no banco

```bash
psql -U postgres -d singleone -f Database/01_CREATE_TABLES.sql
```

### 4. Restaurar pacotes

```bash
dotnet restore
```

### 5. Executar

```bash
dotnet run
```

Acesse:
- **Swagger UI**: http://localhost:5000
- **API Health**: http://localhost:5000/api/integracao/folha/health

## 📡 Usando a API

### 1. Gerar Credenciais

Use o utilitário de geração de API Keys:

```bash
dotnet run --project Tools/ApiKeyGenerator
```

Ou programaticamente:

```csharp
using SingleOneIntegrator.Helpers;

var apiKey = ApiKeyGenerator.GenerateApiKey(isProduction: true);
var apiSecret = ApiKeyGenerator.GenerateApiSecret();

Console.WriteLine($"API Key: {apiKey}");
Console.WriteLine($"API Secret: {apiSecret}");
```

### 2. Inserir no banco

```sql
INSERT INTO "ClienteIntegracao" 
("ClienteId", "ApiKey", "ApiSecret", "Ativo", "DataCriacao")
VALUES 
(1, 'sk_live_...', 'whsec_...', true, NOW());
```

### 3. Fazer requisição

```bash
curl -X POST https://singleone.com.br/api/integracao/folha \
  -H "X-SingleOne-ApiKey: sk_live_..." \
  -H "X-SingleOne-Timestamp: 1698765432" \
  -H "X-SingleOne-Signature: sha256=..." \
  -H "Content-Type: application/json" \
  -d '{
    "timestamp": "2025-10-28T10:30:00Z",
    "tipoOperacao": "INCREMENTAL",
    "colaboradores": [...]
  }'
```

Veja exemplos completos em [Documentation/GUIA_INTEGRACAO.md](Documentation/GUIA_INTEGRACAO.md)

## 📊 Monitoramento

### Logs

Logs são gravados em:
- Console (stdout)
- Arquivos em `C:\SingleOne\Logs\Integrador\` (configurável)

### Métricas

- Total de integrações por cliente
- Taxa de sucesso/erro
- Tempo médio de processamento
- Colaboradores processados

Query exemplo:

```sql
SELECT 
    "ClienteId",
    COUNT(*) as "TotalIntegracoes",
    SUM("ColaboradoresProcessados") as "TotalColaboradores",
    AVG("TempoProcessamento") as "TempoMedio",
    SUM(CASE WHEN "Sucesso" = true THEN 1 ELSE 0 END)::float / COUNT(*) * 100 as "TaxaSucesso"
FROM "IntegracaoFolhaLog"
WHERE "DataHora" >= NOW() - INTERVAL '30 days'
GROUP BY "ClienteId";
```

## 🧪 Testes

### Teste de Health Check

```bash
curl http://localhost:5000/api/integracao/folha/health
```

### Teste de Integração

Veja exemplos em Python, C#, PHP em [Documentation/GUIA_INTEGRACAO.md](Documentation/GUIA_INTEGRACAO.md)

## 📁 Estrutura do Projeto

```
SingleOneIntegrator/
├── Controllers/          # Controllers da Web API
│   └── IntegracaoFolhaController.cs
├── Data/                 # DbContext e providers
├── Database/             # Scripts SQL
│   └── 01_CREATE_TABLES.sql
├── Documentation/        # Documentação
│   └── GUIA_INTEGRACAO.md
├── Helpers/              # Utilities
│   ├── CpfValidator.cs
│   ├── HmacHelper.cs
│   ├── ApiKeyGenerator.cs
│   └── VwInventarioUsuarioComparer.cs
├── Middleware/           # Middlewares
│   └── HmacAuthenticationMiddleware.cs
├── Models/               # Modelos de dados
│   ├── ClienteIntegracao.cs
│   ├── IntegracaoFolhaLog.cs
│   ├── VwInventarioUsuario.cs
│   └── DTOs/
├── Repository/           # Repositórios
│   ├── Colaborador/
│   └── Integracao/
├── Services/             # Serviços de negócio
│   ├── IntegracaoFolhaService.cs
│   └── RateLimitService.cs
├── Worker.cs             # Worker Service (VIEW)
├── Program.cs            # Entry point
└── appsettings.json      # Configurações
```

## 🔧 Configurações Avançadas

### Rate Limiting

Edite em `IntegracaoFolhaController.cs`:

```csharp
await _rateLimitService.CheckLimit(
    cliente.ApiKey, 
    maxRequests: 10,      // Altere aqui
    windowSeconds: 60     // Altere aqui
)
```

### Timestamp Window

Edite em `HmacHelper.cs`:

```csharp
ValidateTimestamp(timestamp, maxDifferenceSeconds: 300) // 5 minutos
```

### Tamanho Máximo do Payload

Edite em `IntegracaoFolhaController.cs`:

```csharp
if (request.Colaboradores.Count > 1000) // Altere aqui
```

## 🐛 Troubleshooting

### Worker não está lendo VIEW

1. Verifique connection string
2. Verifique se VIEW existe: `SELECT * FROM "VW_INVENTARIO_USUARIOS" LIMIT 1`
3. Verifique logs em `C:\SingleOne\Logs\Integrador\`

### API retorna 401 (Unauthorized)

1. Verifique se API Key está ativa no banco
2. Verifique geração da assinatura HMAC
3. Verifique se timestamp está sincronizado (NTP)
4. Verifique se IP está na whitelist (se configurado)

### RabbitMQ não conecta

1. Verifique se RabbitMQ está rodando: `rabbitmq-server`
2. Altere hostname em `Worker.cs` e `IntegracaoFolhaService.cs` se necessário

## 📞 Suporte

**Email:** suporte@singleone.com.br  
**WhatsApp:** (11) 98765-4321  
**Portal:** https://suporte.singleone.com.br

## 📄 Licença

© 2025 SingleOne - Todos os direitos reservados

## 👥 Contribuidores

- **Equipe SingleOne** - Desenvolvimento inicial

## 🗺️ Roadmap

- [ ] Dashboard de monitoramento em tempo real
- [ ] Suporte a outros bancos de dados (Oracle, MongoDB)
- [ ] Webhooks de notificação
- [ ] Validação de ranges CIDR para IP Whitelist
- [ ] Métricas Prometheus/Grafana
- [ ] Docker Compose para deploy simplificado
- [ ] Rate limiting por tenant
- [ ] Retry automático com backoff exponencial


