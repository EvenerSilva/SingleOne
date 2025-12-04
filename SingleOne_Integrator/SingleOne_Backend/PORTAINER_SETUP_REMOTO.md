# 🐳 Configuração do Portainer para Servidor Remoto (84.247.128.180)

## 📋 Situação Atual
- Servidor Linux: 84.247.128.180
- DNS configurado apontando para este IP
- Docker já rodando no servidor
- Necessário: Portainer para gerenciar containers das publicações/demos

---

## 🚀 Instalação do Portainer no Servidor

### Opção 1: Instalação Rápida via SSH

Conecte-se ao servidor:
```bash
ssh root@84.247.128.180
```

Execute os seguintes comandos para instalar o Portainer:

```bash
# 1. Criar volume para persistir dados do Portainer
docker volume create portainer_data

# 2. Executar o Portainer com acesso remoto
docker run -d \
  -p 8000:8000 \
  -p 9000:9000 \
  --name=portainer \
  --restart=unless-stopped \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -v portainer_data:/data \
  portainer/portainer-ce:latest

# 3. Verificar se está rodando
docker ps | grep portainer
```

### Opção 2: Instalação via Docker Compose (Recomendado)

Crie um arquivo `docker-compose.portainer.yml` no servidor:

```bash
cd /opt
nano docker-compose.portainer.yml
```

Conteúdo do arquivo:
```yaml
version: '3.8'

services:
  portainer:
    image: portainer/portainer-ce:latest
    container_name: portainer
    restart: unless-stopped
    ports:
      - "9000:9000"
      - "8000:8000"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - portainer_data:/data
    networks:
      - portainer-network

volumes:
  portainer_data:

networks:
  portainer-network:
    driver: bridge
```

Executar:
```bash
docker-compose -f docker-compose.portainer.yml up -d
```

---

## 🌐 Configuração e Acesso

### 1. Acesse o Portainer
Abra o navegador e acesse:
```
http://84.247.128.180:9000
```

### 2. Configuração Inicial
1. **Primeiro acesso**: Crie um usuário administrador
2. **Environment**: Selecione "Docker" e clique em "Connect"

### 3. Configuração do Endpoint
Se necessário configurar endpoints adicionais:
1. Vá em **Endpoints** > **Add endpoint**
2. Configure:
   - **Name**: `Servidor Principal`
   - **Endpoint URL**: `unix:///var/run/docker.sock`
   - **Public IP**: `84.247.128.180`

---

## 📦 Deploy de Containers via Portainer

### Método 1: Via Stacks (Recomendado para aplicações completas)

1. **Acesse**: Stacks > Add stack
2. **Nome**: `singleone-demo` (ou nome da sua aplicação)
3. **Upload**: Selecione seu `docker-compose.yml`
4. **Environment variables**: Configure se necessário
5. **Deploy**: Clique em "Deploy the stack"

### Método 2: Via Containers (Individual)

1. **Acesse**: Containers > Add container
2. **Configure**:
   - **Name**: `nome-do-container`
   - **Image**: `imagem-necessaria`
   - **Port mapping**: Ex: `5000:5000`
   - **Environment**: Variáveis necessárias
3. **Deploy**: Clique em "Deploy the container"

---

## 🔧 Configuração para Gestão de DNS/Endereços

### 1. Configurar Reverse Proxy (Opcional mas Recomendado)

Para gerenciar múltiplas aplicações no mesmo servidor, configure um reverse proxy:

#### Nginx Reverse Proxy
```bash
# Instalar Nginx
sudo apt update && sudo apt install nginx -y

# Configurar proxy para Portainer
sudo nano /etc/nginx/sites-available/portainer
```

