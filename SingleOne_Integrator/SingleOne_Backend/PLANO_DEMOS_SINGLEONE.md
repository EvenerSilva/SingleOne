# 🎯 Plano de Demonstrações - SingleOne.com.br

## 📋 Estrutura de URLs Configurada

### 🌐 **URLs Principais**
- **Site Principal**: `singleone.com.br` → Frontend Demo 1 (porta 3000)
- **Portainer Admin**: `portainer.singleone.com.br` → Gestão Docker (porta 9000)

### 🎮 **Demo 1** (Atual)
- **Frontend**: `demo1.singleone.com.br` → Porta 3000
- **API**: `api1.singleone.com.br` → Porta 5000

### 🚀 **Demos Futuras** (Estrutura preparada)
- **Demo 2**: `demo2.singleone.com.br` → Porta 3002
- **API 2**: `api2.singleone.com.br` → Porta 5002

- **Demo 3**: `demo3.singleone.com.br` → Porta 3003
- **API 3**: `api3.singleone.com.br` → Porta 5003

## 📊 **Configuração DNS Necessária**

No provedor de DNS (registro.br, Cloudflare, etc.):

```
Tipo    Nome                            Valor
A       @                               84.247.128.180
A       www                             84.247.128.180
A       demo1                           84.247.128.180
A       api1                            84.247.128.180
A       portainer                       84.247.128.180

# Para futuras demos:
A       demo2                           84.247.128.180
A       api2                            84.247.128.180
A       demo3                           84.247.128.180
A       api3                            84.247.128.180
```

## 🎯 **Gestão via Portainer**

### Containers por Demo:
- **Demo 1**: `singleone-frontend`, `singleone-backend`
- **Demo 2**: `singleone-demo2-frontend`, `singleone-demo2-backend`
- **Demo 3**: `singleone-demo3-frontend`, `singleone-demo3-backend`

### Portas Sugeridas:
```
Demo 1: Frontend 3000, Backend 5000
Demo 2: Frontend 3002, Backend 5002  
Demo 3: Frontend 3003, Backend 5003
```

## 🔧 **Comandos para Deploy de Novas Demos**

### Para adicionar Demo 2:
```bash
# 1. Criar containers com portas específicas
# 2. Adicionar configuração nginx:
cat >> /tmp/nginx-demo2.conf << 'EOF'
server {
    listen 80;
    server_name demo2.singleone.com.br;
    
    location / {
        proxy_pass http://84.247.128.180:3002;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 80;
    server_name api2.singleone.com.br;
    
    location / {
        proxy_pass http://84.247.128.180:5002;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
EOF

# 3. Aplicar
docker cp /tmp/nginx-demo2.conf nginx-nginx-1:/etc/nginx/conf.d/
docker exec nginx-nginx-1 nginx -s reload
```

## 📱 **URLs Finais**

Após configuração DNS:
- ✅ `singleone.com.br` → Demo principal
- ✅ `demo1.singleone.com.br` → Demo 1 Frontend
- ✅ `api1.singleone.com.br` → Demo 1 API
- ✅ `portainer.singleone.com.br` → Gestão Docker

## 🎉 **Vantagens desta Estrutura**

1. **Organização Clara**: Cada demo tem seu espaço
2. **Escalabilidade**: Fácil adicionar novas demos
3. **Gestão Centralizada**: Portainer dedicado
4. **SEO Friendly**: URLs limpas e organizadas
5. **Isolamento**: Cada demo pode ter configurações diferentes












