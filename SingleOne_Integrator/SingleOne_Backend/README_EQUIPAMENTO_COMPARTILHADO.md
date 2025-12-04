# 📚 Documentação Completa: Equipamento Compartilhado

> **Status:** 📋 Proposta para Avaliação  
> **Data:** 03/10/2025  
> **Versão:** 1.0

## 📖 Visão Geral

Esta documentação apresenta uma proposta completa para implementar a funcionalidade de **Equipamento Compartilhado** no sistema SingleOne, permitindo que múltiplos usuários utilizem o mesmo recurso, mantendo um responsável principal.

## 🎯 Casos de Uso Atendidos

- ✅ Equipamentos compartilhados por times
- ✅ Recursos temporários para projetos/pesquisas
- ✅ Uso por turnos de trabalho
- ✅ Responsabilidade compartilhada

## 📂 Estrutura da Documentação

### 1️⃣ Documentos para Decisão

| Arquivo | Descrição | Quando Usar |
|---------|-----------|-------------|
| **[RESUMO_EQUIPAMENTO_COMPARTILHADO.md](RESUMO_EQUIPAMENTO_COMPARTILHADO.md)** | Resumo executivo conciso | Para tomada de decisão rápida |
| **[PROPOSTA_EQUIPAMENTO_COMPARTILHADO.md](PROPOSTA_EQUIPAMENTO_COMPARTILHADO.md)** | Documentação técnica completa | Para análise detalhada |

### 2️⃣ Recursos Visuais

| Arquivo | Descrição | Quando Usar |
|---------|-----------|-------------|
| **[DIAGRAMA_EQUIPAMENTO_COMPARTILHADO.txt](DIAGRAMA_EQUIPAMENTO_COMPARTILHADO.txt)** | Diagramas visuais da estrutura | Para entender arquitetura |

### 3️⃣ Exemplos Práticos

| Arquivo | Descrição | Quando Usar |
|---------|-----------|-------------|
| **[EXEMPLOS_USO_EQUIPAMENTO_COMPARTILHADO.md](EXEMPLOS_USO_EQUIPAMENTO_COMPARTILHADO.md)** | Casos de uso com código | Para implementação |

### 4️⃣ Scripts de Implementação

| Arquivo | Tipo | Descrição |
|---------|------|-----------|
| **[criar-equipamento-compartilhado.sql](criar-equipamento-compartilhado.sql)** | SQL | Script completo de criação |
| **[testar-equipamento-compartilhado.sql](testar-equipamento-compartilhado.sql)** | SQL | Testes automatizados |
| **[aplicar-equipamento-compartilhado.ps1](aplicar-equipamento-compartilhado.ps1)** | PowerShell | Script de aplicação |

## 🚀 Início Rápido

### Para Tomadores de Decisão

1. Leia o **[RESUMO_EQUIPAMENTO_COMPARTILHADO.md](RESUMO_EQUIPAMENTO_COMPARTILHADO.md)** (5 minutos)
2. Veja o **[DIAGRAMA_EQUIPAMENTO_COMPARTILHADO.txt](DIAGRAMA_EQUIPAMENTO_COMPARTILHADO.txt)** (5 minutos)
3. Decida se aprova a implementação

### Para Arquitetos/Tech Leads

1. Leia a **[PROPOSTA_EQUIPAMENTO_COMPARTILHADO.md](PROPOSTA_EQUIPAMENTO_COMPARTILHADO.md)** (20 minutos)
2. Analise os diagramas
3. Valide a arquitetura proposta

### Para Desenvolvedores

1. Leia a proposta completa
2. Estude os **[EXEMPLOS_USO_EQUIPAMENTO_COMPARTILHADO.md](EXEMPLOS_USO_EQUIPAMENTO_COMPARTILHADO.md)**
3. Execute os scripts de teste
4. Implemente seguindo os exemplos

## 📊 O Que Será Implementado

### Banco de Dados

```
✅ Nova coluna: equipamentos.compartilhado (BOOLEAN)
✅ Nova tabela: equipamento_usuarios_compartilhados
✅ Índices de performance (6 índices)
✅ 2 Views otimizadas
✅ 2 Funções com validações
✅ 1 Trigger de segurança
```

### Backend (C#)

```
📝 Modelo: EquipamentoUsuarioCompartilhado
📝 Mapeamento: EquipamentoUsuarioCompartilhadoMap
📝 Lógica: EquipamentoNegocio (novos métodos)
📝 Endpoints: 5 novos endpoints de API
📝 DTOs e ViewModels
```

