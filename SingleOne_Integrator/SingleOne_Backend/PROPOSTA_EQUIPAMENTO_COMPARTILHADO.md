# Proposta: Implementação de Equipamento Compartilhado

## 📋 Contexto

Atualmente, o sistema permite apenas **um responsável único** por equipamento através do campo `usuario` na tabela `equipamentos`. Esta proposta visa adicionar a funcionalidade de **equipamentos compartilhados**, permitindo que múltiplos colaboradores utilizem o mesmo recurso, mantendo-se um responsável principal.

## 🎯 Casos de Uso

1. **Equipamento compartilhado por time** - Um notebook utilizado por toda uma equipe
2. **Recurso temporário de pesquisa** - Equipamento disponibilizado para projeto específico
3. **Uso por turnos** - Equipamento utilizado por diferentes pessoas em horários alternados
4. **Responsabilidade compartilhada** - Múltiplas pessoas responsáveis pelo recurso

## 🏗️ Arquitetura Proposta

### Opção 1: Abordagem Simples (Recomendada) ✅

Esta abordagem mantém a estrutura atual e adiciona uma tabela de relacionamento.

#### 1.1. Alteração na Tabela `equipamentos`

```sql
-- Adicionar flag indicando se o equipamento é compartilhado
ALTER TABLE equipamentos 
ADD COLUMN compartilhado BOOLEAN DEFAULT FALSE NOT NULL;

-- Adicionar índice para performance
CREATE INDEX idx_equipamentos_compartilhado ON equipamentos(compartilhado);
```

**Campos na tabela equipamentos:**
- `usuario` (existente) - **Responsável Principal** (obrigatório)
- `compartilhado` (novo) - Flag booleana indicando se permite múltiplos usuários

#### 1.2. Nova Tabela: `equipamento_usuarios_compartilhados`

```sql
-- Tabela de relacionamento para usuários compartilhados
CREATE TABLE equipamento_usuarios_compartilhados (
    id SERIAL PRIMARY KEY,
    equipamento_id INTEGER NOT NULL REFERENCES equipamentos(id) ON DELETE CASCADE,
    colaborador_id INTEGER NOT NULL REFERENCES colaboradores(id) ON DELETE CASCADE,
    data_inicio TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    data_fim TIMESTAMP NULL,
    ativo BOOLEAN DEFAULT TRUE NOT NULL,
    tipo_acesso VARCHAR(50) DEFAULT 'usuario_compartilhado' NOT NULL, -- 'usuario_compartilhado', 'temporario', 'turno'
    observacao TEXT NULL,
    criado_por INTEGER NOT NULL REFERENCES usuarios(id),
    criado_em TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    -- Garantir que não haja duplicatas ativas
    CONSTRAINT uk_equipamento_colaborador_ativo UNIQUE(equipamento_id, colaborador_id, ativo)
);

-- Índices para performance
CREATE INDEX idx_equip_usuarios_comp_equipamento ON equipamento_usuarios_compartilhados(equipamento_id);
CREATE INDEX idx_equip_usuarios_comp_colaborador ON equipamento_usuarios_compartilhados(colaborador_id);
CREATE INDEX idx_equip_usuarios_comp_ativo ON equipamento_usuarios_compartilhados(ativo);
CREATE INDEX idx_equip_usuarios_comp_tipo ON equipamento_usuarios_compartilhados(tipo_acesso);

-- Comentários nas colunas
COMMENT ON TABLE equipamento_usuarios_compartilhados IS 'Gerencia múltiplos usuários para equipamentos compartilhados';
COMMENT ON COLUMN equipamento_usuarios_compartilhados.tipo_acesso IS 'Tipos: usuario_compartilhado, temporario, turno';
COMMENT ON COLUMN equipamento_usuarios_compartilhados.data_fim IS 'NULL = acesso indefinido; preenchido = acesso temporário';
```

**Campos importantes:**
- `equipamento_id` - FK para o equipamento
- `colaborador_id` - FK para o colaborador que tem acesso
- `data_inicio` / `data_fim` - Período de acesso (para casos temporários)
- `ativo` - Permite inativar sem deletar (seguindo o padrão do sistema)
- `tipo_acesso` - Diferencia os cenários (compartilhado, temporário, turno)
- `observacao` - Campo livre para justificativas

