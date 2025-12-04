# Melhoria: Cargos de Confiança com Padrão de Busca

## 📋 Resumo

Implementação de funcionalidade que permite criar regras de Cargos de Confiança baseadas em **padrões de texto**, ao invés de apenas match exato. Isso permite criar uma única regra para múltiplos cargos similares.

### Exemplo Prático
**Antes:** Era necessário criar 3 regras separadas:
- "Gerente I"
- "Gerente II"  
- "Gerente III"

**Agora:** Uma única regra com padrão:
- "Gerente" ✓ (com checkbox "Usar Padrão" marcado)
  - Automaticamente inclui: "Gerente I", "Gerente II", "Gerente III", "Gerente Regional", etc.

## 🎯 Benefícios

1. **Menos configurações** - Uma regra cobre múltiplos cargos
2. **Manutenção facilitada** - Novos cargos similares são automaticamente incluídos
3. **Flexibilidade** - Pode usar tanto match exato quanto padrão
4. **Entrada livre** - Usuário pode digitar qualquer cargo, não apenas selecionar

## ✨ Alterações Implementadas

### 🗄️ Banco de Dados

**Nova Coluna:** `usarpadrao` (BOOLEAN)
- `false` = Match exato (comportamento anterior)
- `true` = Match por padrão (contém o texto)

```sql
ALTER TABLE cargosconfianca 
ADD COLUMN usarpadrao BOOLEAN DEFAULT false;
```

### 🔧 Backend (C#)

#### 1. Model (`CargoConfianca.cs`)
```csharp
public bool Usarpadrao { get; set; }  // Novo campo
```

#### 2. Mapeamento EF Core (`CargoConfiancaMap.cs`)
```csharp
builder.Property(e => e.Usarpadrao)
    .HasColumnName("usarpadrao")
    .HasDefaultValue(false);
```

#### 3. Novo Método de Verificação (`ColaboradorNegocio.cs`)
```csharp
public CargoConfianca VerificarCargoConfianca(string cargo, int cliente)
{
    // 1. Verifica match exato primeiro
    // 2. Se não encontrar, verifica padrões (LIKE)
    // 3. Retorna o cargo de confiança correspondente ou null
}
```

#### 4. Novo Endpoint (`ColaboradorController.cs`)
```csharp
GET /api/Colaborador/cargosconfianca/Verificar/{cargo}/{cliente}
```

### 🎨 Frontend (Angular)

#### 1. Componente TypeScript

**Novo Campo no Form:**
```typescript
usarpadrao: [false]
```

**Métodos Atualizados:**
- `adicionarCargo()` - Inclui novo campo
- `editarCargo()` - Carrega valor de usarpadrao
- `cancelarEdicao()` - Reseta usarpadrao

**Simplificações:**
- ✅ Removida listagem de cargos existentes (`cargosExistentes`)
- ✅ Removida chamada a `listarCargosUnicos()`
- ✅ Entrada totalmente livre para o usuário

#### 2. Template HTML

**Input de Texto Livre:**
```html
<input matInput formControlName="cargo" required 
       placeholder="Ex: Gerente, Diretor, Presidente, etc.">
<mat-hint>Digite o nome do cargo ou um padrão para agrupar cargos similares</mat-hint>
```

**Novo Checkbox:**
```html
<mat-checkbox formControlName="usarpadrao">
  <strong>Usar Padrão (Match Parcial)</strong>
  <span>Ex: "Gerente" incluirá "Gerente I", "Gerente II", etc.</span>
</mat-checkbox>
```

**Badge Visual nos Cards:**
```html
<span class="badge-padrao" *ngIf="cargo.usarpadrao">
  <i class="material-icons">search</i> Padrão
</span>
```

#### 3. Estilos (SCSS)

- Seção destacada para o checkbox de padrão
- Badge visual para indicar cargos com padrão
- Design responsivo e moderno

#### 4. Service (`colaborador-api.service.ts`)

**Novo Método:**
```typescript
verificarCargoConfianca(cargo, cliente, token)
```

**URLs Corrigidas:**
- Todos os endpoints agora usam prefixo correto: `/colaborador/cargosconfianca/`

## 📝 Como Usar

### 1. Aplicar Mudanças no Banco

```powershell
cd C:\SingleOne\SingleOne_Backend
.\aplicar-campo-usarpadrao.ps1
```

### 2. Compilar Backend

```powershell
dotnet build
```

### 3. Executar Backend

```powershell
.\run-backend.ps1
```

### 4. Usar no Frontend

1. Acesse **Cadastros > Cargos de Confiança**
2. No campo **Cargo ou Padrão de Cargo**:
   - Digite livremente qualquer cargo (ex: "Gerente", "Diretor")
   - Baseie-se no seu conhecimento da organização
3. Marque **"Usar Padrão"** se quiser match parcial
4. Configure os processos obrigatórios
5. Salve

## 💡 Exemplos de Uso

### Exemplo 1: Gerentes
**Configuração:**
- Cargo: `Gerente`
- Usar Padrão: ✓ Sim
- Nível: ALTO

**Vai pegar:**
- Gerente I
- Gerente II  
- Gerente III
- Gerente Regional
- Gerente de Vendas
- Sub-Gerente

### Exemplo 2: Diretores
**Configuração:**
- Cargo: `Diretor`
- Usar Padrão: ✓ Sim
- Nível: ALTO

**Vai pegar:**
- Diretor Executivo
- Diretor Financeiro
- Diretor de TI
- Diretor Regional

### Exemplo 3: Cargo Específico
**Configuração:**
- Cargo: `Presidente`
- Usar Padrão: ✗ Não
- Nível: ALTO

**Vai pegar:**
- Apenas "Presidente" (match exato)

## 🔍 Lógica de Verificação

