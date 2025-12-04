# 🔐 2FA Inteligente - SingleOne Backend

## 🎯 **Visão Geral**

Implementamos uma **lógica inteligente de 2FA (Duplo Fator)** que garante que as configurações individuais de usuários só sejam permitidas quando o 2FA estiver habilitado globalmente para o cliente.

## 🏗️ **Arquitetura da Solução**

### **1. Configuração Global (Parâmetros)**
- **Master Switch**: `two_factor_enabled` na tabela `parametros`
- **Controle Centralizado**: Apenas administradores podem habilitar/desabilitar globalmente
- **Escopo por Cliente**: Cada cliente pode ter sua própria configuração

### **2. Configuração Individual (Usuários)**
- **Campo Individual**: `two_factor_enabled` na tabela `usuarios`
- **Validação Inteligente**: Só pode ser alterado se global estiver habilitado
- **Controle de Usuário**: Cada usuário pode escolher usar ou não (quando disponível)

## 🚀 **Endpoints Disponíveis**

### **Verificar Status Global de 2FA**
```http
GET /api/Usuario/GetGlobalTwoFactorStatus/{clienteId}
```

**Resposta:**
```json
{
  "clienteId": 1,
  "twoFactorEnabledGlobally": true,
  "message": "2FA está habilitado globalmente para este cliente",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### **Verificar Status Individual de 2FA**
```http
GET /api/Usuario/GetTwoFactorStatus/{usuarioId}
```

**Resposta:**
```json
{
  "success": true,
  "userId": 1,
  "userName": "Adminstrador",
  "clienteId": 1,
  "twoFactorEnabledGlobally": true,
  "twoFactorEnabledIndividually": false,
  "canEnableTwoFactor": true,
  "message": "2FA está disponível para este usuário"
}
```

## 🛡️ **Validações Implementadas**

### **1. Validação no Backend (UsuarioNegocio.Salvar)**
```csharp
// VALIDAÇÃO INTELIGENTE DE 2FA
// Só permitir alterar configurações de 2FA se estiver habilitado globalmente
if (usr.TwoFactorEnabled.HasValue || !String.IsNullOrEmpty(usr.TwoFactorSecret) || 
    !String.IsNullOrEmpty(usr.TwoFactorBackupCodes))
{
    // Buscar configuração global de 2FA para o cliente
    var configuracaoGlobal = _parametroRepository.Buscar(x => x.Cliente == usr.Cliente).FirstOrDefault();
    
    if (configuracaoGlobal?.TwoFactorEnabled != true)
    {
        return JsonConvert.SerializeObject(new { 
            Mensagem = "2FA não está habilitado globalmente para este cliente. Ative primeiro nas configurações.", 
            Status = "400" 
        });
    }
}
```

### **2. Métodos de Verificação**
- **`IsTwoFactorEnabledGlobally(int clienteId)`**: Verifica status global
- **`GetUserTwoFactorStatus(int usuarioId)`**: Obtém status completo do usuário

## 🔄 **Fluxo de Funcionamento**

### **Cenário 1: 2FA Global DESABILITADO**
```
1. Usuário tenta ativar 2FA individual
2. Sistema verifica configuração global
3. Retorna erro: "2FA não está habilitado globalmente"
4. Campo 2FA fica desabilitado na interface
```

### **Cenário 2: 2FA Global HABILITADO**
```
1. Usuário pode ativar/desativar 2FA individual
2. Sistema permite alterações
3. Campo 2FA fica habilitado na interface
4. Usuário escolhe usar ou não 2FA
```

## 🎨 **Implementação no Frontend**

### **1. Verificar Status Global**
```typescript
// Antes de mostrar opções de 2FA
const globalStatus = await this.usuarioApi.getGlobalTwoFactorStatus(clienteId);
this.twoFactorAvailable = globalStatus.twoFactorEnabledGlobally;
```

### **2. Controlar Interface**
```typescript
// Campo 2FA só aparece quando habilitado globalmente
showTwoFactorField: boolean = this.config.twoFactorEnabled;

// Campo 2FA fica desabilitado quando 2FA global está OFF
twoFactorDisabled: boolean = !this.config.twoFactorEnabled;
```

### **3. Validação de Formulário**
```typescript
// Só permitir envio se 2FA global estiver habilitado
if (this.form.value.twoFactorEnabled && !this.twoFactorAvailable) {
  this.showError('2FA não está disponível globalmente');
  return;
}
```

## 📊 **Estrutura do Banco de Dados**

### **Tabela: parametros**
```sql
-- Configurações globais de 2FA
two_factor_enabled BOOLEAN DEFAULT false
two_factor_type VARCHAR(20) DEFAULT 'email'
two_factor_expiration_minutes INTEGER DEFAULT 5
two_factor_max_attempts INTEGER DEFAULT 3
two_factor_lockout_minutes INTEGER DEFAULT 15
two_factor_email_template TEXT
```

### **Tabela: usuarios**
```sql
-- Configurações individuais de 2FA
two_factor_enabled BOOLEAN DEFAULT false
two_factor_secret VARCHAR(255)
two_factor_backup_codes TEXT
two_factor_last_used TIMESTAMP
```

## 🧪 **Testes Recomendados**

### **1. Teste de Validação Global**
```bash
# 1. Desabilitar 2FA globalmente
# 2. Tentar ativar 2FA para um usuário
# 3. Verificar se retorna erro 400
```

### **2. Teste de Funcionamento Normal**
```bash
# 1. Habilitar 2FA globalmente
# 2. Ativar 2FA para um usuário
# 3. Verificar se funciona normalmente
```

### **3. Teste de Status**
```bash
# 1. Verificar status global: GET /api/Usuario/GetGlobalTwoFactorStatus/1
# 2. Verificar status individual: GET /api/Usuario/GetTwoFactorStatus/1
# 3. Verificar se as respostas estão corretas
```

## 🔧 **Manutenção**

### **Logs Disponíveis**
- **Backend**: Console.WriteLine com prefixo `[2FA]`
- **Controller**: Logs detalhados de cada operação
- **Validações**: Mensagens de erro claras para o usuário

### **Monitoramento**
- **Status Global**: Verificar tabela `parametros`
- **Status Individual**: Verificar tabela `usuarios`
- **Logs de Erro**: Console da aplicação

## 🎉 **Benefícios da Implementação**

1. **✅ Consistência**: Sistema sempre coerente
2. **✅ UX Clara**: Usuário entende quando pode usar 2FA
3. **✅ Segurança**: Evita configurações inválidas
4. **✅ Manutenibilidade**: Controle centralizado
5. **✅ Flexibilidade**: Cada usuário escolhe usar ou não
6. **✅ Validação Inteligente**: Backend previne inconsistências

## 🚀 **Próximos Passos**

1. **Testar a implementação** no ambiente de desenvolvimento
2. **Implementar no Frontend** a lógica de controle de interface
3. **Documentar para usuários finais** como usar a funcionalidade
4. **Monitorar logs** para identificar possíveis melhorias
5. **Considerar implementar** notificações quando 2FA global for alterado

---

**Desenvolvido por**: SingleOne Team  
**Data**: Janeiro 2024  
**Versão**: 1.0.0