### Opção 2: Abordagem com Papéis (Mais Complexa)

Esta abordagem permite definir papéis/permissões diferentes para cada usuário.

#### 2.1. Tabela com Papéis

```sql
-- Tabela de papéis para equipamentos compartilhados
CREATE TABLE equipamento_papeis (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL UNIQUE,
    descricao TEXT,
    ativo BOOLEAN DEFAULT TRUE NOT NULL
);

-- Papéis padrão
INSERT INTO equipamento_papeis (nome, descricao) VALUES
('responsavel_principal', 'Responsável principal pelo equipamento'),
('usuario_compartilhado', 'Usuário com acesso compartilhado ao equipamento'),
('usuario_turno', 'Usuário que utiliza o equipamento em turno específico'),
('usuario_temporario', 'Usuário com acesso temporário ao equipamento');

-- Tabela de relacionamento com papéis
CREATE TABLE equipamento_usuarios_papeis (
    id SERIAL PRIMARY KEY,
    equipamento_id INTEGER NOT NULL REFERENCES equipamentos(id) ON DELETE CASCADE,
    colaborador_id INTEGER NOT NULL REFERENCES colaboradores(id) ON DELETE CASCADE,
    papel_id INTEGER NOT NULL REFERENCES equipamento_papeis(id) ON DELETE RESTRICT,
    data_inicio TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    data_fim TIMESTAMP NULL,
    turno VARCHAR(50) NULL, -- 'manha', 'tarde', 'noite', etc.
    ativo BOOLEAN DEFAULT TRUE NOT NULL,
    observacao TEXT NULL,
    criado_por INTEGER NOT NULL REFERENCES usuarios(id),
    criado_em TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT uk_equipamento_colaborador_papel UNIQUE(equipamento_id, colaborador_id, papel_id, ativo)
);
```

## 📊 Modelos C# Propostos

### Para Opção 1 (Recomendada)

```csharp
// Modelo: EquipamentoUsuarioCompartilhado.cs
namespace SingleOneAPI.Models
{
    public partial class EquipamentoUsuarioCompartilhado
    {
        public int Id { get; set; }
        public int EquipamentoId { get; set; }
        public int ColaboradorId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Ativo { get; set; }
        public string TipoAcesso { get; set; } = "usuario_compartilhado";
        public string? Observacao { get; set; }
        public int CriadoPor { get; set; }
        public DateTime CriadoEm { get; set; }

        // Navegação
        public virtual Equipamento EquipamentoNavigation { get; set; }
        public virtual Colaboradore ColaboradorNavigation { get; set; }
        public virtual Usuario CriadoPorNavigation { get; set; }
    }
}
```

```csharp
// Mapeamento: EquipamentoUsuarioCompartilhadoMap.cs
namespace SingleOneAPI.Infra.Mapeamento
{
    public class EquipamentoUsuarioCompartilhadoMap : IEntityTypeConfiguration<EquipamentoUsuarioCompartilhado>
    {
        public void Configure(EntityTypeBuilder<EquipamentoUsuarioCompartilhado> builder)
        {
            builder.ToTable("equipamento_usuarios_compartilhados");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.TipoAcesso)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.HasOne(e => e.EquipamentoNavigation)
                .WithMany()
                .HasForeignKey(e => e.EquipamentoId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(e => e.ColaboradorNavigation)
                .WithMany()
                .HasForeignKey(e => e.ColaboradorId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(e => e.CriadoPorNavigation)
                .WithMany()
                .HasForeignKey(e => e.CriadoPor)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(e => e.EquipamentoId);
            builder.HasIndex(e => e.ColaboradorId);
            builder.HasIndex(e => e.Ativo);
        }
    }
}
```

```csharp
// Atualização no modelo Equipamento.cs
public partial class Equipamento
{
    public Equipamento()
    {
        // ... código existente ...
        EquipamentoUsuariosCompartilhados = new HashSet<EquipamentoUsuarioCompartilhado>();
    }
    
    // Campos existentes...
    
    // NOVO CAMPO
    public bool Compartilhado { get; set; }
    
    // NOVA NAVEGAÇÃO
    public virtual ICollection<EquipamentoUsuarioCompartilhado> EquipamentoUsuariosCompartilhados { get; set; }
}
```

