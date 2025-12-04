# 📋 Padrão de Número de Nota Fiscal - SingleOne

## 🇧🇷 Padrão Brasileiro (SEFAZ/Receita Federal)

De acordo com a legislação fiscal brasileira e padrão da SEFAZ, o número da Nota Fiscal Eletrônica (NF-e) possui as seguintes características:

### Especificações Técnicas

- **Quantidade de dígitos:** Máximo **9 dígitos**
- **Valor máximo permitido:** **999.999.999**
- **Formato:** Numérico sequencial
- **Base legal:** Manual de Integração NF-e - SEFAZ

### Exemplos Válidos

✅ `000000001` - Primeira nota
✅ `123456789` - Nota com 9 dígitos
✅ `999999999` - Último número válido

### Exemplos Inválidos

❌ `1234567890` - 10 dígitos (excede o limite)
❌ `4526265456` - 10 dígitos (excede o limite)
❌ `0` - Número zero não é permitido

## 💾 Implementação no SingleOne

### Backend (C#)

**Tipo de Dados:** `int` (System.Int32)

- **Capacidade:** -2.147.483.648 até 2.147.483.647
- **Adequação:** ✅ Suficiente (cobre até 999.999.999)

```csharp
// Models/Notasfiscai.cs
public int Numero { get; set; } // Máximo: 999.999.999
```

**Validação no Backend:**
- Tipo `int` já previne valores maiores que 2.147.483.647
- Validação adicional pode ser feita com `[Range(1, 999999999)]` se necessário

### Frontend (Angular/TypeScript)

**Validações Implementadas:**

```typescript
// nota-fiscal.component.ts
this.form = this.fb.group({
  numero: ['', [
    Validators.required,      // Campo obrigatório
    Validators.min(1),        // Mínimo: 1
    Validators.max(999999999) // Máximo: 999.999.999 (9 dígitos)
  ]]
});
```

**HTML:**

```html
<input type="number" 
       min="1" 
       max="999999999"
       placeholder="Digite o número da nota (máx. 9 dígitos)">
```

**Mensagens de Erro:**

1. **Campo vazio:** "Número da nota é obrigatório"
2. **Excede limite:** "Número da nota inválido! Máximo: 999.999.999 (padrão SEFAZ - 9 dígitos)"

**Mensagem Informativa:**

- Exibida quando o campo está válido: "Padrão NF-e: até 9 dígitos (999.999.999)"

## 🎯 Benefícios da Validação

### 1️⃣ Conformidade Legal
- Segue o padrão estabelecido pela SEFAZ
- Evita problemas com integração fiscal
- Garante consistência com sistemas governamentais

### 2️⃣ Experiência do Usuário
- ✅ **Feedback Imediato:** Usuário vê o erro antes de tentar salvar
- ✅ **Mensagem Clara:** Explica o limite e o motivo (padrão SEFAZ)
- ✅ **Visual Intuitivo:** Campo fica vermelho quando inválido
- ✅ **Informação Contextual:** Tooltip mostra o padrão correto

### 3️⃣ Integridade de Dados
- Previne entrada de dados inválidos
- Mantém compatibilidade com o tipo `int` do banco
- Evita erros de overflow ou conversão

## 🔍 Por Que NÃO usar BIGINT?

### Motivos Técnicos

1. **Desnecessário:** O padrão SEFAZ limita a 9 dígitos (999.999.999)
2. **Desperdício de Espaço:** `BIGINT` usa 8 bytes vs 4 bytes do `INT`
3. **Performance:** Operações com `INT` são mais rápidas
4. **Validação de Negócio:** Números maiores seriam **inválidos** segundo a legislação

### Comparação de Tipos

| Tipo | Bytes | Limite Máximo | Adequado para NF-e? |
|------|-------|--------------|---------------------|
| `SMALLINT` | 2 | 32.767 | ❌ Insuficiente |
| `INT` | 4 | 2.147.483.647 | ✅ **Ideal** |
| `BIGINT` | 8 | 9.223.372.036.854.775.807 | ⚠️ Excessivo |

## 📊 Estatísticas

Com o tipo `INT` (limite: 2.147.483.647):

- ✅ Cobre **100%** dos números válidos de NF-e (até 999.999.999)
- ✅ Ainda tem margem de **214%** acima do máximo legal
- ✅ Espaço de armazenamento otimizado

## 🚨 Casos Especiais

### Se o usuário digitar número maior que 999.999.999

**Ação do Sistema:**
1. Campo fica vermelho (`.error`)
2. Exibe mensagem: "Número da nota inválido! Máximo: 999.999.999 (padrão SEFAZ - 9 dígitos)"
3. Botão "Avançar" permanece desabilitado
4. Usuário não consegue prosseguir até corrigir

**Por quê?**
- Número está **fora do padrão fiscal brasileiro**
- Seria **rejeitado pela SEFAZ** de qualquer forma
- Previne erros futuros em integrações fiscais

## 📚 Referências

- **SEFAZ:** Manual de Integração NF-e
- **Receita Federal:** Especificações técnicas da NF-e
- **Padrão Nacional:** Nota Fiscal Eletrônica (NF-e) - Layout 4.0

## 🔄 Histórico de Alterações

| Data | Alteração | Motivo |
|------|-----------|--------|
| 01/11/2025 | Adicionada validação `max(999999999)` no frontend | Conformidade com padrão SEFAZ |
| 01/11/2025 | Mantido tipo `int` no backend | Adequado ao padrão brasileiro |
| 01/11/2025 | Mensagens de erro contextualizadas | Melhor UX e orientação ao usuário |

---

**✅ Conclusão:** O tipo `INT` com validação até 999.999.999 atende perfeitamente ao padrão brasileiro de NF-e, proporcionando validação de negócio adequada e otimização de recursos.

