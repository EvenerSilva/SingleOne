# Troubleshooting - Erro ao Subir Backend

## 1. Verificar o caminho correto

O `docker-compose.yml` está em `SingleOne_Backend/`, então você precisa executar os comandos de dentro dessa pasta:

```bash
cd /opt/SingleOne/SingleOne_Backend
```

## 2. Verificar logs do build

Execute o build com verbose para ver o erro completo:

```bash
cd /opt/SingleOne/SingleOne_Backend
docker compose build --no-cache --progress=plain backend 2>&1 | tee build.log
```

## 3. Verificar logs do container

Se o build funcionou mas o container não sobe:

```bash
# Ver logs do backend
docker compose logs backend

# Ver logs em tempo real
docker compose logs -f backend

# Ver últimas 100 linhas
docker compose logs --tail=100 backend
```

## 4. Verificar se o container está rodando

```bash
docker compose ps
```

## 5. Erros comuns e soluções

### Erro: "context: ." não encontra SingleOneAPI
**Solução:** Certifique-se de estar em `/opt/SingleOne/SingleOne_Backend` ao executar `docker compose`

### Erro: "Cannot find Dockerfile"
**Solução:** Verifique se o Dockerfile existe em `/opt/SingleOne/SingleOne_Backend/Dockerfile`

### Erro: "dotnet restore failed"
**Solução:** Pode ser problema de rede ou dependências. Tente:
```bash
docker compose build --no-cache backend
```

### Erro: "database does not exist"
**Solução:** O banco precisa estar criado. Execute:
```bash
docker exec -it singleone-postgres psql -U postgres -c "CREATE DATABASE singleone;"
```

### Erro: "Connection refused" ou "timeout"
**Solução:** Verifique se o PostgreSQL está rodando:
```bash
docker compose ps postgres
docker compose logs postgres
```

## 6. Comandos completos de deploy

```bash
# 1. Ir para o diretório correto
cd /opt/SingleOne/SingleOne_Backend

# 2. Atualizar código
git pull origin main

# 3. Parar backend
docker compose stop backend

# 4. Remover container antigo (se necessário)
docker compose rm -f backend

# 5. Rebuild sem cache
docker compose build --no-cache backend

# 6. Subir backend
docker compose up -d backend

# 7. Ver logs
docker compose logs -f backend
```

## 7. Se ainda não funcionar

Envie os seguintes logs:

```bash
# Logs do build
docker compose build --no-cache backend 2>&1 | tail -50

# Logs do container
docker compose logs --tail=100 backend

# Status dos containers
docker compose ps

# Verificar se o Dockerfile existe
ls -la /opt/SingleOne/SingleOne_Backend/Dockerfile

# Verificar estrutura do projeto
ls -la /opt/SingleOne/SingleOne_Backend/SingleOneAPI/
```

