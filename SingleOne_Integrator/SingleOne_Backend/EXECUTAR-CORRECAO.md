# 🚀 **EXECUTAR CORREÇÃO AGORA!**

## ⚡ **PASSO A PASSO RÁPIDO:**

### **1. Abrir seu Banco PostgreSQL**
- **pgAdmin** (recomendado)
- **DBeaver**
- **Azure Data Studio**
- **Qualquer cliente PostgreSQL**

### **2. Executar o SQL**
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

### **3. Verificar se Funcionou**
```sql
SELECT cliente, two_factor_enabled FROM parametros WHERE cliente = 2;
```

**Resultado esperado:**
```
cliente | two_factor_enabled
--------+-------------------
   2    |      false
```

### **4. Testar no Frontend**
- Tentar salvar alterações no usuário Evener
- O erro "Falha de comunicação" deve desaparecer

---

## 📁 **Arquivos Disponíveis:**
- `corrigir-cliente2-simples.sql` - SQL simples
- `resolver-parametros-cliente2.sql` - SQL completo
- `SOLUCAO-FALHA-COMUNICACAO.md` - Documentação completa

---

## ⏰ **TEMPO ESTIMADO: 2-3 minutos**

**Execute agora e teste! O problema será resolvido imediatamente!** 🎯✨
