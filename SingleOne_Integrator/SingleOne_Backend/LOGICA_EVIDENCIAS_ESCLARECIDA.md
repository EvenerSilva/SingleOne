# 📸 Lógica de Evidências - Esclarecimento

## 🎯 ENTENDIMENTO CORRETO

### ❌ ANTES (Interpretação Incorreta)
"Evidências" era um checkbox separado, como mais um processo.

### ✅ AGORA (Interpretação Correta)
**"Obrigar Evidências" significa:**  
"Exigir evidências FOTOGRÁFICAS de TODOS os processos obrigatórios"

---

## 📊 EXEMPLOS PRÁTICOS

### Cenário 1: ANALISTA (SEM Evidências)
```
Cadastro no sistema:
  Cargo: ANALISTA
  ✅ Sanitização
  ✅ Descaracterização
  ❌ Perfuração de Disco
  ❌ Obrigar Evidências

Na tela de descarte:
  ┌─────────────────────────────────────┐
  │ Processos Obrigatórios              │
  ├─────────────────────────────────────┤
  │ ☐ Sanitização                       │
  │ ☐ Descaracterização                 │
  └─────────────────────────────────────┘

Usuário marca:
  ✅ Sanitização
  ✅ Descaracterização
  
Descarte permitido: SIM ✅
```

### Cenário 2: DIRETOR (COM Evidências)
```
Cadastro no sistema:
  Cargo: DIRETOR
  ✅ Sanitização
  ✅ Descaracterização
  ✅ Perfuração de Disco
  ✅ Obrigar Evidências ← IMPORTANTE!

Na tela de descarte:
  ┌─────────────────────────────────────┐
  │ Processos Obrigatórios              │
  ├─────────────────────────────────────┤
  │ 📸 Evidências Fotográficas Obrig.   │
  │                                     │
  │ 📸 Sanitização        (0 fotos)     │
  │ 📸 Descaracterização  (0 fotos)     │
  │ 📸 Perfuração Disco   (0 fotos)     │
  │                                     │
  │ [📷 Gerenciar Evidências]           │
  └─────────────────────────────────────┘

Usuário clica em "Gerenciar Evidências":
  1. Modal abre
  2. Seleciona tipo: "Sanitização"
  3. Anexa 2 fotos do processo
  4. Seleciona tipo: "Descaracterização"
  5. Anexa 1 foto
  6. Seleciona tipo: "Perfuração Disco"
  7. Anexa 3 fotos

Resultado:
  ┌─────────────────────────────────────┐
  │ 📸 Sanitização        (2 fotos) ✅  │
  │ 📸 Descaracterização  (1 foto)  ✅  │
  │ 📸 Perfuração Disco   (3 fotos) ✅  │
  │                                     │
  │ [📷 Gerenciar Evidências] (6)       │
  └─────────────────────────────────────┘

Descarte permitido: SIM ✅
```

### Cenário 3: CEO (COM Evidências mas INCOMPLETO)
```
Cadastro no sistema:
  Cargo: CEO
  ✅ Sanitização
  ✅ Descaracterização
  ✅ Perfuração de Disco
  ✅ Obrigar Evidências

Usuário anexa evidências:
  ✅ Sanitização (2 fotos)
  ✅ Descaracterização (1 foto)
  ❌ Perfuração Disco (0 fotos) ← FALTANDO!

Resultado:
  ┌─────────────────────────────────────┐
  │ 📸 Sanitização        (2 fotos) ✅  │
  │ 📸 Descaracterização  (1 foto)  ✅  │
  │ 📸 Perfuração Disco   (0 fotos) ❌  │
  │                                     │
  │ [📷 Gerenciar Evidências] (3)       │
  └─────────────────────────────────────┘

Tenta realizar descarte:
  ❌ BLOQUEADO
  ❌ Mensagem: "Evidência de Perfuração de Disco (foto/arquivo)"
```

---

## 🔄 FLUXO DE VALIDAÇÃO

### Backend - `EquipamentoNegocio.RealizarDescarte()`

```csharp
if (!dsc.ObrigarEvidencias)
{
    // Validação SIMPLES - apenas checkboxes
    if (dsc.ObrigarSanitizacao && !dsc.SanitizacaoExecutada)
        erro: "Sanitização não marcada";
}
else
{
    // Validação FOTOGRÁFICA - verificar evidências de CADA processo
    var evidencias = buscarEvidenciasDoEquipamento();
    
    if (dsc.ObrigarSanitizacao)
    {
        var temFotoSanitizacao = evidencias.Any(e => e.Tipoprocesso == "SANITIZACAO");
        if (!temFotoSanitizacao)
            erro: "Evidência de Sanitização (foto/arquivo)";
    }
    
    if (dsc.ObrigarDescaracterizacao)
    {
        var temFotoDescaracterizacao = evidencias.Any(e => e.Tipoprocesso == "DESCARACTERIZACAO");
        if (!temFotoDescaracterizacao)
            erro: "Evidência de Descaracterização (foto/arquivo)";
    }
    
    if (dsc.ObrigarPerfuracaoDisco)
    {
        var temFotoPerfuracao = evidencias.Any(e => e.Tipoprocesso == "PERFURACAO_DISCO");
        if (!temFotoPerfuracao)
            erro: "Evidência de Perfuração de Disco (foto/arquivo)";
    }
}
```

