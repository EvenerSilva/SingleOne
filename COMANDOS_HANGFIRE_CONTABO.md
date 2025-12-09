# 🔧 COMANDOS PARA CRIAR TABELAS DO HANGFIRE NO CONTABO

## Problema
Os arquivos não foram baixados ainda devido a conflitos no git.

## Solução: Executar comandos diretos

### 1. Resolver conflitos do git primeiro:

```bash
cd /opt/SingleOne
git stash
git pull origin main
chmod +x verificar_e_recriar_forcado.sh
```

### 2. OU criar tabelas do Hangfire diretamente:

```bash
cd /opt/SingleOne

# Criar schema do Hangfire
docker exec singleone-postgres psql -U postgres -d singleone -c "CREATE SCHEMA IF NOT EXISTS hangfire;"

# Criar tabelas do Hangfire
docker exec singleone-postgres psql -U postgres -d singleone << 'EOF'
-- Tabela principal do servidor Hangfire
CREATE TABLE IF NOT EXISTS hangfire.server (
    id VARCHAR(100) PRIMARY KEY,
    data TEXT NOT NULL,
    lastheartbeat TIMESTAMP NOT NULL,
    heartbeatinterval INTEGER NOT NULL DEFAULT 15
);

-- Tabela de jobs
CREATE TABLE IF NOT EXISTS hangfire.job (
    id BIGSERIAL PRIMARY KEY,
    stateid BIGINT,
    statename VARCHAR(20),
    invocationdata TEXT NOT NULL,
    arguments TEXT NOT NULL,
    createdat TIMESTAMP NOT NULL,
    expireat TIMESTAMP
);

-- Tabela de estados dos jobs
CREATE TABLE IF NOT EXISTS hangfire.state (
    id BIGSERIAL PRIMARY KEY,
    jobid BIGINT NOT NULL,
    name VARCHAR(20) NOT NULL,
    reason TEXT,
    createdat TIMESTAMP NOT NULL,
    data TEXT,
    FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON DELETE CASCADE
);

-- Tabela de parâmetros dos jobs
CREATE TABLE IF NOT EXISTS hangfire.jobparameter (
    jobid BIGINT NOT NULL,
    name VARCHAR(40) NOT NULL,
    value TEXT,
    PRIMARY KEY (jobid, name),
    FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON DELETE CASCADE
);

-- Tabela de filas
CREATE TABLE IF NOT EXISTS hangfire.jobqueue (
    id BIGSERIAL PRIMARY KEY,
    jobid BIGINT NOT NULL,
    queue VARCHAR(50) NOT NULL,
    fetchedat TIMESTAMP,
    FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON DELETE CASCADE
);

-- Tabela de hash (para cache)
CREATE TABLE IF NOT EXISTS hangfire.hash (
    key VARCHAR(100) NOT NULL,
    field VARCHAR(100) NOT NULL,
    value TEXT,
    expireat TIMESTAMP,
    PRIMARY KEY (key, field)
);

-- Tabela de listas
CREATE TABLE IF NOT EXISTS hangfire.list (
    key VARCHAR(100) NOT NULL,
    value TEXT,
    expireat TIMESTAMP,
    PRIMARY KEY (key)
);

-- Tabela de sets
CREATE TABLE IF NOT EXISTS hangfire.set (
    key VARCHAR(100) NOT NULL,
    value VARCHAR(256) NOT NULL,
    score DOUBLE PRECISION,
    expireat TIMESTAMP,
    PRIMARY KEY (key, value)
);

-- Tabela de contadores
CREATE TABLE IF NOT EXISTS hangfire.counter (
    key VARCHAR(100) NOT NULL,
    value INTEGER NOT NULL DEFAULT 1,
    expireat TIMESTAMP,
    PRIMARY KEY (key)
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS ix_hangfire_job_stateid ON hangfire.job(stateid);
CREATE INDEX IF NOT EXISTS ix_hangfire_job_expireat ON hangfire.job(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_state_jobid ON hangfire.state(jobid);
CREATE INDEX IF NOT EXISTS ix_hangfire_jobqueue_queue_fetchedat ON hangfire.jobqueue(queue, fetchedat);
CREATE INDEX IF NOT EXISTS ix_hangfire_hash_expireat ON hangfire.hash(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_list_expireat ON hangfire.list(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_set_expireat ON hangfire.set(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_counter_expireat ON hangfire.counter(expireat);
EOF

echo "✅ Tabelas do Hangfire criadas!"
```

### 3. Verificar se foi criado:

```bash
docker exec singleone-postgres psql -U postgres -d singleone -c "\dt hangfire.*"
```

### 4. Corrigir tabela de Notas Fiscais (se necessário):

```bash
docker exec singleone-postgres psql -U postgres -d singleone << 'EOF'
-- Verificar se a tabela está em PascalCase
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'NotasFiscais') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'notasfiscais') THEN
            ALTER TABLE "NotasFiscais" RENAME TO notasfiscais;
            RAISE NOTICE 'Tabela NotasFiscais renomeada para notasfiscais';
        END IF;
    END IF;
END $$;
EOF
```

