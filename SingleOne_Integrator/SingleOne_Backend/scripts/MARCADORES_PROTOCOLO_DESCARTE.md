# 📋 Marcadores Disponíveis - Protocolo de Descarte

## 🎯 Template Tipo: Protocolo de Descarte (ID: 5)

Utilize os marcadores abaixo na confecção dos templates de Protocolo de Descarte:

### 📌 Informações do Protocolo

| Marcador | Descrição |
|----------|-----------|
| `@numeroProtocolo` | Número do protocolo de descarte |
| `@cliente` | Nome/Razão Social do cliente |
| `@tipoDescarte` | Tipo de descarte (Doação, Venda, Devolução, Logística Reversa, Descarte Final) |
| `@responsavel` | Nome do responsável pelo protocolo |
| `@dataCriacao` | Data e hora de criação do protocolo (formato: dd/MM/yyyy HH:mm) |
| `@dataConclusao` | Data e hora de conclusão do protocolo (formato: dd/MM/yyyy HH:mm) |
| `@quantidadeEquipamentos` | Total de equipamentos no protocolo |
| `@motivoDescarte` | Motivo do descarte |
| `@destinoFinal` | Destino final dos equipamentos |
| `@observacoes` | Observações gerais do protocolo |

### 🌳 Logística Reversa

| Marcador | Descrição |
|----------|-----------|
| `@empresaDestino` | Nome da empresa de destino final |
| `@cnpjDestino` | CNPJ da empresa de destino final |
| `@certificado` | Número do certificado de descarte |

### 🚛 MTR - Manifesto de Transporte de Resíduos

| Marcador | Descrição |
|----------|-----------|
| `@mtrObrigatorio` | Indica se MTR é obrigatório (Sim/Não) |
| `@mtrNumero` | Número do MTR |
| `@mtrEmitidoPor` | Quem emitiu o MTR (Gerador/Transportador/Destinador) |
| `@mtrDataEmissao` | Data de emissão do MTR (formato: dd/MM/yyyy) |
| `@mtrValidade` | Data de validade do MTR (formato: dd/MM/yyyy) |
| `@mtrEmpresaTransportadora` | Nome da empresa transportadora (exibido somente se emitido por transportador) |
| `@mtrCnpjTransportadora` | CNPJ da transportadora (exibido somente se emitido por transportador) |
| `@mtrPlacaVeiculo` | Placa do veículo (exibido somente se emitido por transportador) |
| `@mtrMotorista` | Nome do motorista (exibido somente se emitido por transportador) |
| `@mtrCpfMotorista` | CPF do motorista (exibido somente se emitido por transportador) |
| `@mtrDadosTransportadora` | Seção completa com dados da transportadora (gerada automaticamente) |

### 📦 Equipamentos

| Marcador | Descrição |
|----------|-----------|
| `@listaEquipamentos` | Lista detalhada de equipamentos (gerada automaticamente pelo sistema) |
| `@tabelaEquipamentos` | Alias para @listaEquipamentos (compatibilidade) |

---

## 📝 Notas Importantes

1. **Marcador `@mtrDadosTransportadora`**: Este marcador é preenchido automaticamente pelo sistema e contém uma tabela com todos os dados da transportadora (empresa, CNPJ, placa, motorista, CPF). Ele só é exibido quando o MTR foi emitido pelo transportador.

2. **Marcador `@listaEquipamentos`**: Este marcador é processado automaticamente e gera uma lista detalhada com:
   - Dados do equipamento (série, patrimônio, fabricante, modelo, etc.)
   - Processos obrigatórios executados (se houver)
   - Evidências fotográficas anexadas (se houver)
   - Observações do item

3. **Compatibilidade**: O sistema mantém compatibilidade com `@tabelaEquipamentos` como alias para `@listaEquipamentos`.

4. **Padrão de Marcadores**: Todos os marcadores seguem o padrão `@nomeDoMarcador` (camelCase iniciando com minúscula), consistente com os outros templates do sistema.

---

## 🔧 Exemplo de Uso no Template

```html
<div class="header">
    <h1>TERMO DE DESCARTE DE EQUIPAMENTOS</h1>
    <div class="protocolo">Protocolo: @numeroProtocolo</div>
</div>

<div class="section">
    <h2>Informações do Protocolo</h2>
    <table>
        <tr>
            <td>Cliente: @cliente</td>
            <td>Tipo: @tipoDescarte</td>
        </tr>
        <tr>
            <td>Responsável: @responsavel</td>
            <td>Data: @dataCriacao</td>
        </tr>
    </table>
</div>

<div class="section">
    <h2>MTR - Manifesto de Transporte de Resíduos</h2>
    <p>MTR Obrigatório: @mtrObrigatorio</p>
    <p>Número do MTR: @mtrNumero</p>
    @mtrDadosTransportadora
</div>

<div class="section">
    <h2>Equipamentos Descartados</h2>
    @listaEquipamentos
</div>
```

---

**Data de Atualização**: 10/10/2025  
**Versão**: 2.0 (Marcadores @)

