# 📋 Implementação de Campanhas de Assinaturas

## 🎯 Visão Geral

Sistema completo para gerenciar campanhas de assinaturas de termos de responsabilidade, com rastreamento, métricas e relatórios de aderência.

---

## 🗄️ Estrutura do Banco de Dados

### Tabelas Criadas

#### 1. **campanhasassinaturas**
Tabela principal que armazena as campanhas.

```sql
- id: INTEGER (PK, AUTO INCREMENT)
- cliente: INTEGER (FK → clientes)
- usuariocriacao: INTEGER (FK → usuarios)
- nome: VARCHAR(200) - Nome da campanha
- descricao: TEXT - Descrição detalhada
- datacriacao: TIMESTAMP - Data de criação
- datainicio: TIMESTAMP - Data de início (opcional)
- datafim: TIMESTAMP - Data de fim (opcional)
- status: CHAR(1) - A=Ativa, I=Inativa, C=Concluída, G=Agendada
- filtrosjson: TEXT - JSON com filtros aplicados
- totalcolaboradores: INTEGER - Total de colaboradores
- totalenviados: INTEGER - Total de envios realizados
- totalassinados: INTEGER - Total de assinaturas
- totalpendentes: INTEGER - Total pendente
- percentualadesao: DECIMAL(5,2) - Percentual de adesão
- dataultimoenvio: TIMESTAMP - Data do último envio
- dataconclusao: TIMESTAMP - Data de conclusão
```

#### 2. **campanhascolaboradores**
Tabela de associação entre campanhas e colaboradores.

```sql
- id: INTEGER (PK, AUTO INCREMENT)
- campanhaid: INTEGER (FK → campanhasassinaturas)
- colaboradorid: INTEGER (FK → colaboradores)
- datainclusao: TIMESTAMP - Data de inclusão na campanha
- statusassinatura: CHAR(1) - P=Pendente, E=Enviado, A=Assinado, R=Recusado
- dataenvio: TIMESTAMP - Data do primeiro envio
- dataassinatura: TIMESTAMP - Data da assinatura
- totalenvios: INTEGER - Total de envios para este colaborador
- dataultimoenvio: TIMESTAMP - Data do último envio
- ipenvio: VARCHAR(50) - IP do último envio
- localizacaoenvio: VARCHAR(500) - Localização do envio
```

### Views Criadas

#### **vw_campanhas_resumo**
Visão resumida das campanhas com estatísticas atualizadas.

#### **vw_campanhas_colaboradores_detalhado**
Visão detalhada de colaboradores por campanha com informações completas.

### Funções e Triggers

#### **atualizar_estatisticas_campanha(p_campanha_id)**
Função para atualizar automaticamente as estatísticas de uma campanha.

#### **trigger_atualizar_campanha**
Trigger que atualiza estatísticas automaticamente após INSERT/UPDATE/DELETE em `campanhascolaboradores`.

---

## 📦 Modelos C# Criados

### 1. CampanhaAssinatura.cs
```csharp
namespace SingleOneAPI.Models
{
    public partial class CampanhaAssinatura
    {
        // Propriedades principais
        // Propriedades de navegação
        // ICollection<CampanhaColaborador>
    }
}
```

### 2. CampanhaColaborador.cs
```csharp
namespace SingleOneAPI.Models
{
    public partial class CampanhaColaborador
    {
        // Propriedades de associação
        // Status de assinatura
        // Informações de envio
    }
}
```

### 3. DTOs Criados

#### CampanhaResumoDTO
- Resumo completo da campanha
- Estatísticas gerais
- Informações do usuário criador

#### RelatorioAderenciaDTO
- Relatório completo de aderência
- Estatísticas por empresa
- Estatísticas por localidade
- Estatísticas por tipo de colaborador
- Timeline de envios

#### ColaboradorPendenteDTO
- Informações de colaboradores pendentes
- Dados de envio
- Dias desde último envio

---

## 🏗️ Camada de Negócios

### Interface: ICampanhaAssinaturaNegocio

#### CRUD Básico
- `CriarCampanha()`
- `ObterCampanhaPorId()`
- `ListarCampanhasPorCliente()`
- `AtualizarCampanha()`
- `InativarCampanha()`
- `ConcluirCampanha()`

#### Gerenciamento de Colaboradores
- `AdicionarColaboradoresNaCampanha()`
- `RemoverColaboradorDaCampanha()`
- `ObterColaboradoresDaCampanha()`

