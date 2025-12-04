# ⏰ Tempos para Funcionamento - DNS e Configurações

## 🚀 **Configuração Nginx (IMEDIATO)**
```bash
# Após executar estes comandos, funciona em segundos:
docker cp /tmp/nginx-final-sem-www.conf nginx-nginx-1:/etc/nginx/conf.d/dns.conf
docker exec nginx-nginx-1 nginx -s reload
```
**⏱️ Tempo**: **Imediato** (1-5 segundos)

---

## 🌐 **Propagação DNS (VARIÁVEL)**

### **Teste Imediato (sem DNS)**
Você pode testar AGORA mesmo antes do DNS propagar:

```bash
# Testar via header Host (simula DNS)
curl -H "Host: demo1.singleone.com.br" http://84.247.128.180:8080
curl -H "Host: api1.singleone.com.br" http://84.247.128.180:8080
curl -H "Host: portainer.singleone.com.br" http://84.247.128.180:8080
```

### **Propagação DNS Real**
| Cenário | Tempo Típico | Máximo |
|---------|-------------|---------|
| **Local/Região** | 5-15 minutos | 30 minutos |
| **Nacional** | 15-30 minutos | 1 hora |
| **Global** | 30 minutos - 2 horas | 24-48 horas |

### **Fatores que afetam**:
- **Provedor DNS**: Cloudflare (< 5 min), outros (15-60 min)
- **TTL Configurado**: Valores baixos propagam mais rápido
- **Cache local**: Limpar DNS cache do computador

---

## 🔧 **Como Verificar se Está Funcionando**

### **1. Teste via SSH (imediato)**
```bash
# Testar nginx local
curl -H "Host: demo1.singleone.com.br" http://localhost:8080

# Verificar configuração carregada
docker exec nginx-nginx-1 nginx -T | grep -A 2 "server_name"
```

### **2. Teste DNS local**
```bash
# No seu computador (Windows)
nslookup demo1.singleone.com.br
nslookup api1.singleone.com.br
nslookup portainer.singleone.com.br

# No servidor
dig demo1.singleone.com.br
dig api1.singleone.com.br
```

### **3. Teste de conectividade**
```bash
# Testar cada serviço diretamente
curl http://84.247.128.180:3000  # Frontend
curl http://84.247.128.180:5000  # Backend
curl http://84.247.128.180:9000  # Portainer
```

---

## ⚡ **Acelerar Propagação DNS**

### **Limpar Cache Local**:
```bash
# Windows
ipconfig /flushdns

# Linux/Mac
sudo systemctl restart systemd-resolved
# ou
sudo /etc/init.d/nscd restart
```

### **Usar DNS Público** (temporário):
- Google DNS: `8.8.8.8` e `8.8.4.4`
- Cloudflare: `1.1.1.1` e `1.0.0.1`

---

## 🎯 **Status Esperado por Tempo**

### **Imediato (0-5 min)**
- ✅ Nginx configurado
- ✅ Testes via header Host funcionando
- ✅ URLs diretas funcionando (`84.247.128.180:9000`)

### **15-30 minutos**
- ✅ DNS local propagado
- ✅ Acesso via domínios funcionando
- ✅ Browsers principais funcionando

### **1-2 horas**
- ✅ Propagação global completa
- ✅ Todos os dispositivos/redes

---

## 🚨 **Se Não Funcionar**

### **Verificar ordem**:
1. **Nginx configurado?**: `docker exec nginx-nginx-1 nginx -t`
2. **DNS propagado?**: `nslookup demo1.singleone.com.br`
3. **Serviços rodando?**: `docker ps`
4. **Portas acessíveis?**: `curl http://84.247.128.180:3000`

### **Debug rápido**:
```bash
# Ver logs
docker logs nginx-nginx-1 --tail 20
docker logs singleone-frontend --tail 10
docker logs singleone-backend --tail 10
```

---

## 📱 **Teste Móvel/Smartphone**

Para testar no celular (muitas vezes propaga antes):
- Usar dados móveis (não WiFi casa)
- DNS diferente pode propagar mais rápido
- Limpar cache do navegador móvel












