# 🤖 Guia de Integração: Oni o Sábio + Ollama (IA/NLP Local)

## 📋 **O que foi implementado:**

✅ **Serviço de IA Local (`OllamaService`)**  
✅ **RAG (Retrieval-Augmented Generation)** - Combina busca na base de conhecimento + IA  
✅ **Integração com TinOneService** - Usa IA quando habilitada  
✅ **Fallback inteligente** - Se IA não disponível, usa resposta genérica  
✅ **Logs detalhados** para debug  

---

## 🔧 **Como Funciona:**

1. **Usuário faz uma pergunta** ao Oni
2. **Sistema tenta responder com FAQ** (busca por palavras-chave)
3. **Se não encontrar, tenta identificar processo** guiado
4. **Se ainda não encontrar E IA estiver habilitada:**
   - 🔍 Busca contexto relevante na base de conhecimento (FAQ, processos, campos)
   - 🤖 Envia contexto + pergunta para Ollama
   - ✨ Ollama gera resposta personalizada usando IA
5. **Se IA não disponível:** resposta genérica

---

## 📥 **INSTALAÇÃO DO OLLAMA (Windows)**

### **Passo 1: Baixar e Instalar**

#### **Opção A: Download Direto (Recomendado)**
1. Acesse: https://ollama.com/download/windows
2. Baixe `OllamaSetup.exe`
3. Execute o instalador
4. Siga o assistente de instalação
5. **Reinicie o terminal** após instalação

#### **Opção B: Via Winget**
```powershell
winget install Ollama.Ollama
```

#### **Opção C: Via Chocolatey**
```powershell
choco install ollama
```

### **Passo 2: Verificar Instalação**
```powershell
ollama --version
```

Deve retornar algo como: `ollama version 0.x.x`

---

## 🧠 **BAIXAR MODELO DE IA**

### **Modelos Recomendados:**

#### **1. Llama 3.2 (3B) - RECOMENDADO para iniciar**
```bash
ollama pull llama3.2:3b
```
- **Tamanho:** ~2GB
- **RAM necessária:** ~4GB
- **Velocidade:** Muito rápida
- **Qualidade:** Boa para português

#### **2. Mistral (7B) - Mais poderoso**
```bash
ollama pull mistral:7b
```
- **Tamanho:** ~4GB
- **RAM necessária:** ~8GB
- **Velocidade:** Média
- **Qualidade:** Excelente

#### **3. Phi-3 Mini (3.8B) - Alternativa leve**
```bash
ollama pull phi3:mini
```
- **Tamanho:** ~2.3GB
- **RAM necessária:** ~6GB
- **Velocidade:** Rápida
- **Qualidade:** Boa

### **Verificar Modelos Instalados:**
```bash
ollama list
```

---

## ⚙️ **CONFIGURAR NO SINGLEONE**

### **1. Habilitar IA no Sistema**

1. Faça login no SingleOne como **administrador**
2. Acesse **Configurações → Oni o Sábio**
3. Ative a opção: **"Habilitar IA/NLP (BETA)"**
4. Clique em **"Salvar Configurações"**
5. Aguarde 2 segundos para reload automático

### **2. Verificar se Ollama está Rodando**

Abra um novo terminal e execute:
```bash
ollama serve
```

**OU** verifique se o serviço já está rodando:
```powershell
Get-Process -Name "ollama" -ErrorAction SilentlyContinue
```

---

## 🧪 **TESTAR A INTEGRAÇÃO**

### **Teste 1: Verificar Disponibilidade**
```bash
curl http://localhost:11434/api/tags
```
Deve retornar JSON com lista de modelos instalados.

### **Teste 2: Perguntar ao Oni**

Com IA **DESABILITADA** (comportamento antigo):
```
Pergunta: "o que posso fazer com equipamentos no sistema?"
Resposta: "Desculpe, ainda não sei responder essa pergunta. Estou aprendendo! 🤖..."
```

