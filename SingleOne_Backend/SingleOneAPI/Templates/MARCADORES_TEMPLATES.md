# 📝 Marcadores Disponíveis para Templates de E-mail

Este documento lista todos os marcadores (variáveis) disponíveis para uso nos templates de e-mail do sistema SingleOne.

---

## 👤 Informações do Colaborador

| Marcador | Descrição | Exemplo |
|----------|-----------|---------|
| `@nomeColaborador` | Nome completo do colaborador | João Silva Santos |
| `@cpf` | CPF do colaborador (descriptografado) | 123.456.789-00 |
| `@matricula` | Matrícula do colaborador | MAT001 |
| `@cargo` | Cargo do colaborador | Analista de TI |
| `@empresa` | Nome da empresa do colaborador | TechCorp Ltda |

---

## 🏢 Informações da Empresa/Cliente

| Marcador | Descrição | Exemplo |
|----------|-----------|---------|
| `@nomeEmpresa` | Nome da empresa principal do cliente | Empresa S/A |
| `@urlSistema` | URL do sistema do cliente (da tabela clientes) | https://demo.singleone.com.br |

---

## 📅 Datas e Prazos

| Marcador | Descrição | Exemplo |
|----------|-----------|---------|
| `@dataLimite` | Data limite para ação | 17/12/2024 |
| `@prazoCalculado` | Prazo em dias úteis | 5 dias úteis |
| `@dataForcado` | Data/hora em que o inventário foi forçado | 12/12/2024 15:30 |

---

## 👥 Informações de Contato/Equipe

| Marcador | Descrição | Exemplo |
|----------|-----------|---------|
| `@nomeEquipe` | Nome da equipe responsável | TI/Patrimônio |
| `@emailEquipe` | E-mail da equipe | ti@empresa.com |
| `@telefoneEquipe` | Telefone/ramal da equipe | Ramal 4000 |
| `@usuarioQueForcou` | Nome do usuário que forçou o inventário | Maria Administradora |

---

## 💬 Mensagens Dinâmicas

| Marcador | Descrição | Exemplo |
|----------|-----------|---------|
| `@mensagemAdicional` | Mensagem adicional opcional (HTML) | `<p>Prazo estendido devido ao feriado</p>` |

---

## 🔗 URLs e Links

### Usando `@urlSistema`

O marcador `@urlSistema` busca automaticamente a URL configurada na tabela `clientes` (campo `site_url`).

**Exemplos de uso:**

```html
<!-- Link para Meu Patrimônio -->
<a href="@urlSistema/patrimonio">Acessar Meu Patrimônio</a>

<!-- Link para Login -->
<a href="@urlSistema/login">Fazer Login</a>

<!-- Link para Auto-Inventário -->
<p>Para realizar seu auto-inventário acesse: @urlSistema/patrimonio</p>
```

**✅ IMPORTANTE**: Sempre use `@urlSistema` em vez de URLs hardcoded como `http://localhost:4200`!

---

## 📋 Templates que Usam Marcadores

### Template ID 6: Notificação de Inventário Forçado

**Marcadores utilizados:**
- `@nomeColaborador`
- `@cpf`
- `@matricula`
- `@cargo`
- `@empresa`
- `@dataLimite`
- `@prazoCalculado`
- `@nomeEquipe`
- `@emailEquipe`
- `@telefoneEquipe`
- `@usuarioQueForcou`
- `@dataForcado`
- `@mensagemAdicional`
- `@nomeEmpresa`
- `@urlSistema` ✨ **NOVO**

---

## 🛠️ Como Adicionar Novos Marcadores

1. Adicione a substituição no código C# usando `.Replace("@marcador", valor)`
2. Use o marcador no template HTML
3. Documente aqui neste arquivo
4. Teste enviando um e-mail de teste

---

## ⚠️ Boas Práticas

1. ✅ Sempre use marcadores em vez de valores hardcoded
2. ✅ Use `@urlSistema` para qualquer link do sistema
3. ✅ Teste os templates após modificações
4. ✅ Mantenha esta documentação atualizada
5. ❌ NUNCA use `localhost` ou IPs em templates

---

**Última atualização**: 12/12/2024

