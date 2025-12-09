# 🚀 COMANDOS DIRETOS PARA RECRIAR BANCO NO CONTABO

## Problema
O banco `singleone` não existe e os scripts precisam ser atualizados.

## Solução Rápida (Execute no servidor)

### 1. Atualizar scripts primeiro:
```bash
cd /opt/SingleOne
git stash
git pull origin main
chmod +x recriar_banco_contabo.sh verificar_banco_contabo.sh
```

### 2. OU usar comandos diretos do Docker:

```bash
cd /opt/SingleOne

# Verificar se o container está rodando
docker ps | grep postgres

# Se não estiver rodando, iniciar
docker start singleone-postgres

# Verificar se o banco existe
docker exec singleone-postgres psql -U postgres -d postgres -c "\l" | grep singleone

# Se não existir, criar o banco
docker exec singleone-postgres psql -U postgres -d postgres -c "CREATE DATABASE singleone;"

# Executar script de inicialização
docker exec -i singleone-postgres psql -U postgres -d singleone < init_db_atualizado.sql
```

### 3. OU tudo em um comando:

```bash
cd /opt/SingleOne && \
docker start singleone-postgres 2>/dev/null; \
docker exec singleone-postgres psql -U postgres -d postgres -c "DROP DATABASE IF EXISTS singleone;" 2>/dev/null; \
docker exec singleone-postgres psql -U postgres -d postgres -c "CREATE DATABASE singleone;" && \
docker exec -i singleone-postgres psql -U postgres -d singleone < init_db_atualizado.sql && \
echo "✅ Banco recriado com sucesso!"
```

### 4. Verificar resultado:

```bash
# Contar tabelas
docker exec singleone-postgres psql -U postgres -d singleone -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';"

# Contar views
docker exec singleone-postgres psql -U postgres -d singleone -c "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';"
```

## Credenciais

- **Container**: `singleone-postgres`
- **User**: `postgres`
- **Password**: `postgres` (padrão do docker-compose) ou verificar em `docker-compose.yml`
- **Database**: `singleone`

## Se o container tiver outro nome:

```bash
# Listar containers postgres
docker ps -a | grep postgres

# Substituir "singleone-postgres" pelo nome do seu container
```

