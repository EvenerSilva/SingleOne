# Tela de Custos de Manutenção - Padrão do Projeto

## Visão Geral
Esta tela foi modernizada seguindo **exatamente o padrão visual e estrutural** usado no projeto SingleOne, especificamente o mesmo estilo da tela de Fabricantes. Mantém toda a funcionalidade original mas com interface consistente e profissional.

## 🎯 **Padrão Visual Aplicado**

### **Cores e Identidade Visual**
- **Header roxo**: Gradiente `#080039` → `#1a1a2e` (padrão do projeto)
- **Accent laranja**: `#FF3A0F` para botões e elementos de destaque
- **Tons neutros**: `#f8f9fa`, `#e9ecef` para backgrounds e bordas
- **Texto escuro**: `#080039` para títulos e elementos principais

### **Estrutura Consistente**
- **Header com gradiente roxo** e ícone laranja flutuante
- **Breadcrumb moderno** com navegação hierárquica
- **Layout responsivo** com max-width de 1400px
- **Cards com sombras** e bordas arredondadas (16px)
- **Tabelas com headers** em gradiente claro

## 🚀 **Funcionalidades Implementadas**

### **Header Moderno**
- **Gradiente roxo** com sombra profunda
- **Ícone laranja** com animação flutuante
- **Botão de voltar** com hover effects
- **Botão de ação** laranja para exportar

### **Navegação Intuitiva**
- **Breadcrumb** com ícones CoreUI
- **Links ativos** destacados em roxo
- **Hover effects** em laranja

### **Filtros Expandíveis**
- **Container colapsável** com toggle button
- **Grid responsivo** para campos de filtro
- **Seleção em cascata** (Centro de Custo depende da Empresa)
- **Botões de ação** com gradientes consistentes

### **Dashboard de Métricas**
- **4 cards principais** com gradientes distintos:
  - 🔵 **Roxo**: Total de manutenções
  - 🟢 **Verde**: Custo total
  - 🔵 **Azul**: Custo médio
  - 🟡 **Amarelo**: Grupos de análise
- **Hover effects** com transform e sombra
- **Ícones contextuais** com opacidade

### **Sistema de Tabs**
- **Header de tabs** com gradiente claro
- **Botões de tab** com estados ativo/inativo
- **Indicador visual** roxo para tab ativo
- **Transições suaves** entre tabs

### **Gráfico Interativo**
- **Chart.js moderno** com cores do projeto
- **Duplo eixo** para quantidade vs valor
- **Cores consistentes**: Laranja e roxo
- **Responsivo** e interativo

### **Tabelas Profissionais**
- **Headers em gradiente** claro
- **Hover effects** com transform
- **Ícones contextuais** em roxo
- **Badges coloridos** para valores
- **Paginação centralizada**

### **Campo de Pesquisa**
- **Design consistente** com outras telas
- **Ícone de busca** posicionado
- **Botão de limpar** com hover
- **Focus states** em roxo

## 🛠️ **Tecnologias e Padrões**

### **Frontend Consistente**
- **Ícones CoreUI** (`cil-*`) em toda a interface
- **Gradientes CSS** para profundidade visual
- **Animações CSS** com keyframes
- **Transições suaves** em todos os elementos

### **Estrutura do Código**
- **Componente refatorado** seguindo padrões do projeto
- **Interfaces tipadas** para TypeScript
- **Métodos organizados** por responsabilidade
- **Lifecycle hooks** adequados

### **Estilos SCSS**
- **Arquitetura modular** igual à tela de fabricantes
- **Variáveis de cor** consistentes com o projeto
- **Mixins responsivos** para mobile/tablet
- **Animações e transições** padronizadas

## 📱 **Responsividade**

### **Breakpoints Consistentes**
- **Desktop**: > 768px (layout completo)
- **Tablet**: 768px (grid adaptativo)
- **Mobile**: < 480px (layout vertical)

### **Adaptações Mobile**
- **Header vertical** em dispositivos pequenos
- **Grid de métricas** em coluna única
- **Tabs verticais** em mobile
- **Tabelas scrolláveis** horizontalmente

## 🎨 **Elementos Visuais**

### **Gradientes e Sombras**
- **Header**: `rgba(8, 0, 57, 0.3)` com blur
- **Cards**: `rgba(0, 0, 0, 0.08)` para profundidade
- **Botões**: Gradientes laranja e roxo
- **Hover effects**: Transform + sombra aumentada

### **Animações**
- **Float**: Ícone do header flutuando
- **Spin**: Loading spinner rotativo
- **Hover**: Transform + sombra
- **Transições**: 0.3s ease em todos os elementos

### **Ícones e Badges**
- **CoreUI Icons** em toda a interface
- **Badges coloridos** para métricas
- **Avatars circulares** para usuários/empresas
- **Ícones contextuais** em headers de tabela

## 🔧 **Configuração e Uso**

### **Pré-requisitos**
- Angular 12+ (mesmo do projeto)
- CoreUI Icons (já incluído)
- Chart.js 3+ (para gráficos)
- Estilos SCSS (compilação automática)

### **Como Testar**
1. **Acesse**: `http://localhost:4200/relatorios/custos-de-manutencao`
2. **Verifique header roxo** com gradiente
3. **Teste breadcrumb** com navegação
4. **Explore filtros** expandindo/contraindo
5. **Interaja com métricas** (hover effects)
6. **Navegue pelas tabs** Dashboard/Listagem
7. **Teste responsividade** redimensionando

## 📊 **Comparação com Padrão**

### **✅ Seguindo o Padrão**
- **Cores**: Roxo `#080039` e laranja `#FF3A0F`
- **Layout**: Max-width 1400px, padding 2rem
- **Estrutura**: Header + Breadcrumb + Content
- **Componentes**: Cards, tabelas, botões
- **Responsividade**: Breakpoints 768px e 480px
- **Animações**: Float, hover, transições

### **🎯 Consistência Visual**
- **Mesmo header** da tela de fabricantes
- **Mesmo breadcrumb** com ícones
- **Mesmas sombras** e bordas
- **Mesmos gradientes** e cores
- **Mesmos hover effects** e animações

## 🚀 **Próximas Melhorias**

### **Funcionalidades**
- [ ] Exportação para Excel/PDF
- [ ] Filtros por período de data
- [ ] Gráficos adicionais (pizza, linha)
- [ ] Comparação entre períodos
- [ ] Alertas e notificações

### **Técnicas**
- [ ] Lazy loading de dados
- [ ] Cache de filtros
- [ ] Otimização de performance
- [ ] Testes unitários completos

## 📁 **Arquivos Modificados**

- ✅ `manutencoes-com-valor.component.ts` - Componente refatorado
- ✅ `manutencoes-com-valor.component.html` - Template padronizado
- ✅ `manutencoes-com-valor.component.scss` - Estilos consistentes
- ✅ `manutencoes-com-valor.component.spec.ts` - Testes atualizados
- ✅ `README.md` - Documentação atualizada

## 🎉 **Resultado Final**

A tela agora é **100% consistente** com o padrão visual do projeto SingleOne, oferecendo:

- **Experiência unificada** com outras telas
- **Design profissional** e moderno
- **Funcionalidade completa** mantida
- **Responsividade total** para todos os dispositivos
- **Performance otimizada** com animações suaves
- **Acessibilidade** com navegação por teclado

---

*Última atualização: Dezembro 2024*
*Versão: 2.0.0 - Padrão do Projeto*
*Status: ✅ Consistente com Fabricantes*