Com IA **HABILITADA**:
```
Pergunta: "o que posso fazer com equipamentos no sistema?"
Resposta: "No SingleOne, você pode gerenciar equipamentos de diversas formas! 🖥️

Aqui estão as principais funcionalidades:

• **Cadastrar Equipamentos** - Adicione novos equipamentos ao inventário com informações técnicas e fiscais
• **Movimentar Equipamentos** - Faça entregas, devoluções e transferências entre colaboradores
• **Criar Requisições** - Solicite equipamentos para colaboradores conforme política de elegibilidade
• **Consultar Relatórios** - Veja equipamentos por status, garantias próximas do vencimento, e muito mais!
• **Exportar Dados** - Gere planilhas Excel com os dados dos equipamentos

Posso explicar qualquer um desses processos em detalhes! 😊"
```

---

## 📊 **LOGS PARA VERIFICAR FUNCIONAMENTO**

Após fazer uma pergunta, verifique os logs do backend:

```
[TinOne] Processando pergunta: o que posso fazer com equipamentos?
[TinOne] IA habilitada - tentando gerar resposta com Ollama
[TinOne RAG] Buscando contexto relevante
[TinOne RAG] ✅ Contexto montado com 543 caracteres
[Ollama] Gerando resposta para: o que posso fazer com equipamentos?
[Ollama] ✅ Resposta gerada com sucesso
[TinOne] ✅ Resposta gerada com IA
```

Se IA não estiver disponível:
```
[TinOne] Ollama não disponível, usando resposta genérica
```

---

## ⚡ **DESEMPENHO E REQUISITOS**

### **Requisitos Mínimos:**
- **CPU:** 4 cores
- **RAM:** 8GB (4GB livres)
- **Disco:** 5GB livres
- **Windows:** 10/11 64-bit

### **Recomendado:**
- **CPU:** 8 cores
- **RAM:** 16GB
- **Disco:** SSD com 10GB livres

### **Tempo de Resposta:**
- **Llama 3.2 (3B):** 2-5 segundos
- **Mistral (7B):** 5-10 segundos

---

## 🔄 **TROCAR MODELO**

Para usar um modelo diferente, edite `OllamaService.cs`:

```csharp
public OllamaService(ILogger<OllamaService> logger)
{
    _logger = logger;
    _ollamaUrl = "http://localhost:11434";
    _modelo = "mistral:7b"; // ← Altere aqui
    // ...
}
```

Recompile e reinicie o backend.

---

## 🛠️ **TROUBLESHOOTING**

### **Problema: Ollama não inicia**
```bash
# Verificar se porta 11434 está ocupada
netstat -ano | findstr :11434

# Matar processo se necessário
taskkill /PID <PID> /F

# Iniciar Ollama
ollama serve
```

### **Problema: Modelo não foi baixado**
```bash
ollama list
ollama pull llama3.2:3b
```

### **Problema: Erro de conexão**
Verifique se Ollama está rodando:
```bash
curl http://localhost:11434/api/tags
```

### **Problema: Respostas muito lentas**
- Troque para modelo menor (llama3.2:3b)
- Aumente a RAM disponível
- Feche outros programas

---

## 🎛️ **AJUSTAR PARÂMETROS DA IA**

Em `OllamaService.cs`, você pode ajustar:

```csharp
options = new
{
    temperature = 0.7,    // Criatividade (0.0-1.0) - Maior = mais criativo
    top_p = 0.9,          // Diversidade (0.0-1.0)
    max_tokens = 500      // Tamanho máximo da resposta
}
```

---

## 📚 **PRÓXIMOS PASSOS**

1. ✅ **Teste básico** - Pergunte ao Oni coisas simples
2. ✅ **Ajuste fino** - Modifique temperatura e max_tokens
3. ✅ **Expanda FAQ** - Adicione mais conteúdo em `faq.json`
4. 🔮 **Futuro:** Fine-tuning do modelo com dados específicos do SingleOne

---

## 🆘 **SUPORTE**

- **Ollama Docs:** https://ollama.com/docs
- **Modelos disponíveis:** https://ollama.com/library
- **GitHub Ollama:** https://github.com/ollama/ollama

---

## ✅ **CHECKLIST COMPLETO**

- [ ] Ollama instalado e funcionando
- [ ] Modelo baixado (llama3.2:3b ou outro)
- [ ] `ollama serve` rodando
- [ ] Backend recompilado e reiniciado
- [ ] IA habilitada nas configurações do Oni
- [ ] Teste realizado com sucesso

---

🎉 **Pronto! Seu Oni agora é inteligente de verdade!** 🦉🤖

