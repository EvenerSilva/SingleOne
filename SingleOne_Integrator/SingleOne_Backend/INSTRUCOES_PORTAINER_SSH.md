# 🐳 Configuração do Portainer via SSH e WinSCP

## 📋 O que você precisa
- ✅ Acesso SSH ao servidor 84.247.128.180
- ✅ WinSCP configurado
- ✅ Docker já rodando no servidor

---

## 🚀 Passo a Passo Completo

### 1. Conectar via SSH

Abra seu terminal SSH (PuTTY, WinSCP terminal, ou outro) e conecte:
```bash
ssh root@84.247.128.180
```

### 2. Verificar se Docker está rodando
```bash
# Verificar status do Docker
sudo systemctl status docker

# Se não estiver rodando, iniciar:
sudo systemctl start docker
sudo systemctl enable docker

# Verificar versão
docker --version
docker ps
```

### 3. Instalar o Portainer

Execute estes comandos no SSH:

```bash
# Criar diretório para configurações
mkdir -p /opt/portainer
cd /opt/portainer

# Criar volume para persistir dados do Portainer
docker volume create portainer_data

# Executar o Portainer
docker run -d \
  -p 9000:9000 \
  -p 8000:8000 \
  --name portainer \
  --restart unless-stopped \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -v portainer_data:/data \
  portainer/portainer-ce:latest

# Verificar se está rodando
docker ps | grep portainer
```

### 4. Verificar se funcionou

```bash
# Ver logs do Portainer
docker logs portainer

# Verificar se a porta está aberta
netstat -tulpn | grep 9000
# ou
ss -tulpn | grep 9000
```

### 5. Configurar Firewall (se necessário)

```bash
# Ubuntu/Debian com UFW
sudo ufw allow 9000/tcp
sudo ufw allow 8000/tcp

# CentOS/RHEL com firewalld
sudo firewall-cmd --add-port=9000/tcp --permanent
sudo firewall-cmd --add-port=8000/tcp --permanent
sudo firewall-cmd --reload

# Verificar status
sudo ufw status
# ou
sudo firewall-cmd --list-all
```

---

## 📁 Preparar Arquivos via WinSCP

### 1. Conectar no WinSCP
- **Host**: `84.247.128.180`
- **Usuário**: `root` (ou seu usuário)
- **Senha**: sua senha

### 2. Criar estrutura de diretórios
```bash
# Via SSH ou no terminal do WinSCP
mkdir -p /opt/singleone/backend
mkdir -p /opt/singleone/frontend
```

### 3. Upload dos arquivos
Via WinSCP, faça upload de:
```
Local: C:\SingleOne\SingleOne_Backend\*
Remote: /opt/singleone/backend/

Local: C:\SingleOne\SingleOne_Frontend\*
Remote: /opt/singleone/frontend/
```

---

## 🌐 Acessar o Portainer

### 1. Acesse no navegador
```
http://84.247.128.180:9000
```

### 2. Primeira configuração
1. **Criar usuário admin**: Defina username e senha
2. **Selecionar ambiente**: Escolha "Docker"
3. **Conectar**: Clique em "Connect"

---

## 📦 Deploy da Aplicação via Portainer

### Opção 1: Via Stacks (Recomendado)

1. **No Portainer web**:
   - Vá em **Stacks** > **Add stack**
   - Nome: `singleone-production`

2. **Upload do docker-compose.yml**:
   - Copie o conteúdo do `docker-compose.yml` do backend
   - Cole no campo "Web editor"

3. **Configurar Environment** (se necessário):
   ```
   API_URL=http://84.247.128.180:5000/api/
   ```

4. **Deploy**: Clique em "Deploy the stack"

### Opção 2: Via SSH (alternativo)

Se preferir via SSH:
```bash
cd /opt/singleone/backend
docker-compose down  # parar se já estiver rodando
docker-compose up -d --build
```

---

## 🔧 Configurar Múltiplos Domínios/Endereços

Para gerir diferentes aplicações com DNS específicos:

### 1. Criar arquivo de configuração nginx
```bash
sudo nano /etc/nginx/sites-available/singleone
```

### 2. Configuração básica
```nginx
# Servidor principal
server {
    listen 80;
    server_name seu-dominio.com www.seu-dominio.com;
    
    location / {
        proxy_pass http://localhost:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}

# Demo/Público
server {
    listen 80;
    server_name demo.seu-dominio.com;
    
    location / {
        proxy_pass http://localhost:3001;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}

# Portainer Admin
server {
    listen 80;
    server_name admin.seu-dominio.com;
    
    location / {
        proxy_pass http://localhost:9000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### 3. Ativar configuração
```bash
sudo ln -s /etc/nginx/sites-available/singleone /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## 🎯 Gestão de Containers via Portainer

### Containers Disponíveis
No Portainer você poderá gerenciar:

1. **Stacks**: Aplicações completas (frontend + backend + banco)
2. **Containers**: Containers individuais
3. **Images**: Imagens Docker
4. **Volumes**: Dados persistentes
5. **Networks**: Redes entre containers

### Ações Principais
- ▶️ **Start/Stop**: Ligar/desligar containers
- 🔄 **Restart**: Reiniciar containers
- 📊 **Stats**: Monitorar recursos (CPU, RAM, rede)
- 📋 **Logs**: Ver logs em tempo real
- ⚙️ **Settings**: Modificar configurações

---

## 📊 Comandos Úteis via SSH

```bash
# Status geral
docker ps -a
docker stats

# Logs dos containers
docker logs singleone-backend -f
docker logs singleone-frontend -f
docker logs portainer -f

# Reiniciar serviços específicos
docker restart singleone-backend
docker restart singleone-frontend

# Ver uso de recursos
docker system df
docker system prune  # limpeza (cuidado!)

# Backup do Portainer
docker run --rm -v portainer_data:/data -v $(pwd):/backup alpine tar czf /backup/portainer-backup.tar.gz -C /data .
```

---

## 🚨 Troubleshooting

### Portainer não inicia
```bash
# Verificar se há conflito de porta
netstat -tulpn | grep 9000

# Ver logs de erro
docker logs portainer

# Verificar se há container parado
docker ps -a | grep portainer
docker rm portainer  # remover se necessário
```

### Aplicação não funciona
```bash
# Verificar se todos os containers estão rodando
docker ps

# Ver logs de erro
docker-compose logs

# Verificar conectividade entre containers
docker exec -it singleone-backend ping postgres
```

### Problemas de rede/firewall
```bash
# Testar conectividade local
curl -I http://localhost:9000

# Testar conectividade externa (de outro terminal)
curl -I http://84.247.128.180:9000

# Verificar firewall
sudo ufw status numbered
```

---

## ✅ Checklist Final

- [ ] Docker rodando no servidor
- [ ] Portainer instalado e acessível em :9000
- [ ] Firewall configurado (portas 9000, 8000 liberadas)
- [ ] Arquivos da aplicação enviados via WinSCP
- [ ] Stack/Serviços deployados via Portainer
- [ ] Aplicação acessível via browser
- [ ] DNS configurado (se aplicável)

---

**🎉 Com isso configurado, você terá controle total sobre seus containers via Portainer, podendo gerenciar todas as publicações e demos da solução de forma centralizada e visual!**












