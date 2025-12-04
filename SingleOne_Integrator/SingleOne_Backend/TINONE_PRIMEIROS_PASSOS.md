# 🚀 Oni o Sábio - Primeiros Passos

## ✅ Implementação Concluída!

O assistente TinOne foi implementado com sucesso! Aqui está o que fazer agora:

---

## 📋 Checklist de Ativação

### 1️⃣ Instalar no Banco de Dados

```powershell
cd C:\SingleOne\SingleOne_Backend
.\aplicar-tinone.ps1
```

Ou manualmente no PostgreSQL:

```sql
\i setup-tinone-params.sql
\i create-tinone-tables.sql
```

### 2️⃣ Compilar e Executar

```powershell
# Terminal 1 - Backend
cd C:\SingleOne\SingleOne_Backend
.\run-backend.ps1

# Terminal 2 - Frontend
cd C:\SingleOne\SingleOne_Frontend
npm start
```

### 3️⃣ Verificar no Navegador

Acesse: `http://localhost:4200`

Você deve ver um **botão circular azul** no canto inferior direito! 🎉

---

## 🎮 Testando o TinOne

### Teste 1: Abrir o Chat

1. Clique no botão circular azul
2. O chat deve abrir com mensagem de boas-vindas
3. Digite "olá" e pressione Enter
4. O TinOne deve responder!

### Teste 2: Fazer Perguntas

Tente estas perguntas:

```
- Como criar uma requisição?
- O que é patrimônio?
- Como cadastrar equipamento?
- O que são cargos de confiança?
```

### Teste 3: Fechar o Chat

- Clique no X no header do chat
- Ou clique novamente no botão flutuante

---

## ⚙️ Configuração Rápida

### Desabilitar TinOne

Se precisar desabilitar temporariamente:

```sql
UPDATE tinone_config 
SET valor = 'false' 
WHERE chave = 'TINONE_HABILITADO' AND cliente IS NULL;
```

### Mudar Cor

```sql
UPDATE tinone_config 
SET valor = '#ff5733'  -- Sua cor em hexadecimal
WHERE chave = 'TINONE_COR_PRIMARIA' AND cliente IS NULL;
```

### Desabilitar Apenas Chat (mantém tooltips)

```sql
UPDATE tinone_config 
SET valor = 'false' 
WHERE chave = 'TINONE_CHAT_HABILITADO' AND cliente IS NULL;
```

---

## 🎨 Próximo Passo: Criar o Mascote!

O TinOne precisa de uma identidade visual! Veja as sugestões em:

📄 **TINONE_IMPLEMENTACAO_COMPLETA.md** - Seção "Sugestões de Mascote"

### Opções de Mascote:

1. **TinOne o Robozinho** 🤖 - Robô simpático e moderno
2. **Tiny o Assistente Virtual** 💙 - Esfera luminosa futurista
3. **Single o Ajudante** 👾 - Pixel art retrô
4. **Oni o Sábio** 🦉 - Coruja tecnológica

### Como Criar:

1. Escolha um conceito
2. Crie os arquivos:
   - `tinone-icon.svg` (60x60px) - Botão
   - `tinone-avatar.png` (40x40px) - Chat
   - `tinone-full.png` (200x200px) - Completo

3. Salve em: `src/assets/tinone/`

4. Atualize os componentes com as imagens

---

## 📚 Expandir Base de Conhecimento

Adicione mais perguntas editando:

**`SingleOneAPI/TinOne/KnowledgeBase/faq.json`:**

```json
{
  "sua pergunta aqui": "Resposta completa aqui..."
}
```

**Dica:** Use linguagem natural e variações da mesma pergunta.

---

## 🐛 Troubleshooting

### Botão não aparece?

1. Verifique o console do navegador (F12)
2. Confirme que o parâmetro está `true`:
   ```sql
   SELECT * FROM tinone_config WHERE chave = 'TINONE_HABILITADO';
   ```

### Chat não responde?

1. Verifique logs do backend
2. Confirme que a API está rodando: `http://localhost:5000/swagger`
3. Teste o endpoint: `GET /api/tinone/status`

### Erro ao compilar?

Se houver erros de compilação:

```powershell
# Frontend - Limpar e reinstalar
cd C:\SingleOne\SingleOne_Frontend
Remove-Item -Recurse node_modules
npm install
```

---

## 📊 Monitorar Uso

### Ver estatísticas de uso:

```sql
SELECT 
    COUNT(*) as total_perguntas,
    COUNT(DISTINCT usuario_id) as usuarios_ativos,
    DATE(created_at) as data
FROM tinone_analytics
GROUP BY DATE(created_at)
ORDER BY data DESC;
```

### Perguntas mais frequentes:

```sql
SELECT 
    pergunta, 
    COUNT(*) as total
FROM tinone_analytics
WHERE pergunta IS NOT NULL
GROUP BY pergunta
ORDER BY total DESC
LIMIT 10;
```

---

## 🔄 Próximas Melhorias

Quando estiver pronto, implemente:

### Fase 1: Tooltips Contextuais
- Adicionar ajuda em campos específicos
- Use a diretiva: `<input tinOneHelp="campo.id">`

### Fase 2: Guias Interativos
- Processos passo-a-passo
- Navegação assistida

### Fase 3: IA/NLP Local
- Integrar Ollama
- Respostas mais inteligentes

---

## 📞 Precisa de Ajuda?

**Documentação Completa:**
📄 `TINONE_IMPLEMENTACAO_COMPLETA.md`

**Arquivos Importantes:**
- Backend: `Controllers/TinOneController.cs`
- Frontend: `src/app/tinone/`
- Base de Conhecimento: `TinOne/KnowledgeBase/`

---

## 🎉 Parabéns!

O TinOne está funcionando! Agora é hora de:

1. ✅ Criar o mascote visual
2. ✅ Adicionar mais perguntas na FAQ
3. ✅ Coletar feedback dos usuários
4. ✅ Expandir funcionalidades

**O SingleOne agora tem um assistente inteligente! 🚀**

---

**Desenvolvido com ❤️ para melhorar a experiência do usuário**