### ViewModels e DTOs

```csharp
// DTO para adicionar usuário compartilhado
public class AdicionarUsuarioCompartilhadoDTO
{
    public int EquipamentoId { get; set; }
    public int ColaboradorId { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string TipoAcesso { get; set; } = "usuario_compartilhado"; // usuario_compartilhado, temporario, turno
    public string? Observacao { get; set; }
}

// ViewModel para exibir equipamento com usuários
public class EquipamentoCompartilhadoVM
{
    public int EquipamentoId { get; set; }
    public string NumeroSerie { get; set; }
    public string Patrimonio { get; set; }
    public bool Compartilhado { get; set; }
    
    // Responsável principal
    public int? ResponsavelPrincipalId { get; set; }
    public string? ResponsavelPrincipalNome { get; set; }
    
    // Usuários compartilhados
    public List<UsuarioCompartilhadoVM> UsuariosCompartilhados { get; set; }
}

public class UsuarioCompartilhadoVM
{
    public int Id { get; set; }
    public int ColaboradorId { get; set; }
    public string ColaboradorNome { get; set; }
    public string ColaboradorMatricula { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; }
    public string TipoAcesso { get; set; }
    public string? Observacao { get; set; }
}
```

## 🔄 Lógica de Negócio Proposta

```csharp
// Métodos a serem adicionados no EquipamentoNegocio.cs

// Marcar equipamento como compartilhado
public async Task<bool> MarcarComoCompartilhado(int equipamentoId, int usuarioId)
{
    var equipamento = await _equipamentoRepository.BuscarPorId(equipamentoId);
    if (equipamento == null) return false;
    
    equipamento.Compartilhado = true;
    await _equipamentoRepository.Atualizar(equipamento);
    
    // Registrar no histórico
    await RegistrarHistoricoEquipamento(equipamentoId, usuarioId, 
        $"Equipamento marcado como compartilhado");
    
    return true;
}

// Adicionar usuário compartilhado
public async Task<int> AdicionarUsuarioCompartilhado(AdicionarUsuarioCompartilhadoDTO dto, int usuarioId)
{
    // Validar se equipamento existe e está marcado como compartilhado
    var equipamento = await _equipamentoRepository.BuscarPorId(dto.EquipamentoId);
    if (equipamento == null || !equipamento.Compartilhado)
        throw new Exception("Equipamento não encontrado ou não está marcado como compartilhado");
    
    // Validar se colaborador existe
    var colaborador = await _colaboradorRepository.BuscarPorId(dto.ColaboradorId);
    if (colaborador == null)
        throw new Exception("Colaborador não encontrado");
    
    // Verificar se já não está adicionado como usuário ativo
    var existente = await _equipamentoUsuarioCompartilhadoRepository
        .Buscar(x => x.EquipamentoId == dto.EquipamentoId && 
                     x.ColaboradorId == dto.ColaboradorId && 
                     x.Ativo)
        .FirstOrDefaultAsync();
    
    if (existente != null)
        throw new Exception("Colaborador já está cadastrado como usuário deste equipamento");
    
    // Criar relacionamento
    var usuarioCompartilhado = new EquipamentoUsuarioCompartilhado
    {
        EquipamentoId = dto.EquipamentoId,
        ColaboradorId = dto.ColaboradorId,
        DataInicio = dto.DataInicio,
        DataFim = dto.DataFim,
        TipoAcesso = dto.TipoAcesso,
        Observacao = dto.Observacao,
        Ativo = true,
        CriadoPor = usuarioId,
        CriadoEm = DateTime.Now
    };
    
    await _equipamentoUsuarioCompartilhadoRepository.Adicionar(usuarioCompartilhado);
    
    // Registrar no histórico
    await RegistrarHistoricoEquipamento(dto.EquipamentoId, usuarioId, 
        $"Usuário compartilhado adicionado: {colaborador.Nome}");
    
    return usuarioCompartilhado.Id;
}

// Remover usuário compartilhado (inativar)
public async Task<bool> RemoverUsuarioCompartilhado(int id, int usuarioId)
{
    var usuarioCompartilhado = await _equipamentoUsuarioCompartilhadoRepository.BuscarPorId(id);
    if (usuarioCompartilhado == null) return false;
    
    // Seguindo o padrão do sistema: inativar ao invés de deletar
    usuarioCompartilhado.Ativo = false;
    usuarioCompartilhado.DataFim = DateTime.Now;
    
    await _equipamentoUsuarioCompartilhadoRepository.Atualizar(usuarioCompartilhado);
    
    // Registrar no histórico
    await RegistrarHistoricoEquipamento(usuarioCompartilhado.EquipamentoId, usuarioId, 
        $"Usuário compartilhado removido (ID: {id})");
    
    return true;
}

// Listar usuários de um equipamento compartilhado
public async Task<EquipamentoCompartilhadoVM> ObterEquipamentoComUsuarios(int equipamentoId)
{
    var equipamento = await _equipamentoRepository
        .Buscar(e => e.Id == equipamentoId)
        .Include(e => e.UsuarioNavigation)
        .FirstOrDefaultAsync();
    
    if (equipamento == null) return null;
    
    var usuariosCompartilhados = await _equipamentoUsuarioCompartilhadoRepository
        .Buscar(u => u.EquipamentoId == equipamentoId && u.Ativo)
        .Include(u => u.ColaboradorNavigation)
        .Select(u => new UsuarioCompartilhadoVM
        {
            Id = u.Id,
            ColaboradorId = u.ColaboradorId,
            ColaboradorNome = u.ColaboradorNavigation.Nome,
            ColaboradorMatricula = u.ColaboradorNavigation.Matricula,
            DataInicio = u.DataInicio,
            DataFim = u.DataFim,
            Ativo = u.Ativo,
            TipoAcesso = u.TipoAcesso,
            Observacao = u.Observacao
        })
        .ToListAsync();
    
    return new EquipamentoCompartilhadoVM
    {
        EquipamentoId = equipamento.Id,
        NumeroSerie = equipamento.Numeroserie,
        Patrimonio = equipamento.Patrimonio,
        Compartilhado = equipamento.Compartilhado,
        ResponsavelPrincipalId = equipamento.Usuario,
        ResponsavelPrincipalNome = equipamento.UsuarioNavigation?.Nome,
        UsuariosCompartilhados = usuariosCompartilhados
    };
}

// Listar todos equipamentos compartilhados por cliente
public async Task<List<EquipamentoCompartilhadoVM>> ListarEquipamentosCompartilhados(int clienteId)
{
    var equipamentos = await _equipamentoRepository
        .Buscar(e => e.Cliente == clienteId && e.Compartilhado && e.Ativo)
        .Include(e => e.UsuarioNavigation)
        .ToListAsync();
    
    var result = new List<EquipamentoCompartilhadoVM>();
    
    foreach (var equipamento in equipamentos)
    {
        var vm = await ObterEquipamentoComUsuarios(equipamento.Id);
        if (vm != null) result.Add(vm);
    }
    
    return result;
}
```