### Frontend

```
📝 Componente: ListaUsuariosCompartilhados
📝 Modal: AdicionarUsuarioCompartilhado
📝 Toggle: HabilitarCompartilhamento
📝 Filtros e badges
```

## ⏱️ Estimativa de Tempo

| Fase | Atividade | Tempo |
|------|-----------|-------|
| **1** | Backend (SQL + Modelos + API) | 2-3 dias |
| **2** | Frontend (Componentes + Telas) | 2-3 dias |
| **3** | Testes e Ajustes | 1 dia |
| | **TOTAL** | **5-7 dias** |

## 🛠️ Como Aplicar

### Passo 1: Revisar Documentação

```bash
# Ler os documentos na ordem:
1. RESUMO_EQUIPAMENTO_COMPARTILHADO.md
2. PROPOSTA_EQUIPAMENTO_COMPARTILHADO.md
3. DIAGRAMA_EQUIPAMENTO_COMPARTILHADO.txt
4. EXEMPLOS_USO_EQUIPAMENTO_COMPARTILHADO.md
```

### Passo 2: Testar Scripts SQL

```powershell
# Executar apenas testes (não modifica banco)
.\aplicar-equipamento-compartilhado.ps1 -TestarApenas
```

### Passo 3: Aplicar no Banco

```powershell
# Aplicar estrutura completa
.\aplicar-equipamento-compartilhado.ps1

# Ou aplicar com testes
.\aplicar-equipamento-compartilhado.ps1 -ComTestes
```

### Passo 4: Implementar Backend

Seguir exemplos em:
- `PROPOSTA_EQUIPAMENTO_COMPARTILHADO.md` (seção "Modelos C# Propostos")
- `EXEMPLOS_USO_EQUIPAMENTO_COMPARTILHADO.md` (seção "Exemplos de Código Backend")

### Passo 5: Implementar Frontend

Seguir sugestões em:
- `PROPOSTA_EQUIPAMENTO_COMPARTILHADO.md` (seção "Interface Frontend")
- `DIAGRAMA_EQUIPAMENTO_COMPARTILHADO.txt` (seção "Fluxo de Tela")

## 📋 Estrutura Criada

### Banco de Dados

#### Tabela: `equipamentos` (modificada)
- ➕ Coluna `compartilhado` (BOOLEAN)

#### Tabela: `equipamento_usuarios_compartilhados` (nova)
```sql
- id (PK)
- equipamento_id (FK)
- colaborador_id (FK)
- data_inicio
- data_fim (nullable)
- ativo
- tipo_acesso (compartilhado/temporario/turno)
- observacao
- criado_por (FK)
- criado_em
```

#### Views
- `vw_equipamentos_compartilhados` - Listagem simplificada
- `vw_equipamentos_usuarios_compartilhados` - Dados detalhados

#### Funções
- `fn_adicionar_usuario_compartilhado()` - Com validações
- `fn_remover_usuario_compartilhado()` - Inativa registro

#### Triggers
- `trg_validar_equipamento_compartilhado` - Validações automáticas

