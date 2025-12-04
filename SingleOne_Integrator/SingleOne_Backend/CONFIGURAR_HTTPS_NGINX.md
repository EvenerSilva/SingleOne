# 🔒 Configuração HTTPS - SSL/TLS para Nginx

## 📋 **O que é necessário**

HTTPS requer **certificados SSL** no servidor nginx, não no DNS. Existem várias opções:

---

## 🚀 **Opção 1: Let's Encrypt (GRATUITO e RECOMENDADO)**

### **Instalação via Certbot**
```bash
# Atualizar sistema
apt update && apt upgrade -y

# Instalar certbot
apt install certbot -y

# Gerar certificados para todos os domínios
certbot certonly --standalone -d singleone.com.br -d demo1.singleone.com.br -d api1.singleone.com.br -d portainer.singleone.com.br

# Ou gerar um por vez
certbot certonly --standalone -d singleone.com.br
certbot certonly --standalone -d demo1.singleone.com.br
certbot certonly --standalone -d api1.singleone.com.br
certbot certonly --standalone -d portainer.singleone.com.br
```

### **Configuração Nginx para HTTPS**
```nginx
# HTTP -> HTTPS redirect
server {
    listen 80;
    server_name singleone.com.br demo1.singleone.com.br api1.singleone.com.br portainer.singleone.com.br;
    
    # Redirect all HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

# HTTPS Configuration
server {
    listen 443 ssl http2;
    server_name singleone.com.br;
    
    # SSL Certificates
    ssl_certificate /etc/letsencrypt/live/singleone.com.br/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/singleone.com.br/privkey.pem;
    
    # SSL Configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    
    location / {
        proxy_pass http://84.247.128.180:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;
    }
}

server {
    listen 443 ssl http2;
    server_name demo1.singleone.com.br;
    
    ssl_certificate /etc/letsencrypt/live/demo1.singleone.com.br/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/demo1.singleone.com.br/privkey.pem;
    
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    
    location / {
        proxy_pass http://84.247.128.180:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;
    }
}

server {
    listen 443 ssl http2;
    server_name api1.singleone.com.br;
    
    ssl_certificate /etc/letsencrypt/live/api1.singleone.com.br/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api1.singleone.com.br/privkey.pem;
    
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    
    location / {
        proxy_pass http://84.247.128.180:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;
    }
}

server {
    listen 443 ssl http2;
    server_name portainer.singleone.com.br;
    
    ssl_certificate /etc/letsencrypt/live/portainer.singleone.com.br/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/portainer.singleone.com.br/privkey.pem;
    
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    
    location / {
        proxy_pass http://84.247.128.180:9000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;
        
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_read_timeout 86400;
    }
}
```

---

## 🤖 **Opção 2: Certbot Automático com Nginx**

### **Instalação mais fácil:**
```bash
# Instalar certbot + nginx plugin
apt install certbot python3-certbot-nginx -y

# Gerar certificados automaticamente configurando nginx
certbot --nginx -d singleone.com.br -d demo1.singleone.com.br -d api1.singleone.com.br -d portainer.singleone.com.br
```

---

## 🔧 **Opção 3: Certificado Multi-Domain (Mais Simples)**

### **Gerar certificado para múltiplos domínios:**
```bash
certbot certonly --standalone \
  -d singleone.com.br \
  -d demo1.singleone.com.br \
  -d api1.singleone.com.br \
  -d portainer.singleone.com.br
```

### **Configuração simplificada:**
```nginx
# Redirect HTTP to HTTPS
server {
    listen 80;
    server_name singleone.com.br demo1.singleone.com.br api1.singleone.com.br portainer.singleone.com.br;
    return 301 https://$server_name$request_uri;
}

# HTTPS - usando mesmo certificado para todos
server {
    listen 443 ssl http2;
    server_name singleone.com.br demo1.singleone.com.br api1.singleone.com.br portainer.singleone.com.br;
    
    # Certificado multi-domain (gerado com todos os domínios)
    ssl_certificate /etc/letsencrypt/live/singleone.com.br/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/singleone.com.br/privkey.pem;
    
    # SSL config
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    
    # Configurações específicas por domínio
    if ($server_name = singleone.com.br) {
        set $target http://84.247.128.180:3000;
    }
    if ($server_name = demo1.singleone.com.br) {
        set $target http://84.247.128.180:3000;
    }
    if ($server_name = api1.singleone.com.br) {
        set $target http://84.247.128.180:5000;
    }
    if ($server_name = portainer.singleone.com.br) {
        set $target http://84.247.128.180:9000;
    }
    
    location / {
        proxy_pass $target;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;
        
        # Para portainer
        if ($server_name = portainer.singleone.com.br) {
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
        }
    }
}
```

---

## 🚀 **Comandos para Implementar**

### **1. Preparar nginx para certificados:**
```bash
# Parar nginx temporariamente para gerar certificados
docker exec nginx-nginx-1 nginx -s quit

# Ou usar modo standalone sem parar nginx
```

### **2. Gerar certificados:**
```bash
# Instalar certbot
apt install certbot -y

# Gerar certificado multi-domain
certbot certonly --standalone \
  -d singleone.com.br \
  -d demo1.singleone.com.br \
  -d api1.singleone.com.br \
  -d portainer.singleone.com.br \
  --email seuemail@exemplo.com \
  --agree-tos \
  --non-interactive
```

### **3. Configurar nginx HTTPS:**
```bash
# Criar configuração HTTPS
cat > /tmp/nginx-https.conf << 'EOF'
# [Configuração HTTPS aqui]
EOF

# Aplicar
docker cp /tmp/nginx-https.conf nginx-nginx-1:/etc/nginx/conf.d/https.conf
docker exec nginx-nginx-1 nginx -t && docker exec nginx-nginx-1 nginx -s reload
```

### **4. Renovação automática:**
```bash
# Configurar renovação automática
crontab -e
# Adicionar linha:
0 12 * * * /usr/bin/certbot renew --quiet && docker exec nginx-nginx-1 nginx -s reload
```

---

## 📋 **Pré-requisitos importantes**

1. **DNS já configurado** (você já tem ✅)
2. **Porta 80 e 443 liberadas** no firewall
3. **Domínios apontando** para `84.247.128.180`

**Qual opção prefere? Let's Encrypt é gratuito e automático!** 🎯