## 🔌 Endpoints de API Propostos

```csharp
// Controller: EquipamentoController.cs - Adicionar novos endpoints

// GET /api/equipamento/{id}/usuarios-compartilhados
[HttpGet("{id}/usuarios-compartilhados")]
public async Task<IActionResult> ObterUsuariosCompartilhados(int id)
{
    try
    {
        var resultado = await _equipamentoNegocio.ObterEquipamentoComUsuarios(id);
        if (resultado == null)
            return NotFound(new { mensagem = "Equipamento não encontrado" });
        
        return Ok(resultado);
    }
    catch (Exception ex)
    {
        return BadRequest(new { mensagem = ex.Message });
    }
}

// POST /api/equipamento/{id}/marcar-compartilhado
[HttpPost("{id}/marcar-compartilhado")]
public async Task<IActionResult> MarcarComoCompartilhado(int id)
{
    try
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var sucesso = await _equipamentoNegocio.MarcarComoCompartilhado(id, usuarioId);
        
        if (!sucesso)
            return NotFound(new { mensagem = "Equipamento não encontrado" });
        
        return Ok(new { mensagem = "Equipamento marcado como compartilhado com sucesso" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { mensagem = ex.Message });
    }
}

// POST /api/equipamento/usuario-compartilhado
[HttpPost("usuario-compartilhado")]
public async Task<IActionResult> AdicionarUsuarioCompartilhado([FromBody] AdicionarUsuarioCompartilhadoDTO dto)
{
    try
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var id = await _equipamentoNegocio.AdicionarUsuarioCompartilhado(dto, usuarioId);
        
        return Ok(new { id, mensagem = "Usuário compartilhado adicionado com sucesso" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { mensagem = ex.Message });
    }
}

// DELETE /api/equipamento/usuario-compartilhado/{id}
[HttpDelete("usuario-compartilhado/{id}")]
public async Task<IActionResult> RemoverUsuarioCompartilhado(int id)
{
    try
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var sucesso = await _equipamentoNegocio.RemoverUsuarioCompartilhado(id, usuarioId);
        
        if (!sucesso)
            return NotFound(new { mensagem = "Registro não encontrado" });
        
        return Ok(new { mensagem = "Usuário compartilhado removido com sucesso" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { mensagem = ex.Message });
    }
}

// GET /api/equipamento/compartilhados
[HttpGet("compartilhados")]
public async Task<IActionResult> ListarEquipamentosCompartilhados([FromQuery] int clienteId)
{
    try
    {
        var resultado = await _equipamentoNegocio.ListarEquipamentosCompartilhados(clienteId);
        return Ok(resultado);
    }
    catch (Exception ex)
    {
        return BadRequest(new { mensagem = ex.Message });
    }
}
```

