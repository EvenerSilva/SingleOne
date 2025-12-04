# 🤖 TinOne - Assistente Inteligente do SingleOne

## ✅ Implementação Completa - Fase 0 (MVP)

Documentação completa da implementação inicial do assistente TinOne.

---

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [Arquitetura](#arquitetura)
3. [Instalação](#instalação)
4. [Estrutura de Arquivos](#estrutura-de-arquivos)
5. [Como Usar](#como-usar)
6. [Configuração](#configuração)
7. [Sugestões de Mascote](#sugestões-de-mascote)
8. [Próximas Fases](#próximas-fases)
9. [FAQ](#faq)

---

## 🎯 Visão Geral

O **TinOne** é um assistente virtual inteligente e contextual integrado ao SingleOne. Ele ajuda os usuários a navegar pelo sistema, responde dúvidas e fornece orientação passo-a-passo.

### Características Principais

✅ **100% Não-Invasivo**: Não afeta funcionalidades existentes  
✅ **Desabilitável**: Pode ser desativado via parâmetros  
✅ **Modular**: Componentes isolados  
✅ **Contextual**: Entende em qual tela o usuário está  
✅ **Inteligente**: Responde perguntas e guia processos  
✅ **Analytics**: Rastreia uso para melhorias contínuas  

---

## 🏗️ Arquitetura

### Backend (.NET Core 6.0)

```
SingleOneAPI/
├── Controllers/
│   └── TinOneController.cs          # API REST do TinOne
├── Services/TinOne/
│   ├── ITinOneService.cs            # Interface principal
│   ├── TinOneService.cs             # Lógica de negócio
│   ├── ITinOneConfigService.cs      # Interface de config
│   └── TinOneConfigService.cs       # Gerencia parâmetros
├── Models/TinOne/
│   ├── TinOneConfig.cs              # Modelo de configuração
│   ├── TinOneAnalytics.cs           # Modelo de analytics
│   └── TinOneConversa.cs            # Modelo de conversas
├── DTOs/TinOne/
│   └── TinOnePerguntaDTO.cs         # DTOs de comunicação
└── TinOne/KnowledgeBase/
    ├── faq.json                     # Perguntas frequentes
    ├── fields.json                  # Dicionário de campos
    └── processes.json               # Processos guiados
```

### Frontend (Angular 10)

```
src/app/tinone/
├── components/
│   ├── tinone-widget/               # Botão flutuante
│   ├── tinone-chat/                 # Janela de chat
│   └── tinone-tooltip/              # Tooltips contextuais
├── services/
│   ├── tinone.service.ts            # Serviço principal
│   ├── tinone-config.service.ts     # Gerencia configuração
│   └── tinone-context.service.ts    # Detecta contexto
├── directives/
│   └── tinone-help.directive.ts     # Diretiva para campos
├── models/
│   └── tinone.models.ts             # Interfaces TypeScript
└── tinone.module.ts                 # Módulo isolado
```

---

## 🚀 Instalação

### 1. Configurar Banco de Dados

```powershell
# Execute o script de instalação
cd C:\SingleOne\SingleOne_Backend
.\aplicar-tinone.ps1
```

Ou manualmente:

```sql
-- Adicionar parâmetros
\i setup-tinone-params.sql

-- Criar tabelas (opcional - analytics)
\i create-tinone-tables.sql
```

### 2. Configurar Backend

Os serviços já estão registrados em `ServicesExtension.cs`:

```csharp
services.AddScoped<ITinOneConfigService, TinOneConfigService>();
services.AddScoped<ITinOneService, TinOneService>();
```

### 3. Executar Aplicação

```powershell
# Backend
cd SingleOne_Backend
.\run-backend.ps1

# Frontend (novo terminal)
cd SingleOne_Frontend
.\run-frontend.ps1
```

### 4. Verificar Instalação

Acesse: `http://localhost:4200`

O botão do TinOne deve aparecer no canto inferior direito! 🎉

---

## 📁 Estrutura de Arquivos

### Backend - Novos Arquivos

```
SingleOneAPI/
├── Controllers/TinOneController.cs
├── Services/TinOne/
│   ├── ITinOneService.cs
│   ├── TinOneService.cs
│   ├── ITinOneConfigService.cs
│   └── TinOneConfigService.cs
├── Models/TinOne/
│   ├── TinOneConfig.cs
│   ├── TinOneAnalytics.cs
│   └── TinOneConversa.cs
├── DTOs/TinOne/
│   └── TinOnePerguntaDTO.cs
└── TinOne/KnowledgeBase/
    ├── faq.json
    ├── fields.json
    └── processes.json
```

### Frontend - Novos Arquivos

```
src/app/tinone/
├── components/
│   ├── tinone-widget/
│   │   ├── tinone-widget.component.ts
│   │   ├── tinone-widget.component.html
│   │   └── tinone-widget.component.scss
│   ├── tinone-chat/
│   │   ├── tinone-chat.component.ts
│   │   ├── tinone-chat.component.html
│   │   └── tinone-chat.component.scss
│   └── tinone-tooltip/
│       ├── tinone-tooltip.component.ts
│       ├── tinone-tooltip.component.html
│       └── tinone-tooltip.component.scss
├── services/
│   ├── tinone.service.ts
│   ├── tinone-config.service.ts
│   └── tinone-context.service.ts
├── directives/
│   └── tinone-help.directive.ts
├── models/
│   └── tinone.models.ts
└── tinone.module.ts
```

### Arquivos Modificados

**Backend:**
- `DependencyInjection/ServicesExtension.cs` - Adicionadas 2 linhas

**Frontend:**
- `app.module.ts` - Adicionadas 2 linhas
- `app.component.html` - Adicionada 1 linha

---

## 🎮 Como Usar

### Para Usuários Finais

1. **Abrir o Assistente**
   - Clique no botão circular no canto inferior direito
   
2. **Fazer Perguntas**
   - Digite sua pergunta no chat
   - Exemplo: "Como criar uma requisição?"
   
3. **Fechar o Assistente**
   - Clique no X no canto superior direito do chat
   - Ou clique novamente no botão flutuante

### Para Administradores

#### Habilitar/Desabilitar TinOne

**Opção 1 - Via Banco de Dados:**
```sql
-- Desabilitar globalmente
UPDATE tinone_config 
SET valor = 'false' 
WHERE chave = 'TINONE_HABILITADO' AND cliente IS NULL;

-- Desabilitar para um cliente específico
INSERT INTO tinone_config (cliente, chave, valor, descricao, ativo) 
VALUES (1, 'TINONE_HABILITADO', 'false', 'TinOne desabilitado para cliente 1', true)
ON CONFLICT (cliente, chave) DO UPDATE SET valor = 'false';
```

**Opção 2 - Via Interface (Futuro):**
- Configurações > Parâmetros > Buscar "TINONE"
- Alterar valor de `true` para `false`

#### Habilitar Funcionalidades Específicas

```sql
-- Desabilitar apenas chat (mantém tooltips)
UPDATE tinone_config SET valor = 'false' WHERE chave = 'TINONE_CHAT_HABILITADO' AND cliente IS NULL;

-- Desabilitar tooltips contextuais
UPDATE tinone_config SET valor = 'false' WHERE chave = 'TINONE_TOOLTIPS_HABILITADO' AND cliente IS NULL;

-- Habilitar guias interativos (quando implementados)
UPDATE tinone_config SET valor = 'true' WHERE chave = 'TINONE_GUIAS_HABILITADO' AND cliente IS NULL;
```

---

## ⚙️ Configuração

### Configurações Disponíveis

**Tabela:** `tinone_config` (criada especificamente para o TinOne)

| Configuração | Padrão | Descrição |
|-----------|--------|-----------|
| `TINONE_HABILITADO` | `true` | Habilita/desabilita TinOne globalmente |
| `TINONE_CHAT_HABILITADO` | `true` | Habilita funcionalidade de chat |
| `TINONE_TOOLTIPS_HABILITADO` | `true` | Habilita tooltips contextuais |
| `TINONE_GUIAS_HABILITADO` | `false` | Habilita guias passo-a-passo |
| `TINONE_SUGESTOES_PROATIVAS` | `false` | Habilita sugestões proativas (beta) |
| `TINONE_IA_HABILITADA` | `false` | Habilita IA/NLP (requer Ollama) |
| `TINONE_ANALYTICS` | `true` | Habilita analytics de uso |
| `TINONE_DEBUG_MODE` | `false` | Modo debug para desenvolvimento |
| `TINONE_POSICAO` | `bottom-right` | Posição do widget |
| `TINONE_COR_PRIMARIA` | `#4a90e2` | Cor primária (hex) |

### Personalizar Base de Conhecimento

Edite os arquivos JSON em `SingleOneAPI/TinOne/KnowledgeBase/`:

**1. Adicionar FAQ (faq.json):**
```json
{
  "nova pergunta": "Resposta para a pergunta..."
}
```

**2. Adicionar Campo (fields.json):**
```json
{
  "modulo.campo": {
    "CampoId": "modulo.campo",
    "Nome": "Nome do Campo",
    "Descricao": "Descrição do que o campo faz",
    "Exemplo": "Exemplo de valor",
    "Tipo": "string",
    "Obrigatorio": true,
    "Dicas": ["Dica 1", "Dica 2"]
  }
}
```

**3. Adicionar Processo (processes.json):**
```json
{
  "id-processo": {
    "ProcessoId": "id-processo",
    "Nome": "Nome do Processo",
    "Descricao": "Descrição completa",
    "Passos": [
      {
        "Id": 1,
        "Titulo": "Primeiro passo",
        "Descricao": "O que fazer...",
        "Rota": "/rota-angular"
      }
    ]
  }
}
```

---

## 🎨 Sugestões de Mascote - Oni o Sábio

O assistente virtual precisa de um mascote amigável e tecnológico! Aqui estão algumas sugestões:

### Opção 1: TinOne o Robozinho 🤖

**Conceito:** Um robô simpático e moderno

**Características:**
- Corpo arredondado (lembra um "1" estilizado)
- Cor principal: Azul (#4a90e2)
- Olhos expressivos (podem mudar de acordo com o contexto)
- Antena com LED que pisca quando "pensando"
- Braços pequenos que acenam

**Personalidade:**
- Amigável e prestativo
- Sempre com um sorriso
- Curioso e paciente

**Variações:**
- 😊 Normal/Feliz - Estado padrão
- 🤔 Pensando - Processando pergunta
- 😄 Animado - Encontrou a resposta
- 😅 Confuso - Não entendeu a pergunta
- 💤 Dormindo - Modo inativo

### Opção 2: Tiny o Assistente Virtual 💙

**Conceito:** Uma esfera flutuante inteligente

**Características:**
- Esfera azul luminosa
- Padrões de luz que mudam conforme o estado
- Holograma que projeta ícones
- Design minimalista e futurista

**Estados:**
- Pulsando suavemente - Aguardando
- Girando - Processando
- Brilhando - Resposta pronta
- Cores diferentes para diferentes tipos de mensagens

### Opção 3: Single o Ajudante 👾

**Conceito:** Mascote estilo pixel art retrô-futurista

**Características:**
- Design pixelado mas moderno
- Forma lembra o número "1"
- Animações suaves tipo 8-bit
- Cores: Azul, branco e gradientes

### Opção 4: Oni o Sábio 🦉

**Conceito:** Uma coruja tecnológica

**Características:**
- Coruja estilizada com elementos tech
- Óculos de realidade virtual
- Penas em tons de azul
- Carrega um tablet/dispositivo

**Simbolismo:**
- Coruja = Sabedoria, conhecimento
- Tech = Modernidade, inovação

---

### 🎨 Especificações Técnicas para o Designer

#### Tamanhos Necessários:

1. **Ícone do botão** (60x60px):
   - Versão simplificada do mascote
   - Fundo transparente
   - SVG (escalável)

2. **Avatar no chat** (40x40px):
   - Rosto/cabeça do mascote
   - Formato circular
   - PNG ou SVG

3. **Versão completa** (200x200px):
   - Para uso em outras áreas
   - Fundo transparente
   - PNG de alta qualidade

4. **Estados/Expressões:**
   - Normal
   - Pensando
   - Feliz
   - Confuso
   - Loading/Aguardando

#### Paleta de Cores Sugerida:

- **Primária:** #4a90e2 (Azul SingleOne)
- **Secundária:** #ffffff (Branco)
- **Acento:** #5cb85c (Verde - sucesso)
- **Atenção:** #f0ad4e (Laranja - dicas)
- **Erro:** #e74c3c (Vermelho - problemas)

#### Estilo Visual:

- Design flat/minimalista
- Bordas arredondadas
- Sombras suaves
- Animações suaves (CSS transitions)
- Expressivo mas profissional

---

### 📸 Referências de Inspiração

Procure referências de:
- **Intercom chatbot** - Design limpo e moderno
- **Clippy (Microsoft Office)** - Interatividade (mas mais moderno!)
- **Wall-E** - Personalidade amigável
- **BB-8 (Star Wars)** - Design esférico e simpático
- **Baymax (Big Hero 6)** - Amigável e prestativo

---

### 🖼️ Como Implementar o Mascote

Após criar a imagem:

1. **Salvar arquivos:**
```
src/assets/tinone/
├── tinone-icon.svg          # Ícone do botão
├── tinone-avatar.png        # Avatar do chat
├── tinone-full.png          # Versão completa
└── expressions/
    ├── thinking.svg
    ├── happy.svg
    └── confused.svg
```

2. **Atualizar componente widget:**
```html
<!-- tinone-widget.component.html -->
<img src="assets/tinone/tinone-icon.svg" 
     alt="TinOne" 
     width="32" 
     height="32">
```

3. **Atualizar componente chat:**
```html
<!-- tinone-chat.component.html -->
<img src="assets/tinone/tinone-avatar.png" 
     alt="TinOne" 
     class="tinone-avatar">
```

---

## 🔄 Próximas Fases

### Fase 1: Melhorias na Base de Conhecimento (2 semanas)

- [ ] Expandir FAQ com mais perguntas
- [ ] Adicionar todos os campos do sistema
- [ ] Criar processos guiados completos
- [ ] Melhorar algoritmo de busca

### Fase 2: Tooltips Contextuais (2 semanas)

- [ ] Implementar tooltip visual
- [ ] Adicionar diretiva em campos principais
- [ ] Integrar com base de conhecimento
- [ ] Criar dicionário completo de campos

### Fase 3: Guias Interativos (3 semanas)

- [ ] Sistema de highlight de elementos
- [ ] Navegação passo-a-passo
- [ ] Rastreamento de progresso
- [ ] Animações de transição

### Fase 4: Inteligência Artificial (4 semanas)

- [ ] Configurar Ollama local
- [ ] Integrar LLM (Llama 3.1)
- [ ] Implementar RAG
- [ ] Sistema de fallback

### Fase 5: Analytics e Melhorias (2 semanas)

- [ ] Dashboard de analytics
- [ ] Relatórios de uso
- [ ] Sistema de feedback
- [ ] Melhoria contínua baseada em dados

---

## ❓ FAQ

### Como desabilitar o TinOne?

```sql
UPDATE tinone_config SET valor = 'false' WHERE chave = 'TINONE_HABILITADO' AND cliente IS NULL;
```

### O TinOne afeta as funcionalidades existentes?

Não! Ele é 100% isolado e pode ser removido sem impacto.

### Como adicionar novas perguntas?

Edite o arquivo `faq.json` em `TinOne/KnowledgeBase/`.

### O TinOne funciona offline?

Sim! A base de conhecimento é local. Apenas funcionalidades de IA requerem Ollama.

### Como personalizar as cores?

Altere o parâmetro `TINONE_COR_PRIMARIA` no banco de dados.

### O TinOne rastreia conversas dos usuários?

Apenas se `TINONE_ANALYTICS` estiver habilitado. E é anônimo para melhorias do sistema.

### Como remover completamente o TinOne?

1. Desabilite via parâmetro
2. Remova linha do `app.component.html`
3. Remova módulo do `app.module.ts`
4. Delete pasta `tinone/` do frontend e backend

---

## 📞 Suporte

Para dúvidas ou problemas com o TinOne:

1. Verifique os logs do backend
2. Verifique console do navegador (F12)
3. Consulte este documento
4. Entre em contato com a equipe de desenvolvimento

---

## 🎉 Conclusão

O TinOne foi implementado com sucesso! Ele está pronto para ajudar os usuários do SingleOne.

**Próximos passos:**
1. ✅ Testar funcionamento básico
2. ✅ Criar mascote visual
3. ✅ Expandir base de conhecimento
4. ✅ Coletar feedback dos usuários

---

**Desenvolvido com ❤️ para o SingleOne**

