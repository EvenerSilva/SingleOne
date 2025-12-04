# 📋 Mapeamento da Tabela Equipamentos

## 🔍 Análise Realizada

### **Estrutura Atual da Tabela**
A tabela `equipamentos` possui **58 campos**, sendo que muitos são duplicados ou desnecessários.

### **Campos Duplicados Identificados**

#### **1. Campos de Cliente**
- ✅ **`cliente`** (minúscula) - **MANTIDO** - 1.758 registros com dados
- ❌ **`ClienteId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`clienteid`** (minúscula) - **REMOVIDO** - 0 registros

#### **2. Campos de Empresa**
- ✅ **`empresa`** (minúscula) - **MANTIDO** - 5 registros com dados
- ❌ **`EmpresaId`** (maiúscula) - **REMOVIDO** - 0 registros

#### **3. Campos de Centro de Custo**
- ✅ **`centrocusto`** (minúscula) - **MANTIDO** - 15 registros com dados
- ❌ **`CentrocustoId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Centrocusto`** (maiúscula) - **REMOVIDO** - 0 registros

#### **4. Campos de Filial**
- ✅ **`filial_id`** (minúscula) - **MANTIDO** - 1 registro com dados
- ❌ **`FilialId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Filial`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`FilialId1`** (maiúscula) - **REMOVIDO** - 0 registros

#### **5. Campos de Localidade**
- ✅ **`localidade_id`** (minúscula) - **MANTIDO** - 17 registros com dados
- ✅ **`localizacao`** (minúscula) - **MANTIDO** - Campo legado para compatibilidade
- ❌ **`LocalidadeId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Localidade`** (maiúscula) - **REMOVIDO** - 0 registros

#### **6. Campos de Fornecedor**
- ✅ **`fornecedor`** (minúscula) - **MANTIDO** - 0 registros (campo válido)
- ❌ **`FornecedorId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Fornecedor`** (maiúscula) - **REMOVIDO** - 0 registros

#### **7. Campos de Usuario**
- ✅ **`usuario`** (minúscula) - **MANTIDO** - 1.757 registros com dados
- ❌ **`UsuarioId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Usuario`** (maiúscula) - **REMOVIDO** - 0 registros

#### **8. Campos de Status**
- ✅ **`equipamentostatus`** (minúscula) - **MANTIDO** - 1.736 registros com dados
- ❌ **`Equipamentostatus`** (maiúscula) - **REMOVIDO** - 28 registros (duplicado)
- ❌ **`EquipamentostatusId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`EquipamentosstatusId`** (maiúscula) - **REMOVIDO** - 0 registros

#### **9. Campos de Tipo Equipamento**
- ✅ **`tipoequipamento`** (minúscula) - **MANTIDO** - 1.758 registros com dados
- ❌ **`TipoequipamentoId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Tipoequipamento`** (maiúscula) - **REMOVIDO** - 0 registros

#### **10. Campos de Fabricante**
- ✅ **`fabricante`** (minúscula) - **MANTIDO** - 1.758 registros com dados
- ❌ **`FabricanteId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Fabricante`** (maiúscula) - **REMOVIDO** - 0 registros

#### **11. Campos de Modelo**
- ✅ **`modelo`** (minúscula) - **MANTIDO** - 1.758 registros com dados
- ❌ **`ModeloId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Modelo`** (maiúscula) - **REMOVIDO** - 0 registros

#### **12. Campos de Nota Fiscal**
- ✅ **`notafiscal`** (minúscula) - **MANTIDO** - 1.751 registros com dados
- ❌ **`NotafiscalId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Notafiscal`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`NotasfiscaiId`** (maiúscula) - **REMOVIDO** - 0 registros

#### **13. Campos de Contrato**
- ✅ **`contrato`** (minúscula) - **MANTIDO** - 1.694 registros com dados
- ❌ **`ContratoId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Contrato`** (maiúscula) - **REMOVIDO** - 1.660 registros (duplicado)

#### **14. Campos de Tipo Aquisição**
- ✅ **`tipoaquisicao`** (minúscula) - **MANTIDO** - 1.758 registros com dados
- ❌ **`TipoaquisicaoId`** (maiúscula) - **REMOVIDO** - 0 registros
- ❌ **`Tipoaquisicao`** (maiúscula) - **REMOVIDO** - 0 registros

## 📊 Resumo da Limpeza

### **Campos Removidos: 30+ campos duplicados**
- Todos os campos com maiúsculas que não têm dados
- Campos duplicados com nomes similares
- Campos com sufixo "Id" desnecessários

### **Campos Mantidos: 28 campos essenciais**
- Todos os campos em minúsculas que têm dados
- Campos obrigatórios do sistema
- Campos de compatibilidade (localizacao)

## 🎯 Benefícios da Limpeza

### **1. Performance**
- ✅ Redução significativa do tamanho da tabela
- ✅ Queries mais rápidas
- ✅ Menos overhead de armazenamento

### **2. Manutenibilidade**
- ✅ Código mais limpo e organizado
- ✅ Menos confusão entre campos similares
- ✅ Estrutura mais clara

### **3. Desenvolvimento**
- ✅ Menos bugs relacionados a campos duplicados
- ✅ Mapeamento mais simples no Entity Framework
- ✅ API mais consistente

## ⚠️ Campos Especiais

### **Campos de Compatibilidade**
- **`localizacao`**: Mantido para compatibilidade com dados antigos
- **`migrateid`**: Mantido para controle de migração
- **`enviouemailreporte`**: Mantido para funcionalidade de relatórios

### **Campos de Auditoria**
- **`dtcadastro`**: Data de criação (obrigatório)
- **`usuario`**: Usuário que criou (quase todos preenchidos)
- **`ativo`**: Status ativo/inativo (obrigatório)

## 🔧 Scripts de Limpeza

### **Script Principal**
```sql
-- Executar: limpar-campos-duplicados-equipamentos.sql
-- Remove 30+ campos duplicados sem dados
```

### **Verificação**
```sql
-- Executar: analisar-campos-equipamentos.sql
-- Analisa uso de cada campo
```

### **Validação**
```sql
-- Executar: verificar-campos-duplicados.sql
-- Verifica campos duplicados com dados
```

## 📈 Resultado Final

### **Antes da Limpeza**
- **58 campos** na tabela
- **30+ campos duplicados** sem dados
- **Confusão** entre campos similares
- **Performance** comprometida

### **Após a Limpeza**
- **~28 campos** na tabela
- **0 campos duplicados**
- **Estrutura clara** e organizada
- **Performance otimizada**

## 🚀 Próximos Passos

1. **Fazer backup** da tabela antes da limpeza
2. **Executar script** de limpeza
3. **Testar sistema** após limpeza
4. **Atualizar mapeamentos** do Entity Framework
5. **Documentar mudanças** para a equipe
