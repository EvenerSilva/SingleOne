# 📎 Plano de Implementação: Anexo de Arquivos em Notas Fiscais

## 📋 Análise da Situação Atual

### ✅ Estrutura Existente:
- **Tabela**: `notasfiscais` no PostgreSQL
- **Model**: `Notasfiscai.cs`
- **Frontend**: `notas-fiscais.component.ts/html/scss`
- **Colunas atuais**: id, cliente, fornecedor, numero, dtemissao, descricao, valor, contrato, virtual, gerouequipamento

### ❌ Faltando:
- Campos para armazenar arquivo anexado
- Endpoints de upload/download/remoção
- Interface para gerenciar anexos

---

## 🎯 O QUE PRECISA SER IMPLEMENTADO

### 1️⃣ **BANCO DE DADOS**

#### Novos Campos na Tabela `notasfiscais`:
```sql
ALTER TABLE notasfiscais 
ADD COLUMN arquivonotafiscal VARCHAR(500) NULL,
ADD COLUMN nomearquivooriginal VARCHAR(255) NULL,
ADD COLUMN datauploadarquivo TIMESTAMP NULL,
ADD COLUMN usuariouploadarquivo INT NULL,
ADD COLUMN usuarioremocaoarquivo INT NULL,
ADD COLUMN dataremocaoarquivo TIMESTAMP NULL;

-- Foreign Keys
ALTER TABLE notasfiscais 
ADD CONSTRAINT fk_notasfiscais_usuarioupload 
FOREIGN KEY (usuariouploadarquivo) REFERENCES usuarios(id) ON DELETE SET NULL;

ALTER TABLE notasfiscais 
ADD CONSTRAINT fk_notasfiscais_usuarioremocao 
FOREIGN KEY (usuarioremocaoarquivo) REFERENCES usuarios(id) ON DELETE SET NULL;
```

---

### 2️⃣ **BACKEND (.NET)**

#### Arquivos a Atualizar:

**A) Model: `Notasfiscai.cs`**
```csharp
public string ArquivoNotaFiscal { get; set; }
public string NomeArquivoOriginal { get; set; }
public DateTime? DataUploadArquivo { get; set; }
public int? UsuarioUploadArquivo { get; set; }
public int? UsuarioRemocaoArquivo { get; set; }
public DateTime? DataRemocaoArquivo { get; set; }

public virtual Usuario UsuarioUploadArquivoNavigation { get; set; }
public virtual Usuario UsuarioRemocaoArquivoNavigation { get; set; }
```

**B) Mapeamento: `NotasfiscaiMap.cs`** (precisa ser criado ou atualizado)
- Mapear os novos campos
- Configurar as foreign keys

**C) DTOs: Criar `NotaFiscalDTO.cs`** ou atualizar ViewModels existentes
```csharp
public bool TemArquivo => !string.IsNullOrEmpty(ArquivoNotaFiscal);
```

**D) Controller: Criar ou atualizar `NotaFiscalController.cs`**
```csharp
[HttpPost("[action]/{notaFiscalId}")]
Task<IActionResult> UploadArquivo(int notaFiscalId, IFormFile arquivo)

[HttpGet("[action]/{notaFiscalId}")]
Task<IActionResult> DownloadArquivo(int notaFiscalId)

[HttpDelete("[action]/{notaFiscalId}")]
IActionResult RemoverArquivo(int notaFiscalId)
```

**E) Service: Criar ou atualizar `NotaFiscalService.cs`**
- `UploadArquivoNotaFiscal()`
- `DownloadArquivoNotaFiscal()`
- `RemoverArquivoNotaFiscal()`

---

### 3️⃣ **FRONTEND (Angular)**

#### Arquivos a Atualizar:

**A) API Service: Criar `nota-fiscal-api.service.ts`** ou atualizar existente
```typescript
uploadArquivo(notaFiscalId: number, arquivo: File)
downloadArquivo(notaFiscalId: number)
removerArquivo(notaFiscalId: number)
```

**B) Componente Lista: `notas-fiscais.component.ts/html`**
- Adicionar coluna "Arquivo" na tabela
- Botões de upload/download/remover
- Métodos para gerenciar anexos

**C) Componente Detalhes/Edição** (se existir)
- Seção de anexos similar ao contrato
- Upload drag-and-drop
- Preview do arquivo

---

## 📁 Estrutura de Armazenamento

```
wwwroot/
└── notasfiscais/
    └── {guid}.pdf
```

---

## 🔒 Validações

### Tipos Permitidos:
- PDF (principal)
- XML (nota fiscal eletrônica)
- JPG/PNG (imagem da nota)

### Tamanho Máximo:
- 10MB por arquivo

---

## 📊 Comparação com Contratos

| Aspecto | Contratos | Notas Fiscais |
|---------|-----------|---------------|
| Tabela | `contratos` | `notasfiscais` |
| Pasta | `wwwroot/contratos/` | `wwwroot/notasfiscais/` |
| Tipos | PDF, DOC, DOCX | PDF, XML, JPG, PNG |
| Model | `Contrato.cs` | `Notasfiscai.cs` |

---

## ✅ Vantagens da Implementação

1. ✅ Nota fiscal anexada diretamente no registro
2. ✅ Não precisa buscar em diretórios externos
3. ✅ Rastreamento completo (quem anexou, quando, quem removeu)
4. ✅ Validação de compliance (todas NFs com arquivo)
5. ✅ Auditoria facilitada

---

## 🚀 Ordem de Implementação Recomendada

1. ✅ Criar script SQL e aplicar no banco
2. ✅ Atualizar Model `Notasfiscai.cs`
3. ✅ Criar/Atualizar Mapeamento EF Core
4. ✅ Atualizar DTOs/ViewModels
5. ✅ Criar/Atualizar Controller com 3 endpoints
6. ✅ Criar/Atualizar Service com lógica de negócio
7. ✅ Atualizar API Service no frontend
8. ✅ Atualizar componente de listagem
9. ✅ Atualizar componente de detalhes (se existir)
10. ✅ Adicionar estilos CSS

---

## ⏱️ Estimativa de Tempo

- **Backend**: ~30 minutos
- **Frontend**: ~20 minutos
- **Testes**: ~10 minutos
- **Total**: ~1 hora

---

## 📝 Observações Importantes

1. **Nota fiscal eletrônica (XML)**: Considere validar se o XML é válido
2. **Múltiplos arquivos**: Se necessário, criar tabela `notasfiscaisanexos` (1-N)
3. **Obrigatoriedade**: Considere tornar o anexo obrigatório para certas situações
4. **Integração**: Possível integração futura com SEFAZ para buscar XML automaticamente

---

**Pronto para implementar?** 🚀

