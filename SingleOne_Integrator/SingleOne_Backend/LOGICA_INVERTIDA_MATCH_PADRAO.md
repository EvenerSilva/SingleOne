# Lógica Invertida: Match por Padrão como Default

## 🎯 Decisão de Design

**Match por Padrão é o comportamento DEFAULT (recomendado)**  
**Match Exato é a EXCEÇÃO (casos raros)**

## 📊 Raciocínio

### 99% dos casos querem agrupar
- "Gerente" → Todos os gerentes
- "Diretor" → Todos os diretores
- "Coordenador" → Todos os coordenadores

### 1% dos casos querem exato
- Casos muito específicos e raros
- Geralmente não fazem sentido com entrada livre

## 🔄 Implementação

### Frontend (Form)
```typescript
matchexato: [false]  // false = usa padrão (default), true = match exato (exceção)
```

### Conversão para Backend
```typescript
// Ao salvar/atualizar:
usarpadrao: !formValue.matchexato

// Ao carregar para edição:
matchexato: !cargo.usarpadrao
```

### Campo no Banco (mantido)
```sql
usarpadrao BOOLEAN DEFAULT false
```

**Nota:** O campo no banco se chama `usarpadrao`, mas a lógica está invertida no frontend para melhor UX.

## 🎨 Interface do Usuário

### Checkbox
```
⚠️ Cargo Exato (Exceção)

Padrão (desmarcado): Usa match parcial. 
Ex: "Gerente" incluirá "Gerente I", "Gerente II", etc.

Exceção (marcado): Apenas o cargo exato. 
Ex: "Presidente" incluirá SOMENTE "Presidente".
```

### Badges nos Cards

**Match Padrão (maioria):**
```
[🔍 Padrão] - Badge azul/verde
```

**Match Exato (exceção):**
```
[✓ Exato] - Badge cinza
```

## 💡 Exemplos de Uso

### Caso 1: Padrão (99% dos casos)
```
Cargo: Gerente
Match Exato: ☐ (desmarcado)
Comportamento: Pega "Gerente I", "Gerente II", "Gerente Regional"
```

### Caso 2: Padrão
```
Cargo: Diretor
Match Exato: ☐ (desmarcado)
Comportamento: Pega "Diretor Financeiro", "Diretor de TI", etc.
```

### Caso 3: Exceção (1% dos casos)
```
Cargo: Presidente
Match Exato: ☑ (marcado)
Comportamento: Pega APENAS "Presidente" (exato)
```

## 🔍 Lógica de Verificação (Backend)

Prioridade ao verificar cargo:

1. **Match Exato** (`usarpadrao = false`)
   - Compara cargo == cargoConfigurado
   
2. **Match Padrão** (`usarpadrao = true`)
   - Compara cargo.Contains(cargoConfigurado)

## ✅ Benefícios desta Abordagem

1. **Comportamento padrão intuitivo** - Match parcial faz sentido com entrada livre
2. **Menos cliques** - Maioria dos casos não precisa marcar nada
3. **Visual claro** - Checkbox de "exceção" indica caso especial
4. **Flexibilidade** - Ainda permite match exato quando necessário

## 📝 Mapeamento Frontend ↔ Backend

| Frontend (UX)      | Backend (DB)     | Comportamento           |
|--------------------|------------------|-------------------------|
| matchexato = false | usarpadrao = true | Match Parcial (Padrão) |
| matchexato = true  | usarpadrao = false | Match Exato (Exceção)  |

## 🎨 Fluxo do Usuário

### Cenário Comum (Padrão)
1. Digite: `Gerente`
2. Deixe desmarcado (padrão)
3. Salve
4. ✅ Pega todos os gerentes automaticamente

### Cenário Raro (Exceção)
1. Digite: `Presidente`
2. Marque ☑ "Cargo Exato"
3. Salve
4. ✅ Pega apenas "Presidente" exato

## 🔧 Código de Conversão

```typescript
// Ao adicionar/editar cargo
const dados = {
  cargo: formValue.cargo,
  usarpadrao: !formValue.matchexato,  // Inverte a lógica
  // ... outros campos
};

// Ao carregar cargo para edição
this.form.patchValue({
  cargo: cargo.cargo,
  matchexato: !cargo.usarpadrao,  // Inverte de volta
  // ... outros campos
});
```

## 🎯 Conclusão

Esta abordagem torna a interface mais intuitiva:
- **Default = Match Padrão** (o que 99% dos usuários querem)
- **Exceção = Match Exato** (marcado apenas quando necessário)
- Menos configuração, mais produtividade
- Interface auto-explicativa com warnings visuais (⚠️) para exceções