#### Envio de Termos
- `EnviarTermoParaColaborador()`
- `EnviarTermosEmMassa()`

#### Atualização de Status
- `MarcarComoAssinado()`
- `AtualizarEstatisticasCampanha()`

#### Relatórios
- `ObterResumoCampanha()`
- `ObterResumoCampanhasPorCliente()`
- `ObterRelatorioAderencia()`
- `ObterColaboradoresPendentes()`

---

## 🌐 API Endpoints

### Base URL: `/api/CampanhaAssinatura`

#### CRUD Básico

```
POST   /Criar
GET    /{id}
GET    /Cliente/{clienteId}
PUT    /Atualizar
PUT    /Inativar/{id}
PUT    /Concluir/{id}
```

#### Gerenciamento de Colaboradores

```
POST   /{campanhaId}/AdicionarColaboradores
DELETE /{campanhaId}/RemoverColaborador/{colaboradorId}
GET    /{campanhaId}/Colaboradores
```

#### Envio de Termos

```
POST   /{campanhaId}/EnviarTermo/{colaboradorId}
POST   /{campanhaId}/EnviarTermosEmMassa
PUT    /{campanhaId}/MarcarAssinado/{colaboradorId}
POST   /{campanhaId}/AtualizarEstatisticas
```

#### Relatórios

```
GET    /{id}/Resumo
GET    /Cliente/{clienteId}/Resumos
GET    /{id}/RelatorioAderencia
GET    /{id}/Pendentes
```

---

## 🔧 Passos de Instalação

### 1. Executar Script SQL
```bash
cd SingleOneAPI/Scripts
# Executar: 001_CriarTabelasCampanhasAssinaturas.sql no PostgreSQL
```

### 2. Registrar Serviços no Startup.cs/Program.cs

```csharp
// Adicionar no ConfigureServices
services.AddScoped<IRepository<CampanhaAssinatura>, Repository<CampanhaAssinatura>>();
services.AddScoped<IRepository<CampanhaColaborador>, Repository<CampanhaColaborador>>();
services.AddScoped<ICampanhaAssinaturaNegocio, CampanhaAssinaturaNegocio>();
```

### 3. Atualizar DbContext (se necessário)

```csharp
public class SingleOneDbContext : DbContext
{
    public DbSet<CampanhaAssinatura> CampanhasAssinaturas { get; set; }
    public DbSet<CampanhaColaborador> CampanhasColaboradores { get; set; }
    
    // ... outras configurações
}
```

---

## 📊 Exemplos de Uso

### Criar Nova Campanha

```http
POST /api/CampanhaAssinatura/Criar
Content-Type: application/json

{
  "clienteId": 1,
  "usuarioCriacaoId": 1,
  "nome": "Campanha Q1 2025",
  "descricao": "Campanha de assinaturas do primeiro trimestre",
  "dataInicio": "2025-01-01T00:00:00",
  "dataFim": "2025-03-31T23:59:59",
  "filtrosJson": "{\"empresas\":[1,2],\"tipos\":[\"E\",\"T\"]}",
  "colaboradoresIds": [10, 20, 30, 40, 50]
}
```

### Enviar Termos em Massa

```http
POST /api/CampanhaAssinatura/5/EnviarTermosEmMassa
Content-Type: application/json

{
  "colaboradoresIds": [10, 20, 30],
  "usuarioEnvioId": 1,
  "ip": "192.168.1.100",
  "localizacao": "São Paulo, SP, Brasil (Lat: -23.5505, Long: -46.6333)"
}
```

### Obter Relatório de Aderência

```http
GET /api/CampanhaAssinatura/5/RelatorioAderencia
```

**Resposta:**
```json
{
  "campanhaId": 5,
  "campanhaNome": "Campanha Q1 2025",
  "dataCriacao": "2025-01-01T10:00:00",
  "totalColaboradores": 50,
  "totalEnviados": 45,
  "totalAssinados": 30,
  "totalPendentes": 15,
  "totalRecusados": 5,
  "percentualAdesao": 60.00,
  "percentualPendente": 30.00,
  "percentualRecusado": 10.00,
  "aderenciaPorEmpresa": [
    {
      "empresaNome": "Empresa A",
      "total": 20,
      "assinados": 15,
      "pendentes": 5,
      "percentualAdesao": 75.00
    }
  ],
  "aderenciaPorLocalidade": [...],
  "aderenciaPorTipo": [...],
  "timelineEnvios": [...]
}
```

---

