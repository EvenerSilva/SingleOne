# 🌐 Configuração DNS com Nginx Reverse Proxy

## 📋 Situação Atual
- ✅ Portainer rodando em: http://84.247.128.180:9000/
- ✅ nginx-nginx-1 container rodando
- 🎯 Objetivo: Configurar DNS para direcionar aplicações via nginx

---

## 🔧 Configuração do Nginx como Reverse Proxy

### 1. Acessar o Container Nginx via Portainer

1. **Acesse**: http://84.247.128.180:9000/
2. **Vá em**: Containers > nginx-nginx-1
3. **Clique em**: Console (para acessar shell do container)

### 2. Ou via SSH no servidor

```bash
# Conectar no container nginx
docker exec -it nginx-nginx-1 /bin/bash
# ou
docker exec -it nginx-nginx-1 /bin/sh
```

### 3. Configurar Nginx Virtual Hosts

#### Estrutura de configuração recomendada:

```bash
# Dentro do container nginx, editar configurações
cd /etc/nginx/conf.d/
# ou
cd /etc/nginx/sites-available/
```

#### Criar arquivo de configuração principal:

```nginx
# Arquivo: /etc/nginx/conf.d/default.conf
# ou /etc/nginx/sites-available/default

# Servidor Principal (Aplicação SingleOne)
server {
    listen 80;
    server_name seu-dominio-principal.com www.seu-dominio-principal.com;
    
    # Logs
    access_log /var/log/nginx/app.log;
    error_log /var/log/nginx/error.log;
    
    location / {
        proxy_pass http://singleone-frontend:80;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Configurações extras para SPA
        try_files $uri $uri/ /index.html;
    }
    
    # API Backend
    location /api/ {
        proxy_pass http://singleone-backend:5000/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# Demo/Público
server {
    listen 80;
    server_name demo.seu-dominio.com;
    
    location / {
        proxy_pass http://singleone-frontend:80;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}

# Portainer Admin
server {
    listen 80;
    server_name admin.seu-dominio.com;
    
    location / {
        proxy_pass http://84.247.128.180:9000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Configurações específicas do Portainer
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }
}

# Servidor padrão (fallback)
server {
    listen 80 default_server;
    server_name _;
    
    location / {
        return 301 http://seu-dominio-principal.com$request_uri;
    }
}
```

---

## 🚀 Configuração Prática via Portainer

### Opção 1: Editar via Portainer Console

1. **No Portainer**:
   - Containers > nginx-nginx-1 > Console
   - Execute: `nginx -t` (testar configuração)

2. **Editar arquivo de configuração**:
   ```bash
   # Localizar arquivo de configuração
   find /etc/nginx -name "*.conf" | head -5
   
   # Editar configuração principal
   nano /etc/nginx/conf.d/default.conf
   # ou
   nano /etc/nginx/nginx.conf
   ```

### Opção 2: Via Volume Mount (Recomendado)

#### 1. Criar arquivo de configuração local

Crie um arquivo `nginx.conf` no servidor:

```bash
# No servidor SSH
mkdir -p /opt/nginx-config
nano /opt/nginx-config/default.conf
```

#### 2. Configurar volume no docker-compose ou via Portainer

No Portainer, edite o container `nginx-nginx-1`:
- **Volumes** > **Bind**
- **Container path**: `/etc/nginx/conf.d/`
- **Host path**: `/opt/nginx-config/`

---

## 🌐 Configuração de DNS

### 1. Configurar DNS Provider

No seu provedor de DNS (registro.br, Cloudflare, etc.), configure:

```
Tipo    Nome                    Valor
A       @                      84.247.128.180
A       www                    84.247.128.180
A       admin                  84.247.128.180
A       demo                   84.247.128.180
A       api                    84.247.128.180
```

### 2. Exemplo de configuração DNS completa

```
# Zona DNS para seu-dominio.com
seu-dominio.com.          A     84.247.128.180
www.seu-dominio.com.      A     84.247.128.180
admin.seu-dominio.com.    A     84.247.128.180
demo.seu-dominio.com.     A     84.247.128.180
api.seu-dominio.com.      A     84.247.128.180
```

---

## 🔧 Configuração Específica para SingleOne

### Arquivo de configuração otimizado:

```nginx
# /etc/nginx/conf.d/singleone.conf

# Frontend Principal
server {
    listen 80;
    server_name app.seu-dominio.com www.seu-dominio.com;
    
    location / {
        proxy_pass http://singleone-frontend:80;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Para Angular SPA
        try_files $uri $uri/ /index.html;
    }
    
    # Assets estáticos
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
        proxy_pass http://singleone-backend:5000;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}

# API Backend
server {
    listen 80;
    server_name api.seu-dominio.com;
    
    location / {
        proxy_pass http://singleone-backend:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # CORS headers se necessário
        add_header Access-Control-Allow-Origin *;
        add_header Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS";
        add_header Access-Control-Allow-Headers "Authorization, Content-Type";
    }
}

# Demo/Público
server {
    listen 80;
    server_name demo.seu-dominio.com;
    
    location / {
        proxy_pass http://singleone-frontend:80;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}

# Portainer Admin
server {
    listen 80;
    server_name admin.seu-dominio.com;
    
    location / {
        proxy_pass http://host.docker.internal:9000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # WebSocket support para Portainer
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_read_timeout 86400;
    }
}
```

---

## 🎯 Passos Práticos

### 1. Identificar nomes dos containers

Via Portainer ou SSH:
```bash
docker ps --format "table {{.Names}}\t{{.Ports}}"
```

### 2. Testar conectividade entre containers

```bash
# Dentro do nginx container
docker exec -it nginx-nginx-1 ping singleone-backend
docker exec -it nginx-nginx-1 ping singleone-frontend
```

### 3. Aplicar configuração

```bash
# Dentro do nginx container
nginx -t                    # Testar configuração
nginx -s reload            # Recarregar configuração
# ou
service nginx reload
```

---

## 🚨 Troubleshooting

### Verificar se nginx está funcionando:
```bash
# Status do nginx
docker exec -it nginx-nginx-1 nginx -t
docker exec -it nginx-nginx-1 nginx -s reload

# Ver logs
docker logs nginx-nginx-1

# Testar conectividade
curl -H "Host: admin.seu-dominio.com" http://84.247.128.180
```

### Verificar DNS:
```bash
# Testar DNS resolution
nslookup admin.seu-dominio.com
dig admin.seu-dominio.com
```

---

## 📋 Checklist Final

- [ ] nginx-nginx-1 container rodando
- [ ] Configuração nginx editada com virtual hosts
- [ ] DNS configurado apontando para 84.247.128.180
- [ ] Containers singleone-backend e singleone-frontend rodando
- [ ] Conectividade entre containers testada
- [ ] Nginx reload executado
- [ ] Acesso via DNS testado

**Qual o seu domínio principal que queremos configurar primeiro?**












