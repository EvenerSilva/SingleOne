# 🚨 **EXECUTAR CORREÇÃO URGENTE!**

## ⚡ **O PROBLEMA PERSISTE - EXECUTE AGORA:**

### **1. ABRIR SEU BANCO POSTGRESQL**
- **pgAdmin** (recomendado)
- **DBeaver**
- **Azure Data Studio**
- **Qualquer cliente PostgreSQL**

### **2. EXECUTAR ESTE COMANDO EXATO:**
```sql
INSERT INTO parametros (
    cliente,
    two_factor_enabled,
    two_factor_type,
    two_factor_expiration_minutes,
    two_factor_max_attempts,
    two_factor_lockout_minutes,
    two_factor_email_template
) VALUES (
    2,                           -- Cliente 2
    false,                       -- 2FA desabilitado por padrão
    'email',                     -- Tipo de 2FA
    5,                           -- Expiração em minutos
    3,                           -- Máximo de tentativas
    15,                          -- Bloqueio em minutos
    'Código de verificação: {code}' -- Template de email
) ON CONFLICT (cliente) DO NOTHING;
```

### **3. VERIFICAR SE FUNCIONOU:**
```sql
SELECT cliente, two_factor_enabled FROM parametros WHERE cliente = 2;
```

**Resultado esperado:**
```
cliente | two_factor_enabled
--------+-------------------
   2    |      false
```

### **4. TESTAR NO FRONTEND**
- Tentar salvar alterações no usuário Evener
- O erro "Falha de comunicação" deve desaparecer

---

## 📁 **Arquivos Disponíveis:**
- `COMANDO-EXATO.sql` - Comando SQL pronto para copiar
- `corrigir-cliente2-simples.sql` - SQL simples
- `resolver-parametros-cliente2.sql` - SQL completo

---

## ⏰ **TEMPO ESTIMADO: 2-3 minutos**

**O problema só será resolvido quando você executar este SQL no banco!** 🎯

**Execute agora e teste! O erro deve desaparecer imediatamente!** 🚀✨
