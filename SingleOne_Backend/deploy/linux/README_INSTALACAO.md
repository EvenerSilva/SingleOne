# Instalação SingleOne - Servidor Linux (sem Docker)

## 📋 Visão Geral

Este script automatiza a instalação completa do SingleOne em servidores Linux Ubuntu, incluindo:
- ✅ PostgreSQL (banco de dados)
- ✅ .NET 6 (runtime e SDK)
- ✅ API SingleOne (publicada e rodando como serviço systemd)
- ✅ Frontend Angular (buildado e servido via Nginx)
- ✅ Nginx (configurado com proxy para API e SPA routing)

## 🚀 Uso Rápido

### Pré-requisitos
- Servidor Linux Ubuntu (testado em 22.04)
- Acesso root ou sudo
- Conexão com internet (para baixar dependências)

### Instalação em Novo Servidor

```bash
# 1. Clonar repositório
cd /opt
git clone https://github.com/EvenerSilva/SingleOne.git
cd /opt/SingleOne/SingleOne_Backend

# 2. Executar script de instalação
sudo SITE_IP="SEU_IP_AQUI" \
     DB_PASSWORD="SUA_SENHA_AQUI" \
     bash deploy/linux/install_singleone_full.sh
```

### Variáveis de Ambiente (Opcionais)

```bash
DB_NAME="singleone"           # Nome do banco (default: singleone)
DB_USER="postgres"            # Usuário do banco (default: postgres)
DB_PASSWORD="Admin@2025"      # Senha do banco (default: Admin@2025)
SITE_DOMAIN="exemplo.com.br"  # Domínio (opcional, usado no Nginx)
SITE_IP="192.168.1.100"       # IP do servidor (fallback se SITE_DOMAIN não for definido)
USE_SSL="false"               # Habilitar SSL/HTTPS (default: false)
```

### Exemplo Completo

```bash
sudo SITE_DOMAIN="fitbank.singleone.com.br" \
     SITE_IP="173.249.37.16" \
     DB_PASSWORD="MinhaSenhaSegura123" \
     bash deploy/linux/install_singleone_full.sh
```

## 📁 Estrutura Criada

Após a instalação, o sistema estará organizado assim:

```
/opt/
├── SingleOne/                    # Código-fonte (clonado do Git)
│   ├── SingleOne_Backend/
│   ├── SingleOne_Frontend/
│   └── SingleOne_Integrator/
└── singleone-api-publish/        # API publicada (.NET)
    ├── SingleOneAPI.dll
    ├── appsettings.json
    └── ... (outros arquivos)

/etc/systemd/system/
└── singleone-api.service         # Serviço da API

/etc/nginx/sites-available/
└── singleone                     # Configuração Nginx
```

## 🗄️ Banco de Dados

O script cria automaticamente:
- Banco `singleone` no PostgreSQL
- Todas as tabelas (via `01. Criar Tabelas.sql`)
- Todas as views (via `02. Criar Views.sql`)
- Templates iniciais (via `03. Importar_templates.sql`)

**Nota:** Alguns erros em views durante a criação são normais (devido a diferenças de case ou tabelas opcionais). O script continua mesmo com esses erros.

## 🔧 Verificação Pós-Instalação

```bash
# Verificar status da API
systemctl status singleone-api

# Testar API localmente
curl http://localhost:5000/swagger

# Testar frontend via Nginx
curl -I http://localhost

# Ver logs da API
journalctl -u singleone-api -f
```

## 🔄 Atualizar Sistema Existente

Se você já tem um servidor instalado e quer atualizar:

```bash
cd /opt/SingleOne
git pull origin main
cd SingleOne_Backend

# Reexecutar apenas a parte que precisa (exemplo: atualizar API)
cd SingleOneAPI
dotnet publish -c Release -o /opt/singleone-api-publish
systemctl restart singleone-api
```

## 📦 Copiar Dados de Outro Servidor

Para copiar dados do banco de um servidor para outro:

```bash
# No servidor ORIGEM
sudo -u postgres pg_dump -Fc -d singleone > /tmp/singleone_backup.dump

# Copiar arquivo para servidor DESTINO (via SCP)
scp /tmp/singleone_backup.dump root@SERVIDOR_DESTINO:/tmp/

# No servidor DESTINO
sudo -u postgres pg_restore -c -d singleone /tmp/singleone_backup.dump
```

## 🔧 Scripts de correção / migração

- **Novos ambientes:** o script `install_singleone_full.sh` já executa automaticamente a correção `corrigir_patrimonio_contestoes_equipamento_nullable.sql` após criar tabelas/views, então **novas instalações** já saem com `equipamento_id` nullable em `patrimonio_contestoes` (Auto Inventário funciona sem erro).

Em **ambientes já existentes** (instalados antes dessa alteração), rode manualmente se aparecer o erro ao criar Auto Inventário:

| Script | Quando usar |
|--------|-------------|
| `corrigir_coluna_tipo_contestacao.sql` | Adiciona coluna `tipo_contestacao` em `patrimonio_contestoes` |
| `corrigir_patrimonio_contestoes_equipamento_nullable.sql` | Corrige erro ao **criar Auto Inventário**: "null value in column equipamento_id". Permite `equipamento_id` NULL (equipamento vinculado depois). |

Exemplo (ajuste o caminho conforme o diretório atual):

```bash
cd /opt/SingleOne/SingleOne_Backend
sudo -u postgres psql -d singleone -f deploy/linux/corrigir_patrimonio_contestoes_equipamento_nullable.sql
```

## 🐛 Troubleshooting

### Erro: "npm ci" falha
- **Solução:** O script já usa `npm install --legacy-peer-deps` para resolver conflitos do Angular 10

### Erro: "digital envelope routines::unsupported"
- **Solução:** O script já define `NODE_OPTIONS=--openssl-legacy-provider` antes do build

### Erro: Views não criadas
- **Normal:** Alguns erros em views são esperados (diferenças de case). O sistema funciona mesmo assim.

### API não inicia
```bash
# Ver logs detalhados
journalctl -u singleone-api -n 50

# Verificar se o banco está acessível
sudo -u postgres psql -d singleone -c "SELECT 1;"
```

## 📝 Notas Importantes

1. **Senha do Banco:** A senha padrão é `Admin@2025`. Altere via variável `DB_PASSWORD` para produção.

2. **SSL/HTTPS:** Para habilitar SSL, defina `USE_SSL="true"` e configure certificados Let's Encrypt manualmente.

3. **Firewall:** Certifique-se de que as portas 80 (HTTP) e 443 (HTTPS, se habilitado) estão abertas.

4. **Backup:** Configure backups regulares do banco PostgreSQL.

## 🔗 Links Úteis

- Repositório: https://github.com/EvenerSilva/SingleOne
- API Swagger: http://SEU_IP:5000/swagger
- Hangfire Dashboard: http://SEU_IP/hangfire

---

**Última atualização:** Dezembro 2025