## 💡 Regras de Negócio Sugeridas

1. **Responsável Principal é Obrigatório**
   - Todo equipamento deve ter um responsável principal (campo `usuario` na tabela `equipamentos`)
   - O responsável principal NÃO precisa estar na lista de usuários compartilhados

2. **Flag Compartilhado**
   - Quando `compartilhado = true`, permite adicionar usuários na tabela de compartilhamento
   - Quando `compartilhado = false`, não permite adicionar usuários compartilhados

3. **Inativação ao Invés de Exclusão**
   - Seguindo o padrão do sistema, usar o campo `ativo` para remover usuários
   - Manter histórico completo de quem já teve acesso ao equipamento

4. **Acesso Temporário**
   - Se `data_fim` for NULL = acesso indefinido
   - Se `data_fim` for preenchida = acesso temporário (validar nas consultas)

5. **Validações**
   - Não permitir duplicatas: mesmo colaborador + equipamento com `ativo = true`
   - Colaborador deve estar ativo no sistema
   - Equipamento deve estar marcado como compartilhado

6. **Histórico**
   - Registrar todas as operações na tabela `equipamentohistorico`
   - Incluir informações sobre quem adicionou/removeu usuários

## 📱 Interface Frontend (Sugestão)

### Tela de Detalhes do Equipamento