Configuração básica:
```nginx
server {
    listen 80;
    server_name portainer.seudominio.com;  # Seu DNS

    location / {
        proxy_pass http://localhost:9000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### 2. Configurar Múltiplos Domínios

Para gerir diferentes aplicações com DNS:

```bash
# Exemplo para múltiplas aplicações
sudo nano /etc/nginx/sites-available/apps
```

```nginx
# Aplicação Principal
server {
    listen 80;
    server_name app1.seudominio.com;
    
    location / {
        proxy_pass http://localhost:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}

# Demo/Público
server {
    listen 80;
    server_name demo.seudominio.com;
    
    location / {
        proxy_pass http://localhost:3001;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}

# Portainer Admin
server {
    listen 80;
    server_name admin.seudominio.com;
    
    location / {
        proxy_pass http://localhost:9000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

---

## 🎯 Deploy do SingleOne via Portainer

### 1. Preparar arquivos no servidor

```bash
# Criar diretório do projeto
mkdir -p /opt/singleone
cd /opt/singleone

# Upload dos arquivos (via SCP ou clone)
scp -r SingleOne_Backend root@84.247.128.180:/opt/singleone/
scp -r SingleOne_Frontend root@84.247.128.180:/opt/singleone/
```

### 2. Deploy via Portainer Web Interface

1. **Acesse**: http://84.247.128.180:9000
2. **Vá em**: Stacks > Add stack
3. **Nome**: `singleone-production`
4. **Upload**: Faça upload do `docker-compose.yml` do backend
5. **Configure Environment**:
   ```
   API_URL=http://84.247.128.180:5000/api/
   ```
6. **Deploy**: Clique em "Deploy the stack"

### 3. Verificar Deploy

```bash
# Ver containers rodando
docker ps

# Ver logs
docker logs singleone-backend
docker logs singleone-frontend
```

---

## 🔒 Segurança e Firewall

### 1. Configurar Firewall
```bash
# Ubuntu/Debian
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 9000/tcp  # Portainer
sudo ufw allow 5000/tcp  # Backend API
sudo ufw allow 3000/tcp  # Frontend
sudo ufw enable

# Verificar status
sudo ufw status
```

### 2. Segurança do Portainer
```bash
# Criar usuário específico para Portainer (opcional)
sudo useradd -m -s /bin/bash portainer-user
sudo usermod -aG docker portainer-user
```

---

## 📊 Monitoramento e Gestão

### 1. Métricas via Portainer
- **Dashboard**: Visualize uso de recursos
- **Containers**: Gerencie estado dos containers
- **Images**: Gerencie imagens Docker
- **Volumes**: Gerencie volumes e dados persistentes
- **Networks**: Configure redes entre containers

### 2. Comandos Úteis via SSH
```bash
# Status geral
docker ps -a
docker stats

# Logs em tempo real
docker logs -f singleone-backend
docker logs -f singleone-frontend

# Reiniciar serviços
docker restart singleone-backend
docker restart singleone-frontend

# Backup volumes
docker run --rm -v portainer_data:/data -v $(pwd):/backup alpine tar czf /backup/portainer-backup.tar.gz -C /data .
```

---

## 🌐 URLs Finais de Acesso

Após configuração completa:
- **Portainer**: http://84.247.128.180:9000
- **Frontend**: http://84.247.128.180:3000 (ou seu DNS)
- **Backend API**: http://84.247.128.180:5000 (ou seu DNS)
- **Swagger/Docs**: http://84.247.128.180:5000/swagger

---

## 🚨 Troubleshooting

### Portainer não acessível
```bash
# Verificar se está rodando
docker ps | grep portainer

# Ver logs
docker logs portainer

# Reiniciar
docker restart portainer
```

### Containers não iniciam
```bash
# Ver logs detalhados
docker logs nome-do-container

# Verificar recursos
docker stats

# Verificar espaço em disco
df -h
```

### Problemas de rede
```bash
# Verificar portas em uso
netstat -tulpn | grep :9000

# Testar conectividade
curl -I http://localhost:9000
```

---

**✅ Com essa configuração, você terá controle total sobre seus containers via Portainer, permitindo gerenciar todas as publicações e demos da solução de forma centralizada!**