### API Endpoints

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/equipamento/{id}/marcar-compartilhado` | Habilita compartilhamento |
| `GET` | `/api/equipamento/{id}/usuarios-compartilhados` | Lista usuários |
| `POST` | `/api/equipamento/usuario-compartilhado` | Adiciona usuário |
| `DELETE` | `/api/equipamento/usuario-compartilhado/{id}` | Remove usuário |
| `GET` | `/api/equipamento/compartilhados` | Lista equipamentos |

## ✅ Benefícios

### Técnicos
- ✅ Não quebra código existente
- ✅ Performance otimizada (índices adequados)
- ✅ Segue padrões do sistema (inativa ao invés de deletar)
- ✅ Código limpo e manutenível
- ✅ Escalável para futuras melhorias

### Negócio
- ✅ Melhor controle de recursos compartilhados
- ✅ Rastreabilidade completa de acessos
- ✅ Histórico de responsabilidades
- ✅ Relatórios mais precisos

### Usuários
- ✅ Interface simples e intuitiva
- ✅ Visibilidade clara de responsabilidades
- ✅ Gestão facilitada de acessos temporários
- ✅ Diferenciação entre tipos de uso

## 🔒 Regras de Negócio

1. ✅ Responsável principal é obrigatório (campo `usuario`)
2. ✅ Flag `compartilhado = true` habilita usuários compartilhados
3. ✅ Não permite duplicatas (mesmo colaborador ativo no equipamento)
4. ✅ Inativa ao invés de deletar (padrão do sistema)
5. ✅ Registra no histórico todas as operações
6. ✅ Valida datas (fim > início, temporário requer data fim)

## 📈 Métricas Sugeridas

- Total de equipamentos compartilhados
- Total de usuários compartilhados ativos
- Equipamentos mais compartilhados
- Colaboradores com mais equipamentos
- Acessos temporários expirando
- Distribuição por tipo de acesso

## 🧪 Testes Incluídos

O script `testar-equipamento-compartilhado.sql` inclui:

1. ✅ Criação de equipamento compartilhado
2. ✅ Adição de múltiplos usuários
3. ✅ Consultas usando views
4. ✅ Validação de duplicatas
5. ✅ Inativação de usuários
6. ✅ Validação de equipamento não compartilhado
7. ✅ Estatísticas gerais
8. ✅ Análise de performance

## ⚠️ Considerações Importantes

1. **Migração**: Equipamentos existentes terão `compartilhado = false` por padrão
2. **Permissões**: Definir quem pode marcar como compartilhado
3. **Notificações**: Avaliar notificar usuários quando adicionados/removidos
4. **Relatórios**: Atualizar relatórios existentes
5. **Termo de Responsabilidade**: Avaliar necessidade de termo específico
6. **Dashboard**: Adicionar métricas de compartilhamento

## 🎓 Casos de Uso Detalhados

Veja **[EXEMPLOS_USO_EQUIPAMENTO_COMPARTILHADO.md](EXEMPLOS_USO_EQUIPAMENTO_COMPARTILHADO.md)** para:

- 📱 Caso 1: Notebook de Time de Desenvolvimento
- 🔬 Caso 2: Equipamento Temporário para Pesquisa
- ⚙️ Caso 3: Recurso por Turnos
- 🖥️ Caso 4: Responsabilidade Compartilhada

## 📞 Suporte e Dúvidas

Para dúvidas técnicas, consulte:

| Tipo de Dúvida | Documento |
|----------------|-----------|
| Visão geral | Este README |
| Decisão de negócio | RESUMO_EQUIPAMENTO_COMPARTILHADO.md |
| Detalhes técnicos | PROPOSTA_EQUIPAMENTO_COMPARTILHADO.md |
| Arquitetura | DIAGRAMA_EQUIPAMENTO_COMPARTILHADO.txt |
| Implementação | EXEMPLOS_USO_EQUIPAMENTO_COMPARTILHADO.md |
| Scripts SQL | criar-equipamento-compartilhado.sql |
| Testes | testar-equipamento-compartilhado.sql |

## ✨ Próximos Passos

### 1. Aprovação
- [ ] Revisar proposta com equipe técnica
- [ ] Validar cenários de uso com stakeholders
- [ ] Aprovar implementação

### 2. Planejamento
- [ ] Definir prioridade no backlog
- [ ] Criar tasks no gerenciador de projetos
- [ ] Alocar desenvolvedores

### 3. Implementação
- [ ] Aplicar scripts SQL
- [ ] Implementar backend
- [ ] Implementar frontend
- [ ] Criar testes
- [ ] Atualizar documentação

### 4. Deploy
- [ ] Testar em ambiente de desenvolvimento
- [ ] Testar em homologação
- [ ] Deploy em produção
- [ ] Treinar usuários

## 📝 Histórico de Versões

| Versão | Data | Descrição |
|--------|------|-----------|
| 1.0 | 03/10/2025 | Proposta inicial completa |

## 🏆 Autoria

**Proposta criada por:** AI Assistant (Claude)  
**Solicitada por:** Equipe SingleOne  
**Data:** 03 de outubro de 2025

---

## 🎯 Conclusão

Esta proposta oferece uma solução **simples, eficiente e escalável** para gerenciar equipamentos compartilhados, atendendo múltiplos cenários de uso sem comprometer a estrutura existente do sistema.

**Complexidade:** ⭐⭐ Média  
**Impacto:** ⭐⭐⭐⭐ Alto  
**Recomendação:** ✅ Aprovação sugerida

---

📧 **Para mais informações, consulte os documentos listados acima.**

