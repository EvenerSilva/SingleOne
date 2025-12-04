# Template ID 6: Notificação de Inventário Forçado

## 📋 Descrição

Template de e-mail HTML enviado automaticamente quando um administrador força um inventário para um colaborador que não possui recursos cadastrados no sistema.

## 🎯 Objetivo

Notificar o colaborador de forma profissional que a equipe entrará em contato para realizar o levantamento dos recursos de TI sob sua responsabilidade.

---

## 📝 Informações do Template

| Campo | Valor |
|-------|-------|
| **ID** | 6 |
| **Tipo (Enum)** | `TipoTemplateEnum.NotificacaoInventarioForcado` |
| **Título** | Levantamento de Recursos de TI - Ação Necessária |
| **Arquivo HTML** | `NotificacaoInventarioForcado.html` |
| **Arquivo SQL** | `Insert_Template_InventarioForcado.sql` |

---

## 🔧 Variáveis Dinâmicas (Placeholders)

Todas as variáveis devem ser substituídas usando `.Replace()` no código C#:

### Variáveis Obrigatórias

| Variável | Descrição | Exemplo | Tipo |
|----------|-----------|---------|------|
| `@nomeColaborador` | Nome completo do colaborador | "Evener Silva" | String |
| `@dataLimite` | Data limite para resposta | "05/11/2025" | DateTime formatado |
| `@nomeEquipe` | Nome da equipe responsável | "TI/Patrimônio" | String |
| `@emailEquipe` | E-mail de contato | "patrimonio@empresa.com" | String |
| `@telefoneEquipe` | Telefone/ramal | "4000" | String |
| `@nomeEmpresa` | Nome da empresa | "TechCorp Ltda" | String |
| `@usuarioQueForçou` | Admin que forçou | "João Silva" | String |
| `@dataForcado` | Data que foi forçado | "28/10/2025" | DateTime formatado |

### Variáveis Opcionais

| Variável | Descrição | Exemplo | Padrão se vazio |
|----------|-----------|---------|-----------------|
| `@cpf` | CPF do colaborador | "324.543.XXX-XX" | (não usado no template atual) |
| `@matricula` | Matrícula | "324543" | (não usado no template atual) |
| `@cargo` | Cargo | "Diretor" | (não usado no template atual) |
| `@empresa` | Empresa do colaborador | "TechCorp" | (não usado no template atual) |
| `@prazoCalculado` | Prazo em dias úteis | "5 dias úteis" | (não usado no template atual) |
| `@mensagemAdicional` | Mensagem customizada do admin | "Observação: ..." | String vazia |

---

## 💻 Exemplo de Uso no Código C#

### 1. Buscar o Template

```csharp
var template = _templateRepository.Buscar(x => 
    x.Tipo == (int)TipoTemplateEnum.NotificacaoInventarioForcado && 
    x.Cliente == clienteId && 
    x.Ativo == true
).FirstOrDefault();

if (template == null)
{
    throw new Exception("Template de Notificação de Inventário Forçado não encontrado");
}
```

### 2. Substituir Variáveis

```csharp
var colaborador = _colaboradorRepository.ObterPorId(colaboradorId);
var prazoDias = request.PrazoDias ?? 5; // Default 5 dias
var dataLimite = DateTime.Now.AddDays(prazoDias);

var conteudoEmail = template.Conteudo
    .Replace("@nomeColaborador", colaborador.Nome)
    .Replace("@dataLimite", dataLimite.ToString("dd/MM/yyyy"))
    .Replace("@nomeEquipe", "TI/Patrimônio") // ou buscar de configuração
    .Replace("@emailEquipe", "patrimonio@empresa.com")
    .Replace("@telefoneEquipe", "4000")
    .Replace("@nomeEmpresa", cliente.Nome)
    .Replace("@mensagemAdicional", FormatarMensagemAdicional(request.MensagemAdicional))
    .Replace("@usuarioQueForçou", usuarioLogado.Nome)
    .Replace("@dataForcado", DateTime.Now.ToString("dd/MM/yyyy"));
```

