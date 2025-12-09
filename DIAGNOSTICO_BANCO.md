# 🔍 Diagnóstico do Banco de Dados SingleOne

## Problema
O banco `singleone` não aparece no PGAdmin, mesmo que o sistema esteja funcionando.

## Possíveis Causas

### 1. Banco foi deletado ou volume foi recriado
Se o volume do PostgreSQL foi deletado ou o container foi recriado, o banco pode ter sido perdido.

### 2. Script de inicialização não executou
O script `init_db_atualizado.sql` só executa automaticamente na **primeira inicialização** do volume PostgreSQL (via `/docker-entrypoint-initdb.d/`).

### 3. Conexão do PGAdmin incorreta
O PGAdmin pode estar conectado ao banco errado ou com credenciais incorretas.

## 🔧 Solução: Verificar e Recriar o Banco

### Opção 1: Usar o script de verificação (Recomendado)

```bash
cd /opt/SingleOne
chmod +x verificar_e_recriar_banco.sh
./verificar_e_recriar_banco.sh
```

### Opção 2: Verificar manualmente via Docker

```bash
# Verificar se o banco existe
docker exec -it singleone-postgres psql -U postgres -c "\l" | grep singleone

# Se não existir, criar e executar script
docker exec -it singleone-postgres psql -U postgres -c "CREATE DATABASE singleone;"
docker exec -i singleone-postgres psql -U postgres -d singleone < init_db_atualizado.sql
```

### Opção 3: Conectar via psql direto

```bash
# Conectar ao PostgreSQL
docker exec -it singleone-postgres psql -U postgres

# Dentro do psql:
\l                    # Listar bancos
\c singleone          # Conectar ao banco singleone
\dt                   # Listar tabelas
\q                    # Sair
```

## 🔑 Credenciais Padrão

Baseado no `docker-compose.yml`:

- **Host**: `postgres` (dentro do Docker) ou `localhost` (do host)
- **Port**: `5432`
- **User**: `postgres`
- **Password**: `postgres` (padrão, pode estar em variável de ambiente `DB_PASSWORD`)
- **Database**: `singleone`

## 📋 Configuração no PGAdmin

1. **Servidor**: IP do servidor Contabo ou `localhost` se local
2. **Porta**: `5432`
3. **Database**: `postgres` (para conectar primeiro) ou `singleone` (se existir)
4. **Username**: `postgres`
5. **Password**: Verificar variável `DB_PASSWORD` no servidor

## 🔍 Verificar Variáveis de Ambiente no Servidor

```bash
# Ver variáveis do container PostgreSQL
docker exec singleone-postgres env | grep -E "POSTGRES|DB_"

# Ver variáveis do container Backend
docker exec singleone-backend env | grep -E "DB_"
```

## ⚠️ Se o Banco Realmente Não Existe

1. **Criar o banco**:
   ```bash
   docker exec -it singleone-postgres psql -U postgres -c "CREATE DATABASE singleone;"
   ```

2. **Executar script de inicialização**:
   ```bash
   cd /opt/SingleOne
   docker exec -i singleone-postgres psql -U postgres -d singleone < init_db_atualizado.sql
   ```

3. **Verificar resultado**:
   ```bash
   docker exec -it singleone-postgres psql -U postgres -d singleone -c "\dt" | wc -l
   ```

## 🚨 Problema Comum: Volume Deletado

Se o volume `postgres_data` foi deletado:

```bash
# Verificar volumes
docker volume ls | grep postgres

# Se não existir, recriar:
docker-compose up -d postgres
# Aguardar inicialização
sleep 10
# Executar script
docker exec -i singleone-postgres psql -U postgres -d singleone < init_db_atualizado.sql
```