## 🎯 Fluxo de Trabalho

### 1. Criação de Campanha
```
Usuario cria campanha
  ↓
Seleciona filtros (empresas, localidades, tipos)
  ↓
Sistema aplica filtros e seleciona colaboradores
  ↓
Campanha criada com status 'Ativa'
  ↓
Colaboradores adicionados com status 'Pendente'
```

### 2. Envio de Termos
```
Usuário seleciona colaboradores
  ↓
Clica em "Enviar Termos em Massa"
  ↓
Sistema captura geolocalização
  ↓
Envia email para cada colaborador
  ↓
Atualiza status para 'Enviado'
  ↓
Registra IP e localização
  ↓
Atualiza estatísticas da campanha
```

### 3. Assinatura
```
Colaborador recebe email
  ↓
Acessa link do termo
  ↓
Assina eletronicamente
  ↓
Sistema atualiza status para 'Assinado'
  ↓
Registra data de assinatura
  ↓
Atualiza estatísticas da campanha
  ↓
Atualiza percentual de adesão
```

### 4. Relatórios
```
Usuário acessa relatório de aderência
  ↓
Sistema consolida dados
  ↓
Calcula estatísticas por empresa
  ↓
Calcula estatísticas por localidade
  ↓
Calcula estatísticas por tipo
  ↓
Gera timeline de envios
  ↓
Exibe dashboard visual
```

---

## 📈 Métricas Calculadas

### Por Campanha
- Total de colaboradores
- Total de envios realizados
- Total de assinaturas
- Total de pendentes
- Percentual de adesão
- Data do último envio

### Por Empresa
- Total de colaboradores por empresa
- Assinados por empresa
- Pendentes por empresa
- Percentual de adesão por empresa

### Por Localidade
- Total de colaboradores por localidade
- Assinados por localidade
- Pendentes por localidade
- Percentual de adesão por localidade

### Por Tipo de Colaborador
- Total de colaboradores por tipo (Efetivo, Terceiro, etc)
- Assinados por tipo
- Pendentes por tipo
- Percentual de adesão por tipo

---

## 🔒 Segurança e Auditoria

### Informações Registradas
- ✅ IP do envio
- ✅ Localização geográfica
- ✅ Data e hora de cada ação
- ✅ Usuário que realizou a ação
- ✅ Total de tentativas de envio
- ✅ Status de cada colaborador

### Rastreabilidade
- Histórico completo de envios
- Timeline de assinaturas
- Identificação de colaboradores pendentes
- Dias desde último envio

---

## 🚀 Próximos Passos

### Frontend (Pendente)
- [ ] Integrar com APIs criadas
- [ ] Adicionar listagem de campanhas ativas
- [ ] Criar página de detalhes da campanha
- [ ] Implementar dashboard de aderência
- [ ] Adicionar gráficos e visualizações
- [ ] Exportar relatórios para Excel/PDF

### Backend (Melhorias Futuras)
- [ ] Agendamento de envios
- [ ] Templates de email personalizáveis
- [ ] Notificações automáticas de follow-up
- [ ] Integração com WhatsApp/SMS
- [ ] Sistema de lembretes automáticos
- [ ] Dashboard analítico avançado

---

## 📞 Suporte

Para dúvidas sobre a implementação:
1. Verificar este documento
2. Revisar código nos arquivos criados
3. Consultar logs do sistema
4. Contatar equipe de desenvolvimento

---

## 📝 Changelog

**v1.0.0 - 2025-10-20**
- Criação inicial do sistema de campanhas
- Modelos, negócios e controllers implementados
- Script SQL com tabelas, views e triggers
- DTOs para relatórios
- Documentação completa

---

## 📚 Arquivos Criados

### Backend
```
Models/
├── CampanhaAssinatura.cs
├── CampanhaColaborador.cs
└── DTO/
    ├── CampanhaResumoDTO.cs
    ├── RelatorioAderenciaDTO.cs
    └── ColaboradorPendenteDTO.cs

Negocios/
├── Interfaces/
│   └── ICampanhaAssinaturaNegocio.cs
└── CampanhaAssinaturaNegocio.cs

Controllers/
└── CampanhaAssinaturaController.cs

Scripts/
└── 001_CriarTabelasCampanhasAssinaturas.sql

Documentação/
└── CAMPANHAS_ASSINATURAS_IMPLEMENTACAO.md
```

---

**Sistema pronto para uso! 🎉**

Execute o script SQL, registre os serviços e comece a usar as APIs.