Quando o sistema precisa verificar se um cargo é de confiança:

1. **Prioridade 1:** Match Exato
   - Busca cargos com `usarpadrao = false`
   - Compara cargo.ToUpper() == cargoConfigurado.ToUpper()

2. **Prioridade 2:** Match por Padrão
   - Busca cargos com `usarpadrao = true`
   - Verifica se cargo.ToUpper().Contains(cargoConfigurado.ToUpper())

3. **Retorno:** Primeiro cargo encontrado ou null

## 📊 Estrutura Completa da Tabela

```sql
cargosconfianca
├── id (SERIAL PRIMARY KEY)
├── cliente (INTEGER NOT NULL)
├── cargo (VARCHAR(200) NOT NULL)
├── usarpadrao (BOOLEAN DEFAULT false)      ⬅️ NOVO
├── nivelcriticidade (VARCHAR(20))
├── obrigarsanitizacao (BOOLEAN)
├── obrigardescaracterizacao (BOOLEAN)
├── obrigarperfuracaodisco (BOOLEAN)
├── obrigarevidencias (BOOLEAN)
├── ativo (BOOLEAN DEFAULT true)
├── usuariocriacao (INTEGER NOT NULL)
├── datacriacao (TIMESTAMP)
├── usuarioalteracao (INTEGER)
└── dataalteracao (TIMESTAMP)
```

## 🎯 Endpoints API

### Listar Cargos de Confiança Configurados
```
GET /api/Colaborador/cargosconfianca/Listar/{cliente}
```

### Salvar Cargo de Confiança
```
POST /api/Colaborador/cargosconfianca/Salvar
Body: {
  "cliente": 2,
  "cargo": "Gerente",
  "usarpadrao": true,
  "nivelcriticidade": "ALTO",
  "obrigarsanitizacao": true,
  "obrigardescaracterizacao": true,
  "obrigarperfuracaodisco": true,
  "obrigarevidencias": true,
  "usuariocriacao": 1
}
```

### Atualizar Cargo de Confiança
```
PUT /api/Colaborador/cargosconfianca/Atualizar/{id}
Body: { ... }
```

### Excluir Cargo de Confiança
```
DELETE /api/Colaborador/cargosconfianca/Excluir/{id}
```

### Verificar se Cargo é de Confiança (NOVO)
```
GET /api/Colaborador/cargosconfianca/Verificar/{cargo}/{cliente}
```

## ✅ Testes Sugeridos

### Teste 1: Cadastrar com Padrão
1. Cadastrar cargo "Gerente" com `usarpadrao = true`
2. Verificar se pega "Gerente I", "Gerente II", etc.

### Teste 2: Cadastrar sem Padrão
1. Cadastrar cargo "Presidente" com `usarpadrao = false`
2. Verificar se pega apenas "Presidente" exato

### Teste 3: Prioridade
1. Cadastrar "Gerente Regional" com `usarpadrao = false` (MÉDIO)
2. Cadastrar "Gerente" com `usarpadrao = true` (ALTO)
3. Verificar qual regra é aplicada para "Gerente Regional"
   - Esperado: MÉDIO (match exato tem prioridade)

### Teste 4: Interface
1. Testar input com autocomplete
2. Testar checkbox de padrão
3. Verificar badge visual nos cards
4. Testar edição de cargo existente

## 🎨 Interface Visual

### Formulário
- ✅ Input de texto livre com autocomplete
- ✅ Lista de cargos existentes como sugestão
- ✅ Checkbox destacado para "Usar Padrão"
- ✅ Hint explicativo do comportamento

### Cards de Cargos
- ✅ Badge "Padrão" visível nos cargos configurados
- ✅ Tooltip explicativo
- ✅ Design moderno e responsivo

## 📚 Arquivos Modificados

### Backend
- ✅ `SingleOneAPI/Models/CargoConfianca.cs`
- ✅ `SingleOneAPI/Infra/Mapeamento/CargoConfiancaMap.cs`
- ✅ `SingleOneAPI/Negocios/ColaboradorNegocio.cs`
- ✅ `SingleOneAPI/Negocios/Interfaces/IColaboradorNegocio.cs`
- ✅ `SingleOneAPI/Controllers/ColaboradorController.cs`

### Frontend
- ✅ `src/app/pages/parametros/cargosconfianca/cargosconfianca.component.ts`
- ✅ `src/app/pages/parametros/cargosconfianca/cargosconfianca.component.html`
- ✅ `src/app/pages/parametros/cargosconfianca/cargosconfianca.component.scss`
- ✅ `src/app/api/colaboradores/colaborador-api.service.ts`

### Scripts
- ✅ `adicionar-campo-usarpadrao-cargosconfianca.sql`
- ✅ `aplicar-campo-usarpadrao.ps1`

## 🚀 Próximas Melhorias Sugeridas

1. **Relatório de Cobertura**
   - Mostrar quantos colaboradores cada regra de cargo abrange

2. **Validação de Conflitos**
   - Alertar se há regras conflitantes (ex: match exato + padrão no mesmo cargo)

3. **Preview em Tempo Real**
   - Ao digitar o padrão, mostrar quais cargos seriam incluídos

4. **Regex Avançado**
   - Suportar expressões regulares para padrões mais complexos

5. **Integração com Descarte**
   - Aplicar automaticamente as regras no processo de descarte

## 📞 Suporte

Se encontrar algum problema:
1. Verificar logs do backend
2. Verificar console do navegador (F12)
3. Conferir se o campo `usarpadrao` existe no banco
4. Verificar se as URLs dos endpoints estão corretas

## ✨ Conclusão

Esta melhoria torna o sistema de Cargos de Confiança muito mais **flexível e prático**, reduzindo significativamente o trabalho de configuração e manutenção das regras de segurança para descartes.