```
┌─────────────────────────────────────────────────────────┐
│ Equipamento #1234 - Notebook Dell Latitude 5420         │
├─────────────────────────────────────────────────────────┤
│                                                          │
│ [✓] Equipamento Compartilhado                           │
│                                                          │
│ 👤 Responsável Principal:                                │
│    João Silva (Matrícula: 12345)                        │
│                                                          │
│ 👥 Usuários Compartilhados:                              │
│ ┌───────────────────────────────────────────────────┐  │
│ │ Maria Santos (98765)                               │  │
│ │ Tipo: Usuário Turno | Desde: 01/10/2025           │  │
│ │ [Remover]                                          │  │
│ ├───────────────────────────────────────────────────┤  │
│ │ Pedro Costa (54321)                                │  │
│ │ Tipo: Temporário | 01/10/2025 - 31/12/2025        │  │
│ │ [Remover]                                          │  │
│ └───────────────────────────────────────────────────┘  │
│                                                          │
│ [+ Adicionar Usuário Compartilhado]                     │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### Modal: Adicionar Usuário Compartilhado

```
┌─────────────────────────────────────────┐
│ Adicionar Usuário Compartilhado         │
├─────────────────────────────────────────┤
│                                          │
│ Colaborador: *                           │
│ [Selecionar Colaborador ▼]              │
│                                          │
│ Tipo de Acesso: *                        │
│ ○ Usuário Compartilhado                  │
│ ○ Usuário Temporário                     │
│ ○ Usuário por Turno                      │
│                                          │
│ Data Início: *                           │
│ [03/10/2025]                             │
│                                          │
│ Data Fim: (opcional)                     │
│ [  /  /    ]                             │
│                                          │
│ Observação: (opcional)                   │
│ [_________________________________]      │
│                                          │
│    [Cancelar]    [Adicionar]             │
│                                          │
└─────────────────────────────────────────┘
```

## ✅ Vantagens da Opção 1 (Recomendada)

1. **Simplicidade** - Mantém a estrutura existente e adiciona funcionalidade mínima necessária
2. **Compatibilidade** - Não quebra código existente
3. **Performance** - Consultas diretas e eficientes
4. **Manutenibilidade** - Fácil de entender e manter
5. **Flexibilidade** - Campo `tipo_acesso` permite diferentes cenários
6. **Rastreabilidade** - Mantém histórico completo com campos de auditoria
7. **Segue padrões do sistema** - Usa `ativo` para inativação ao invés de delete

## ⚠️ Considerações Importantes

1. **Migração de Dados** - Equipamentos existentes terão `compartilhado = false` por padrão
2. **Permissões** - Definir quem pode marcar equipamento como compartilhado e adicionar usuários
3. **Notificações** - Considerar notificar usuários quando são adicionados/removidos
4. **Relatórios** - Atualizar relatórios para incluir informações de compartilhamento
5. **Termo de Responsabilidade** - Avaliar se precisa de termo específico para equipamentos compartilhados
6. **Dashboard** - Adicionar cards/métricas sobre equipamentos compartilhados

## 🚀 Plano de Implementação

### Fase 1: Backend (Estimativa: 2-3 dias)
1. Criar migration SQL para adicionar campo `compartilhado`
2. Criar tabela `equipamento_usuarios_compartilhados`
3. Criar modelo C# `EquipamentoUsuarioCompartilhado`
4. Criar mapeamento Entity Framework
5. Atualizar modelo `Equipamento` com nova propriedade e navegação
6. Implementar métodos de negócio no `EquipamentoNegocio`
7. Criar DTOs e ViewModels
8. Implementar endpoints na API
9. Testes unitários e de integração

### Fase 2: Frontend (Estimativa: 2-3 dias)
1. Criar componente para exibir usuários compartilhados
2. Criar modal para adicionar usuário compartilhado
3. Implementar toggle para marcar equipamento como compartilhado
4. Atualizar tela de detalhes do equipamento
5. Adicionar filtro para equipamentos compartilhados na listagem
6. Implementar validações de formulário
7. Testes de interface

### Fase 3: Ajustes Finais (Estimativa: 1 dia)
1. Atualizar relatórios
2. Documentação da API
3. Ajustes de performance se necessário
4. Testes de aceitação

## 📝 Alternativas Descartadas

### Por que não usar apenas o campo `usuario`?
- Limitaria a um único responsável
- Não permitiria rastrear histórico de compartilhamento

### Por que não usar array/JSON no banco?
- Dificulta queries e relacionamentos
- Perde integridade referencial
- Complica auditoria e histórico

### Por que não criar nova tabela de "responsáveis"?
- Quebraria muito código existente
- Migrações complexas
- Risco alto de bugs

## 🎯 Conclusão

A **Opção 1** é recomendada por ser:
- ✅ Simples e direta
- ✅ Compatível com código existente
- ✅ Escalável para futuras necessidades
- ✅ Alinhada com os padrões do sistema
- ✅ Fácil de implementar e testar

Esta abordagem resolve todos os casos de uso mencionados mantendo a complexidade sob controle.

---

**Próximos Passos:**
1. Validar proposta com a equipe
2. Definir prioridade de implementação
3. Criar tasks no backlog
4. Iniciar desenvolvimento

