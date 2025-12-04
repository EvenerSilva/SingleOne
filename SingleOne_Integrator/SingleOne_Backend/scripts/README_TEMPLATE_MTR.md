# 📋 Atualização do Template de Descarte com MTR

## 🎯 Objetivo

Atualizar o **Template ID 5** no banco de dados para incluir a seção **MTR (Manifesto de Transporte de Resíduos)** no PDF gerado pelos protocolos de descarte.

## ❓ Problema Identificado

O template de descarte (ID 5) no banco de dados **não continha as variáveis do MTR**, resultando em PDFs sem as informações do Manifesto de Transporte de Resíduos, mesmo quando essas informações estavam cadastradas no banco.

### Como funcionava:

1. Código buscava o template ID 5 do banco de dados
2. Verificava se o template tinha as variáveis `{{MTR_NUMERO}}` e `{{LISTA_EQUIPAMENTOS}}`
3. Como o template do banco **não tinha essas variáveis**, o código usava um **template padrão embutido** (método `ObterTemplatePadrao()`)
4. Por isso o MTR aparecia no PDF, mas usando um template "genérico" e não o template oficial do banco

## ✅ Solução Implementada

Atualizamos o arquivo `insert_template_descarte.sql` para incluir a seção completa do MTR, com todas as variáveis necessárias:

### Variáveis do MTR Adicionadas:

- `{{MTR_OBRIGATORIO}}` - Se MTR é obrigatório (Sim/Não)
- `{{MTR_NUMERO}}` - Número do MTR
- `{{MTR_EMITIDO_POR}}` - Quem emitiu (Gerador/Transportador/Destinador)
- `{{MTR_DATA_EMISSAO}}` - Data de emissão do MTR
- `{{MTR_VALIDADE}}` - Data de validade do MTR
- `{{MTR_DADOS_TRANSPORTADORA}}` - Seção dinâmica com dados da transportadora (aparece apenas quando MTR foi emitido pelo transportador)

### Seção MTR no Template:

```html
<div class="section" style="background: #fff3cd; border-left-color: #ffc107;">
    <h2 style="color: #856404;">📋 MTR - Manifesto de Transporte de Resíduos</h2>
    <div class="info-grid">
        <div class="info-item"><span class="label">MTR Obrigatório:</span> <span class="value">{{MTR_OBRIGATORIO}}</span></div>
        <div class="info-item"><span class="label">Número do MTR:</span> <span class="value">{{MTR_NUMERO}}</span></div>
        <div class="info-item"><span class="label">Emitido Por:</span> <span class="value">{{MTR_EMITIDO_POR}}</span></div>
        <div class="info-item"><span class="label">Data de Emissão:</span> <span class="value">{{MTR_DATA_EMISSAO}}</span></div>
        <div class="info-item"><span class="label">Validade do MTR:</span> <span class="value">{{MTR_VALIDADE}}</span></div>
    </div>
    {{MTR_DADOS_TRANSPORTADORA}}
    <p style="font-size: 12px; color: #856404; margin-top: 15px; font-style: italic;">
        O MTR (Manifesto de Transporte de Resíduos) é obrigatório conforme Resolução CONAMA nº 313/2002
    </p>
</div>
```

## 🚀 Como Aplicar a Atualização

### Opção 1: Usar o Script PowerShell (Recomendado)

```powershell
cd C:\SingleOne\SingleOne_Backend\scripts
.\atualizar_template_descarte_com_mtr.ps1
```

O script irá solicitar:
- Servidor do banco de dados (ex: localhost)
- Nome do banco de dados (ex: singleone)
- Usuário do banco de dados
- Senha do banco de dados

### Opção 2: Executar manualmente o SQL

```bash
mysql -h localhost -u seu_usuario -p singleone < insert_template_descarte.sql
```

### Opção 3: Executar pelo cliente MySQL

1. Abra o MySQL Workbench ou outro cliente
2. Conecte ao banco de dados
3. Abra o arquivo `insert_template_descarte.sql`
4. Execute o script

## 🧪 Verificar se Funcionou

Execute a seguinte query no banco:

```sql
SELECT 
    id, 
    titulo, 
    CASE 
        WHEN conteudo LIKE '%MTR_NUMERO%' THEN 'SIM ✅' 
        ELSE 'NÃO ❌' 
    END as tem_campos_mtr
FROM templates 
WHERE id = 5;
```

Deve retornar:
```
id | titulo                               | tem_campos_mtr
5  | Template de Descarte de Equipamentos | SIM ✅
```

## 📊 Resultado Final

Após a atualização, o PDF gerado pelos protocolos de descarte irá:

1. ✅ Usar o **template oficial do banco de dados** (ID 5)
2. ✅ Exibir **todas as informações do MTR** cadastradas
3. ✅ Mostrar **dados da transportadora** (quando MTR foi emitido pelo transportador)
4. ✅ Manter **consistência visual** com o layout oficial do sistema

## 📝 Arquivos Modificados

- ✅ `scripts/insert_template_descarte.sql` - Template atualizado com seção MTR
- ✅ `scripts/atualizar_template_descarte_com_mtr.ps1` - Script para facilitar execução
- ✅ `scripts/README_TEMPLATE_MTR.md` - Esta documentação

## 🔍 Código de Geração do PDF

O código que gera o PDF está em:
```
SingleOneAPI\Negocios\ProtocoloDescarteNegocio.cs
Método: GerarDocumentoDescarte()
Linhas: ~946-1309
```

### Fluxo de Geração:

1. Busca template ID 5 do banco
2. Verifica se tem as variáveis `{{MTR_NUMERO}}` e `{{LISTA_EQUIPAMENTOS}}`
3. ✅ **ANTES**: Template do banco não tinha → usava template embutido
4. ✅ **AGORA**: Template do banco tem → usa template oficial do banco
5. Substitui todas as variáveis com dados do protocolo
6. Gera PDF usando IronPDF

## 📞 Suporte

Se tiver algum problema na atualização:

1. Verifique se o template ID 5 existe no banco
2. Verifique se o script SQL foi executado sem erros
3. Execute a query de verificação acima
4. Teste gerando um novo PDF de protocolo de descarte

---

**Data de Atualização**: 09/10/2025  
**Versão do Template**: 2.0 (com MTR)