### Frontend - `descarte.component.ts.realizarDescarte()`

```typescript
if (!dsc.obrigarEvidencias)
{
    // Validação SIMPLES
    if (dsc.obrigarSanitizacao && !dsc.sanitizacaoExecutada)
        erro.push('Sanitização');
}
else
{
    // Validação FOTOGRÁFICA
    if (dsc.obrigarSanitizacao && getCountEvidencias(dsc, 'SANITIZACAO') === 0)
        erro.push('Evidência de Sanitização (foto/arquivo)');
    
    if (dsc.obrigarDescaracterizacao && getCountEvidencias(dsc, 'DESCARACTERIZACAO') === 0)
        erro.push('Evidência de Descaracterização (foto/arquivo)');
    
    if (dsc.obrigarPerfuracaoDisco && getCountEvidencias(dsc, 'PERFURACAO_DISCO') === 0)
        erro.push('Evidência de Perfuração de Disco (foto/arquivo)');
}
```

---

## 💡 INTERFACE DO USUÁRIO

### Quando NÃO exige evidências (ANALISTA)
```
┌─────────────────────────────────────┐
│ Processos Obrigatórios              │
├─────────────────────────────────────┤
│ ☑ Sanitização                       │
│ ☑ Descaracterização                 │
│                                     │
│ ℹ️ Cargos: ANALISTA                 │
└─────────────────────────────────────┘
```
**Usuário:** Apenas marca os checkboxes

### Quando EXIGE evidências (DIRETOR)
```
┌─────────────────────────────────────┐
│ 📸 Evidências Fotográficas Obrig.   │
├─────────────────────────────────────┤
│ 📸 Sanitização        (2 fotos)     │
│ 📸 Descaracterização  (1 foto)      │
│ 📸 Perfuração Disco   (3 fotos)     │
│                                     │
│ [📷 Gerenciar Evidências] (6)       │
│                                     │
│ ℹ️ Cargos: DIRETOR                  │
└─────────────────────────────────────┘
```
**Usuário:** Clica em "Gerenciar Evidências" e anexa fotos

---

## 🔒 TIPOS DE PROCESSOS NO MODAL

Quando o usuário abre o modal, pode selecionar:

| Tipo | Código Backend | Quando Usar |
|------|----------------|-------------|
| Sanitização | `SANITIZACAO` | Fotos do processo de sanitização |
| Descaracterização | `DESCARACTERIZACAO` | Fotos da descaracterização |
| Perfuração de Disco | `PERFURACAO_DISCO` | Fotos da perfuração física |
| Evidências Gerais | `EVIDENCIAS_GERAIS` | Outras evidências do descarte |

---

## ⚠️ REGRAS IMPORTANTES

### 1. Checkbox "Obrigar Evidências" no Cadastro de Cargos
```
SE marcado: 
  → Usuário DEVE anexar fotos de CADA processo marcado
  → NÃO aparecem checkboxes simples
  → Aparece botão "Gerenciar Evidências"

SE desmarcado:
  → Usuário apenas marca checkboxes
  → NÃO precisa anexar fotos
  → Validação mais simples
```

### 2. Validação é Específica por Processo
```
Não basta anexar "1 evidência geral"
Precisa anexar evidências de CADA TIPO:
  - Pelo menos 1 foto de SANITIZACAO
  - Pelo menos 1 foto de DESCARACTERIZACAO  
  - Pelo menos 1 foto de PERFURACAO_DISCO
```

### 3. Contador Mostra Total Geral
```
Badge (6) = Total de TODAS as evidências
Mas internamente valida se tem de CADA tipo
```

---

## 🧪 TESTES RECOMENDADOS

### Teste 1: Cargo sem Evidências
```
1. Cadastrar cargo "ANALISTA":
   - Sanitização: ✅
   - Descaracterização: ✅
   - Evidências: ❌

2. Descarte:
   - Marcar checkboxes
   - Realizar descarte
   ✅ Sucesso!
```

### Teste 2: Cargo com Evidências Completo
```
1. Cadastrar cargo "DIRETOR":
   - Sanitização: ✅
   - Descaracterização: ✅
   - Perfuração: ✅
   - Evidências: ✅

2. Descarte:
   - Anexar 1 foto de Sanitização
   - Anexar 1 foto de Descaracterização
   - Anexar 1 foto de Perfuração
   - Realizar descarte
   ✅ Sucesso!
```

### Teste 3: Cargo com Evidências Incompleto
```
1. Cadastrar cargo "CEO":
   - Sanitização: ✅
   - Descaracterização: ✅
   - Perfuração: ✅
   - Evidências: ✅

2. Descarte:
   - Anexar 2 fotos de Sanitização
   - Anexar 1 foto de Descaracterização
   - NÃO anexar fotos de Perfuração
   - Tentar realizar descarte
   ❌ Bloqueado: "Evidência de Perfuração de Disco (foto/arquivo)"
```

---

## ✨ RESUMO

**"Obrigar Evidências" = "Exigir comprovação fotográfica de CADA processo"**

- Sem evidências → Apenas confirmar verbalmente (checkbox)
- Com evidências → Provar com fotos de cada etapa

**Desenvolvido com clareza! 🎉**

---

**Data:** 03/10/2025  
**Versão:** Final

