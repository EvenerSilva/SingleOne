# 💡 Exemplos Práticos: Equipamento Compartilhado

## 📖 Índice
1. [Caso 1: Notebook de Time de Desenvolvimento](#caso-1-notebook-de-time-de-desenvolvimento)
2. [Caso 2: Equipamento Temporário para Pesquisa](#caso-2-equipamento-temporário-para-pesquisa)
3. [Caso 3: Recurso por Turnos](#caso-3-recurso-por-turnos)
4. [Caso 4: Responsabilidade Compartilhada](#caso-4-responsabilidade-compartilhada)
5. [Exemplos de Código Backend](#exemplos-de-código-backend)
6. [Exemplos de Chamadas de API](#exemplos-de-chamadas-de-api)
7. [Exemplos de Consultas SQL](#exemplos-de-consultas-sql)

---

## Caso 1: Notebook de Time de Desenvolvimento

### Contexto
Um notebook de alta performance é compartilhado entre desenvolvedores do time para testes de integração.

### Configuração

**Equipamento:**
- Patrimônio: `NB-DEV-001`
- Responsável Principal: `João Silva` (Tech Lead)
- Compartilhado: `Sim`

**Usuários Compartilhados:**
```
1. Maria Santos (Dev Senior)
   - Tipo: usuario_compartilhado
   - Desde: 01/10/2025
   - Sem data fim (indefinido)
   - Obs: "Testes de performance"

2. Pedro Costa (Dev Pleno)
   - Tipo: usuario_compartilhado
   - Desde: 01/10/2025
   - Sem data fim (indefinido)
   - Obs: "Testes de integração"

3. Ana Oliveira (QA)
   - Tipo: usuario_compartilhado
   - Desde: 15/09/2025
   - Sem data fim (indefinido)
   - Obs: "Validações de qualidade"
```

### SQL para Implementar

```sql
-- 1. Marcar equipamento como compartilhado
UPDATE equipamentos 
SET compartilhado = TRUE 
WHERE patrimonio = 'NB-DEV-001';

-- 2. Adicionar Maria Santos
SELECT fn_adicionar_usuario_compartilhado(
    (SELECT id FROM equipamentos WHERE patrimonio = 'NB-DEV-001'),
    (SELECT id FROM colaboradores WHERE matricula = '98765'),
    CURRENT_TIMESTAMP,
    NULL,
    'usuario_compartilhado',
    'Testes de performance',
    (SELECT id FROM usuarios WHERE nome = 'Admin')
);

-- 3. Adicionar Pedro Costa
SELECT fn_adicionar_usuario_compartilhado(
    (SELECT id FROM equipamentos WHERE patrimonio = 'NB-DEV-001'),
    (SELECT id FROM colaboradores WHERE matricula = '54321'),
    CURRENT_TIMESTAMP,
    NULL,
    'usuario_compartilhado',
    'Testes de integração',
    (SELECT id FROM usuarios WHERE nome = 'Admin')
);

-- 4. Adicionar Ana Oliveira
SELECT fn_adicionar_usuario_compartilhado(
    (SELECT id FROM equipamentos WHERE patrimonio = 'NB-DEV-001'),
    (SELECT id FROM colaboradores WHERE matricula = '11111'),
    '2025-09-15 00:00:00',
    NULL,
    'usuario_compartilhado',
    'Validações de qualidade',
    (SELECT id FROM usuarios WHERE nome = 'Admin')
);
```

### Resultado Esperado

```
📱 Equipamento: NB-DEV-001
👤 Responsável: João Silva (Tech Lead)
👥 Usuários Compartilhados: 3
   ├─ Maria Santos - Performance Testing
   ├─ Pedro Costa - Integration Testing
   └─ Ana Oliveira - Quality Assurance
```

---

## Caso 2: Equipamento Temporário para Pesquisa

### Contexto
Um microscópio científico é disponibilizado temporariamente para um projeto de pesquisa que durará 3 meses.

### Configuração

**Equipamento:**
- Patrimônio: `MICRO-LAB-005`
- Responsável Principal: `Dr. Carlos Oliveira` (Chefe de Laboratório)
- Compartilhado: `Sim`

**Usuários Compartilhados:**
```
1. Dra. Juliana Alves (Pesquisadora Principal)
   - Tipo: temporario
   - Período: 01/10/2025 a 31/12/2025
   - Obs: "Projeto Biologia Molecular - Fase 2"

2. Lucas Mendes (Assistente de Pesquisa)
   - Tipo: temporario
   - Período: 01/10/2025 a 31/12/2025
   - Obs: "Assistente do Projeto Biologia Molecular"

3. Fernanda Lima (Bolsista)
   - Tipo: temporario
   - Período: 15/10/2025 a 15/12/2025
   - Obs: "Bolsista CNPQ - 2 meses"
```

### SQL para Implementar

```sql
-- Marcar como compartilhado
UPDATE equipamentos 
SET compartilhado = TRUE 
WHERE patrimonio = 'MICRO-LAB-005';

-- Adicionar Dra. Juliana (3 meses)
SELECT fn_adicionar_usuario_compartilhado(
    (SELECT id FROM equipamentos WHERE patrimonio = 'MICRO-LAB-005'),
    (SELECT id FROM colaboradores WHERE nome = 'Juliana Alves'),
    '2025-10-01 00:00:00',
    '2025-12-31 23:59:59',
    'temporario',
    'Projeto Biologia Molecular - Fase 2',
    (SELECT id FROM usuarios WHERE nome = 'Dr. Carlos Oliveira')
);

-- Adicionar Lucas (3 meses)
SELECT fn_adicionar_usuario_compartilhado(
    (SELECT id FROM equipamentos WHERE patrimonio = 'MICRO-LAB-005'),
    (SELECT id FROM colaboradores WHERE nome = 'Lucas Mendes'),
    '2025-10-01 00:00:00',
    '2025-12-31 23:59:59',
    'temporario',
    'Assistente do Projeto Biologia Molecular',
    (SELECT id FROM usuarios WHERE nome = 'Dr. Carlos Oliveira')
);

-- Adicionar Fernanda (2 meses)
SELECT fn_adicionar_usuario_compartilhado(
    (SELECT id FROM equipamentos WHERE patrimonio = 'MICRO-LAB-005'),
    (SELECT id FROM colaboradores WHERE nome = 'Fernanda Lima'),
    '2025-10-15 00:00:00',
    '2025-12-15 23:59:59',
    'temporario',
    'Bolsista CNPQ - 2 meses',
    (SELECT id FROM usuarios WHERE nome = 'Dr. Carlos Oliveira')
);
```

### Consulta de Acessos Temporários Expirados

```sql
-- Ver acessos que vão expirar nos próximos 7 dias
SELECT 
    patrimonio,
    colaborador_nome,
    data_fim,
    data_fim - CURRENT_TIMESTAMP AS tempo_restante
FROM vw_equipamentos_usuarios_compartilhados
WHERE tipo_acesso = 'temporario'
  AND data_fim BETWEEN CURRENT_TIMESTAMP AND CURRENT_TIMESTAMP + INTERVAL '7 days'
ORDER BY data_fim;
```

---

## Caso 3: Recurso por Turnos

### Contexto
Uma máquina CNC de produção é operada por diferentes operadores em três turnos (manhã, tarde, noite).

### Configuração

**Equipamento:**
- Patrimônio: `CNC-PROD-012`
- Responsável Principal: `Roberto Silva` (Supervisor de Produção)
- Compartilhado: `Sim`

**Usuários Compartilhados:**
```
1. Marcos Ferreira
   - Tipo: turno
   - Turno: Manhã (06:00 - 14:00)
   - Obs: "Operador turno manhã"

2. Paulo Santos
   - Tipo: turno
   - Turno: Tarde (14:00 - 22:00)
   - Obs: "Operador turno tarde"

3. José Almeida
   - Tipo: turno
   - Turno: Noite (22:00 - 06:00)
   - Obs: "Operador turno noite"
```

### SQL para Implementar

```sql
-- Marcar como compartilhado
UPDATE equipamentos 
SET compartilhado = TRUE 
WHERE patrimonio = 'CNC-PROD-012';

-- Adicionar operadores por turno
SELECT fn_adicionar_usuario_compartilhado(
    (SELECT id FROM equipamentos WHERE patrimonio = 'CNC-PROD-012'),
    (SELECT id FROM colaboradores WHERE nome = 'Marcos Ferreira'),
    CURRENT_TIMESTAMP,
    NULL,
    'turno',
    'Operador turno manhã (06:00 - 14:00)',
    (SELECT id FROM usuarios WHERE nome = 'Roberto Silva')
);

SELECT fn_adicionar_usuario_compartilhado(
    (SELECT id FROM equipamentos WHERE patrimonio = 'CNC-PROD-012'),
    (SELECT id FROM colaboradores WHERE nome = 'Paulo Santos'),
    CURRENT_TIMESTAMP,
    NULL,
    'turno',
    'Operador turno tarde (14:00 - 22:00)',
    (SELECT id FROM usuarios WHERE nome = 'Roberto Silva')
);

SELECT fn_adicionar_usuario_compartilhado(
    (SELECT id FROM equipamentos WHERE patrimonio = 'CNC-PROD-012'),
    (SELECT id FROM colaboradores WHERE nome = 'José Almeida'),
    CURRENT_TIMESTAMP,
    NULL,
    'turno',
    'Operador turno noite (22:00 - 06:00)',
    (SELECT id FROM usuarios WHERE nome = 'Roberto Silva')
);
```

### Relatório de Equipamentos por Turno

```sql
-- Listar todos os equipamentos usados por turnos
SELECT 
    e.patrimonio,
    e.numeroserie,
    u.nome AS responsavel_principal,
    COUNT(euc.id) AS total_operadores_turno
FROM equipamentos e
INNER JOIN equipamento_usuarios_compartilhados euc ON e.id = euc.equipamento_id
LEFT JOIN usuarios u ON e.usuario = u.id
WHERE e.compartilhado = TRUE
  AND euc.tipo_acesso = 'turno'
  AND euc.ativo = TRUE
GROUP BY e.id, e.patrimonio, e.numeroserie, u.nome
ORDER BY total_operadores_turno DESC;
```

---

## Caso 4: Responsabilidade Compartilhada

### Contexto
Um servidor crítico tem múltiplos administradores responsáveis pela manutenção e monitoramento.

### Configuração

**Equipamento:**
- Patrimônio: `SRV-PROD-001`
- Responsável Principal: `Ricardo Moura` (Coordenador de Infraestrutura)
- Compartilhado: `Sim`

**Usuários Compartilhados:**
```
1. André Costa (SysAdmin Senior)
   - Tipo: usuario_compartilhado
   - Obs: "Administrador principal - 24x7"

2. Beatriz Souza (SysAdmin)
   - Tipo: usuario_compartilhado
   - Obs: "Administrador backup"

3. Carlos Dias (DevOps)
   - Tipo: usuario_compartilhado
   - Obs: "Deploy e CI/CD"

4. Daniel Ribeiro (Segurança)
   - Tipo: usuario_compartilhado
   - Obs: "Monitoramento de segurança"
```

### SQL para Implementar

```sql
UPDATE equipamentos 
SET compartilhado = TRUE 
WHERE patrimonio = 'SRV-PROD-001';

-- Adicionar administradores
DECLARE
    v_equipamento_id INT = (SELECT id FROM equipamentos WHERE patrimonio = 'SRV-PROD-001');
    v_criador_id INT = (SELECT id FROM usuarios WHERE nome = 'Ricardo Moura');

SELECT fn_adicionar_usuario_compartilhado(
    v_equipamento_id,
    (SELECT id FROM colaboradores WHERE nome = 'André Costa'),
    CURRENT_TIMESTAMP, NULL, 'usuario_compartilhado',
    'Administrador principal - 24x7', v_criador_id
);

SELECT fn_adicionar_usuario_compartilhado(
    v_equipamento_id,
    (SELECT id FROM colaboradores WHERE nome = 'Beatriz Souza'),
    CURRENT_TIMESTAMP, NULL, 'usuario_compartilhado',
    'Administrador backup', v_criador_id
);

SELECT fn_adicionar_usuario_compartilhado(
    v_equipamento_id,
    (SELECT id FROM colaboradores WHERE nome = 'Carlos Dias'),
    CURRENT_TIMESTAMP, NULL, 'usuario_compartilhado',
    'Deploy e CI/CD', v_criador_id
);

SELECT fn_adicionar_usuario_compartilhado(
    v_equipamento_id,
    (SELECT id FROM colaboradores WHERE nome = 'Daniel Ribeiro'),
    CURRENT_TIMESTAMP, NULL, 'usuario_compartilhado',
    'Monitoramento de segurança', v_criador_id
);
```

---

## Exemplos de Código Backend

### 1. Adicionar Usuário Compartilhado (C#)

```csharp
// Controller
[HttpPost("usuario-compartilhado")]
public async Task<IActionResult> AdicionarUsuarioCompartilhado(
    [FromBody] AdicionarUsuarioCompartilhadoDTO dto)
{
    try
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var resultado = await _equipamentoNegocio
            .AdicionarUsuarioCompartilhado(dto, usuarioId);
        
        return Ok(new { 
            id = resultado, 
            mensagem = "Usuário compartilhado adicionado com sucesso" 
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new { mensagem = ex.Message });
    }
}

// Chamada no service/negócio
var dto = new AdicionarUsuarioCompartilhadoDTO
{
    EquipamentoId = 123,
    ColaboradorId = 456,
    DataInicio = DateTime.Now,
    DataFim = DateTime.Now.AddMonths(3), // Temporário por 3 meses
    TipoAcesso = "temporario",
    Observacao = "Projeto Alpha - Fase 2"
};

var id = await _equipamentoService.AdicionarUsuarioCompartilhado(dto, userId);
```

### 2. Listar Equipamento com Usuários (C#)

```csharp
// Controller
[HttpGet("{id}/usuarios-compartilhados")]
public async Task<IActionResult> ObterUsuariosCompartilhados(int id)
{
    try
    {
        var resultado = await _equipamentoNegocio
            .ObterEquipamentoComUsuarios(id);
        
        if (resultado == null)
            return NotFound(new { mensagem = "Equipamento não encontrado" });
        
        return Ok(resultado);
    }
    catch (Exception ex)
    {
        return BadRequest(new { mensagem = ex.Message });
    }
}

// Response esperado:
{
    "equipamentoId": 123,
    "patrimonio": "NB-DEV-001",
    "numeroserie": "SN123456",
    "compartilhado": true,
    "responsavelPrincipalId": 10,
    "responsavelPrincipalNome": "João Silva",
    "usuariosCompartilhados": [
        {
            "id": 1,
            "colaboradorId": 456,
            "colaboradorNome": "Maria Santos",
            "colaboradorMatricula": "98765",
            "dataInicio": "2025-10-01T00:00:00",
            "dataFim": null,
            "ativo": true,
            "tipoAcesso": "usuario_compartilhado",
            "observacao": "Testes de performance"
        },
        {
            "id": 2,
            "colaboradorId": 789,
            "colaboradorNome": "Pedro Costa",
            "colaboradorMatricula": "54321",
            "dataInicio": "2025-10-01T00:00:00",
            "dataFim": "2025-12-31T23:59:59",
            "ativo": true,
            "tipoAcesso": "temporario",
            "observacao": "Projeto temporário"
        }
    ]
}
```

### 3. Remover Usuário Compartilhado (C#)

```csharp
// Controller
[HttpDelete("usuario-compartilhado/{id}")]
public async Task<IActionResult> RemoverUsuarioCompartilhado(int id)
{
    try
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var sucesso = await _equipamentoNegocio
            .RemoverUsuarioCompartilhado(id, usuarioId);
        
        if (!sucesso)
            return NotFound(new { mensagem = "Registro não encontrado" });
        
        return Ok(new { mensagem = "Usuário compartilhado removido com sucesso" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { mensagem = ex.Message });
    }
}
```

---

## Exemplos de Chamadas de API

### 1. Marcar Equipamento como Compartilhado

```bash
POST /api/equipamento/123/marcar-compartilhado
Authorization: Bearer {token}

# Response:
{
    "mensagem": "Equipamento marcado como compartilhado com sucesso"
}
```

### 2. Adicionar Usuário Compartilhado

```bash
POST /api/equipamento/usuario-compartilhado
Authorization: Bearer {token}
Content-Type: application/json

{
    "equipamentoId": 123,
    "colaboradorId": 456,
    "dataInicio": "2025-10-03T00:00:00",
    "dataFim": null,
    "tipoAcesso": "usuario_compartilhado",
    "observacao": "Usuário compartilhado do time de desenvolvimento"
}

# Response:
{
    "id": 789,
    "mensagem": "Usuário compartilhado adicionado com sucesso"
}
```

### 3. Listar Usuários de um Equipamento

```bash
GET /api/equipamento/123/usuarios-compartilhados
Authorization: Bearer {token}

# Response: (ver exemplo no item 2 do código backend)
```

### 4. Remover Usuário Compartilhado

```bash
DELETE /api/equipamento/usuario-compartilhado/789
Authorization: Bearer {token}

# Response:
{
    "mensagem": "Usuário compartilhado removido com sucesso"
}
```

### 5. Listar Todos Equipamentos Compartilhados

```bash
GET /api/equipamento/compartilhados?clienteId=5
Authorization: Bearer {token}

# Response:
[
    {
        "equipamentoId": 123,
        "patrimonio": "NB-DEV-001",
        "compartilhado": true,
        "responsavelPrincipalNome": "João Silva",
        "usuariosCompartilhados": [...]
    },
    {
        "equipamentoId": 456,
        "patrimonio": "MICRO-LAB-005",
        "compartilhado": true,
        "responsavelPrincipalNome": "Dr. Carlos Oliveira",
        "usuariosCompartilhados": [...]
    }
]
```

---

## Exemplos de Consultas SQL

### 1. Equipamentos mais Compartilhados

```sql
-- Top 10 equipamentos com mais usuários compartilhados
SELECT 
    e.patrimonio,
    e.numeroserie,
    u.nome AS responsavel_principal,
    COUNT(euc.id) AS total_usuarios
FROM equipamentos e
INNER JOIN equipamento_usuarios_compartilhados euc ON e.id = euc.equipamento_id
LEFT JOIN usuarios u ON e.usuario = u.id
WHERE e.compartilhado = TRUE
  AND euc.ativo = TRUE
GROUP BY e.id, e.patrimonio, e.numeroserie, u.nome
ORDER BY total_usuarios DESC
LIMIT 10;
```

### 2. Colaboradores com Mais Equipamentos Compartilhados

```sql
-- Colaboradores que usam mais equipamentos compartilhados
SELECT 
    c.nome,
    c.matricula,
    c.cargo,
    COUNT(DISTINCT euc.equipamento_id) AS total_equipamentos,
    json_agg(
        json_build_object(
            'patrimonio', e.patrimonio,
            'tipo_acesso', euc.tipo_acesso
        )
    ) AS equipamentos
FROM colaboradores c
INNER JOIN equipamento_usuarios_compartilhados euc ON c.id = euc.colaborador_id
INNER JOIN equipamentos e ON euc.equipamento_id = e.id
WHERE euc.ativo = TRUE
  AND c.situacao = 'A'
GROUP BY c.id, c.nome, c.matricula, c.cargo
ORDER BY total_equipamentos DESC;
```

### 3. Acessos Temporários Expirando

```sql
-- Acessos temporários que expiram nos próximos 30 dias
SELECT 
    e.patrimonio,
    c.nome AS colaborador,
    c.email,
    euc.data_fim,
    euc.data_fim - CURRENT_TIMESTAMP AS tempo_restante,
    euc.observacao
FROM equipamento_usuarios_compartilhados euc
INNER JOIN equipamentos e ON euc.equipamento_id = e.id
INNER JOIN colaboradores c ON euc.colaborador_id = c.id
WHERE euc.tipo_acesso = 'temporario'
  AND euc.ativo = TRUE
  AND euc.data_fim BETWEEN CURRENT_TIMESTAMP AND CURRENT_TIMESTAMP + INTERVAL '30 days'
ORDER BY euc.data_fim;
```

### 4. Auditoria de Compartilhamentos

```sql
-- Histórico completo de compartilhamentos (incluindo inativos)
SELECT 
    e.patrimonio,
    c.nome AS colaborador,
    euc.tipo_acesso,
    euc.data_inicio,
    euc.data_fim,
    euc.ativo,
    u.nome AS adicionado_por,
    euc.criado_em,
    CASE 
        WHEN euc.ativo = TRUE THEN 'Ativo'
        ELSE 'Removido em ' || TO_CHAR(euc.data_fim, 'DD/MM/YYYY')
    END AS status
FROM equipamento_usuarios_compartilhados euc
INNER JOIN equipamentos e ON euc.equipamento_id = e.id
INNER JOIN colaboradores c ON euc.colaborador_id = c.id
LEFT JOIN usuarios u ON euc.criado_por = u.id
WHERE e.patrimonio = 'NB-DEV-001'
ORDER BY euc.criado_em DESC;
```

### 5. Dashboard de Estatísticas

```sql
-- Estatísticas gerais de compartilhamento
SELECT 
    'Total de Equipamentos Compartilhados' AS metrica,
    COUNT(*) AS valor
FROM equipamentos 
WHERE ativo = TRUE AND compartilhado = TRUE

UNION ALL

SELECT 
    'Total de Usuários Compartilhados Ativos',
    COUNT(*)
FROM equipamento_usuarios_compartilhados
WHERE ativo = TRUE

UNION ALL

SELECT 
    'Acessos Temporários Ativos',
    COUNT(*)
FROM equipamento_usuarios_compartilhados
WHERE ativo = TRUE AND tipo_acesso = 'temporario'

UNION ALL

SELECT 
    'Usuários por Turno',
    COUNT(*)
FROM equipamento_usuarios_compartilhados
WHERE ativo = TRUE AND tipo_acesso = 'turno'

UNION ALL

SELECT 
    'Acessos Expirados (não removidos)',
    COUNT(*)
FROM equipamento_usuarios_compartilhados
WHERE ativo = TRUE 
  AND tipo_acesso = 'temporario'
  AND data_fim < CURRENT_TIMESTAMP;
```

---

## 🎯 Checklist de Implementação

- [ ] Executar script SQL de criação
- [ ] Testar funções SQL
- [ ] Criar modelos C#
- [ ] Implementar lógica de negócio
- [ ] Criar endpoints de API
- [ ] Desenvolver componentes frontend
- [ ] Implementar validações
- [ ] Criar testes unitários
- [ ] Criar testes de integração
- [ ] Atualizar documentação da API
- [ ] Treinar usuários
- [ ] Deploy em produção

---

**Última Atualização:** 03/10/2025  
**Versão:** 1.0