### 3. Formatar Mensagem Adicional (Se Houver)

```csharp
private string FormatarMensagemAdicional(string mensagem)
{
    if (string.IsNullOrWhiteSpace(mensagem))
    {
        return ""; // Retorna vazio, o HTML não exibirá nada
    }
    
    return $@"
        <div class='mensagem-adicional'>
            <h3>📌 Observações Importantes:</h3>
            <p>{mensagem}</p>
        </div>";
}
```

### 4. Enviar E-mail

```csharp
_emailService.EnviarEmail(
    destinatario: colaborador.Email,
    assunto: template.Titulo, // "Levantamento de Recursos de TI - Ação Necessária"
    corpoHtml: conteudoEmail,
    clienteId: clienteId
);
```

---

## 📊 Fluxo de Implementação

```
1. Admin força inventário
   ↓
2. Sistema verifica checkbox "Enviar E-mail"
   ↓
3. Busca Template ID 6 do banco
   ↓
4. Substitui variáveis dinâmicas
   ↓
5. Envia e-mail para colaborador
   ↓
6. Registra envio no histórico (opcional)
```

---

## 🎨 Características do Design

- ✅ **Responsivo**: Adapta-se a qualquer dispositivo
- ✅ **Profissional**: Design moderno e clean
- ✅ **Claro**: Informações organizadas em seções
- ✅ **Iconografia**: Emojis para melhor visual
- ✅ **Destaque Visual**: Prazo em amarelo para chamar atenção
- ✅ **Impressão**: CSS otimizado para impressão

---

## 🔒 Segurança

- Todas as variáveis devem ser **sanitizadas** antes da substituição
- Evitar injection de HTML/JavaScript em `@mensagemAdicional`
- Validar e-mail do destinatário antes do envio

---

## 📝 Notas Importantes

1. O template é **HTML inline CSS** para garantir compatibilidade com clientes de e-mail
2. Todos os estilos estão embutidos na tag `<style>`
3. A variável `@mensagemAdicional` pode ser vazia (não exibirá nada)
4. O template é **versionado** (campo `versao` na tabela)
5. Cada **cliente** deve ter seu próprio template (multi-tenant)

---

## 🧪 Testes Recomendados

1. ✅ Enviar e-mail de teste para verificar renderização
2. ✅ Testar em diferentes clientes (Gmail, Outlook, Apple Mail)
3. ✅ Verificar responsividade em mobile
4. ✅ Testar com e sem mensagem adicional
5. ✅ Validar todas as variáveis substituídas corretamente

---

## 🔄 Histórico de Versões

| Versão | Data | Alterações |
|--------|------|------------|
| 1.0 | 28/10/2025 | Versão inicial do template |

---

## 📧 Exemplo de E-mail Final

**Assunto:** Levantamento de Recursos de TI - Ação Necessária

**Destinatário:** evener.silva@empresa.com

**Conteúdo:**

```
Olá Evener Silva,

Identificamos que você pode ter equipamentos ou recursos de TI sob 
sua responsabilidade que ainda não estão registrados em nosso sistema 
de controle patrimonial.

🔍 O que faremos:
Nossa equipe entrará em contato com você nos próximos dias...

📝 Como você pode se preparar:
• Notebook/Desktop
• Monitor(es)
• Teclado e Mouse
...

⏰ Prazo: 05/11/2025

📞 Dúvidas?
E-mail: patrimonio@empresa.com
Ramal: 4000

---
TechCorp Ltda
Equipe de Gestão de Patrimônio
Inventário forçado por: João Silva em 28/10/2025
```

---

## 🚀 Próximos Passos

1. ✅ Template criado e documentado
2. ⏳ Implementar lógica de envio no backend
3. ⏳ Adicionar checkbox no modal de forçar inventário (frontend)
4. ⏳ Testar fluxo completo
5. ⏳ Deploy em produção

---

**Desenvolvido por:** Equipe SingleOne  
**Data:** 28/10/2025  
**Versão da Documentação:** 1.0

