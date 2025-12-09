# 🔧 RECRIAÇÃO DO BANCO SINGLEONE NO CONTABO

## 📋 Credenciais do Banco

Baseado nos arquivos de configuração, as credenciais padrão são:

- **Host**: `localhost` (ou o IP do servidor)
- **Port**: `5432`
- **User**: `postgres`
- **Password**: `postgres` (padrão do docker-compose) ou `Admin@2025` (padrão do Startup.cs)
- **Database**: `singleone`

⚠️ **IMPORTANTE**: Verifique qual senha está configurada no servidor Contabo!

## 🚀 Comandos para Executar no Servidor

### 1. Verificar Status do Banco

```bash
cd /opt/SingleOne
chmod +x verificar_banco_contabo.sh
./verificar_banco_contabo.sh
```

### 2. Recriar o Banco (se necessário)

```bash
cd /opt/SingleOne
chmod +x recriar_banco_contabo.sh
./recriar_banco_contabo.sh
```

### 3. Verificar Manualmente via psql

```bash
# Conectar ao PostgreSQL
psql -h localhost -U postgres -d postgres

# Dentro do psql, verificar se o banco existe
\l

# Se o banco não existir, criar:
CREATE DATABASE singleone;

# Sair do psql
\q

# Executar script de inicialização
psql -h localhost -U postgres -d singleone -f init_db_atualizado.sql
```

### 4. Se a senha for diferente

Se a senha não for `postgres`, você pode:

**Opção A**: Definir variável de ambiente antes de executar:
```bash
export DB_PASSWORD="sua_senha_aqui"
./recriar_banco_contabo.sh
```

**Opção B**: Usar PGPASSWORD:
```bash
PGPASSWORD="sua_senha_aqui" psql -h localhost -U postgres -d postgres -c "CREATE DATABASE singleone;"
```

## 🔍 Verificar Qual Senha Está Sendo Usada

No servidor Contabo, verifique:

1. **Docker Compose** (se estiver usando):
```bash
cd /opt/SingleOne/SingleOne_Backend
cat docker-compose.yml | grep POSTGRES_PASSWORD
```

2. **Variáveis de ambiente do container**:
```bash
docker exec singleone-backend env | grep DB_PASSWORD
```

3. **Arquivo .env** (se existir):
```bash
cat /opt/SingleOne/.env | grep DB_PASSWORD
```

## 📝 Checklist de Recuperação

- [ ] Verificar se o PostgreSQL está rodando
- [ ] Identificar a senha correta do banco
- [ ] Verificar se o banco `singleone` existe
- [ ] Se não existir, criar o banco
- [ ] Executar `init_db_atualizado.sql`
- [ ] Verificar se todas as tabelas foram criadas (esperado: ~64)
- [ ] Verificar se todas as views foram criadas (esperado: ~32)
- [ ] Testar conexão do backend com o banco

## 🆘 Troubleshooting

### Erro: "database does not exist"
- O banco foi deletado ou nunca foi criado
- Execute `recriar_banco_contabo.sh`

### Erro: "password authentication failed"
- A senha está incorreta
- Verifique as variáveis de ambiente ou docker-compose.yml

### Erro: "connection refused"
- O PostgreSQL não está rodando
- Verifique: `docker ps | grep postgres` ou `systemctl status postgresql`

### Erro: "permission denied"
- Dê permissão de execução: `chmod +x *.sh`

## 📞 Informações para PGAdmin

Se estiver usando PGAdmin para conectar:

- **Host**: IP do servidor Contabo ou `localhost` se estiver no servidor
- **Port**: `5432`
- **Database**: `singleone` (ou `postgres` para conectar primeiro)
- **Username**: `postgres`
- **Password**: Verifique no servidor (provavelmente `postgres` ou `Admin@2025`)

