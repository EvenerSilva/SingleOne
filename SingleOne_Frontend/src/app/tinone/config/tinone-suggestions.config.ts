/**
 * Configuração de sugestões contextuais do Oni o Sábio
 * Mapeamento de rotas para perguntas sugeridas
 */

export interface TinOneSuggestion {
  text: string;      // Texto exibido no botão
  query: string;     // Pergunta completa enviada ao backend
  icon?: string;     // Ícone opcional (CoreUI icons)
}

export interface TinOneSuggestionsConfig {
  [route: string]: TinOneSuggestion[];
}

/**
 * Mapeamento de rotas para sugestões contextuais
 */
export const TINONE_SUGGESTIONS: TinOneSuggestionsConfig = {
  
  // Contestações (Movimentações)
  '/movimentacoes/contestacoes': [
    { text: '❓ Como atender uma contestação?', query: 'como atender uma contestação de patrimônio?' },
    { text: '📝 O que é uma contestação?', query: 'o que é uma contestação de patrimônio?' },
    { text: '✅ Como aprovar?', query: 'como aprovar uma contestação?' },
    { text: '❌ Como recusar?', query: 'como recusar uma contestação?' }
  ],

  // Requisições
  '/requisicoes': [
    { text: '➕ Como criar requisição?', query: 'como criar uma nova requisição?' },
    { text: '👀 Como aprovar?', query: 'como aprovar uma requisição?' },
    { text: '📋 Consultar status', query: 'como consultar o status de uma requisição?' },
    { text: '❌ Como cancelar?', query: 'como cancelar uma requisição?' }
  ],

  '/requisicoes/nova': [
    { text: '📝 Como preencher?', query: 'como preencher uma nova requisição?' },
    { text: '👤 Quem pode solicitar?', query: 'quem pode solicitar equipamentos?' },
    { text: '⏱️ Quanto tempo demora?', query: 'quanto tempo demora para aprovar uma requisição?' }
  ],

  // Movimentações
  '/equipamentos/movimentacoes': [
    { text: '📦 Como entregar equipamento?', query: 'como fazer entrega de equipamento?' },
    { text: '↩️ Como devolver?', query: 'como fazer devolução de equipamento?' },
    { text: '🔄 Como transferir?', query: 'como transferir equipamento entre colaboradores?' },
    { text: '📄 Sobre o termo de responsabilidade', query: 'como funciona o termo de responsabilidade?' }
  ],

  // Garantias
  '/relatorios/gestao-garantias': [
    { text: '📊 Como funciona o relatório?', query: 'como funciona o relatório de gestão de garantias?' },
    { text: '⏰ Alertas de vencimento', query: 'como funcionam os alertas de garantia?' },
    { text: '📤 Como exportar?', query: 'como exportar dados de garantias?' },
    { text: '🔍 Como filtrar?', query: 'como filtrar equipamentos por garantia?' }
  ],

  // Equipamentos por Status
  '/relatorios/equipamentos-status': [
    { text: '📊 O que são os status?', query: 'quais são os status de equipamentos?' },
    { text: '📤 Como exportar?', query: 'como exportar relatório de equipamentos?' },
    { text: '🔍 Como usar filtros?', query: 'como usar os filtros de equipamentos?' }
  ],

  // BYOD
  '/equipamentos/byod': [
    { text: '📱 O que é BYOD?', query: 'o que é BYOD?' },
    { text: '➕ Como cadastrar?', query: 'como cadastrar um equipamento BYOD?' },
    { text: '📝 Sobre o termo BYOD', query: 'como funciona o termo de uso BYOD?' },
    { text: '🔐 Regras de acesso', query: 'quais as regras para dispositivos BYOD?' }
  ],

  // Estoque Mínimo
  '/configuracoes/estoque-minimo': [
    { text: '📦 Como funciona?', query: 'como funciona o estoque mínimo?' },
    { text: '➕ Como configurar?', query: 'como configurar estoque mínimo?' },
    { text: '🔔 Sobre os alertas', query: 'como funcionam os alertas de estoque mínimo?' }
  ],

  // Auditoria de Acessos
  '/relatorios/auditoria-acessos': [
    { text: '🔍 O que é auditado?', query: 'o que é registrado na auditoria de acessos?' },
    { text: '📊 Como consultar?', query: 'como consultar a auditoria de acessos?' },
    { text: '📤 Como exportar?', query: 'como exportar dados de auditoria?' }
  ],

  // Sinalizações de Suspeita
  '/relatorios/sinalizacao-suspeita': [
    { text: '⚠️ O que são sinalizações?', query: 'o que são sinalizações de suspeita?' },
    { text: '🔍 Como investigar?', query: 'como investigar uma sinalização de suspeita?' },
    { text: '✅ Como resolver?', query: 'como resolver uma sinalização de suspeita?' }
  ],

  // Colaboradores
  '/colaboradores': [
    { text: '➕ Como cadastrar?', query: 'como cadastrar um colaborador?' },
    { text: '📝 Campos obrigatórios', query: 'quais são os campos obrigatórios de colaborador?' },
    { text: '🔄 Como atualizar dados?', query: 'como atualizar dados de um colaborador?' }
  ],

  // Termo Eletrônico e Campanhas
  '/termo-eletronico': [
    { text: '📧 Enviar em Massa vs Campanha?', query: 'qual a diferença entre enviar termos em massa e criar uma campanha de assinatura?' },
    { text: '🎯 Como criar campanha?', query: 'como criar uma campanha de assinatura de termos?' },
    { text: '📊 Como acompanhar campanha?', query: 'como acompanhar o progresso de uma campanha de assinatura?' },
    { text: '🔄 Como reenviar termos?', query: 'como reenviar termos para colaboradores que não assinaram?' },
    { text: '📈 Sobre métricas de adesão', query: 'como funcionam as métricas e taxa de adesão das campanhas?' }
  ],

  // Equipamentos
  '/equipamentos': [
    { text: '➕ Como cadastrar?', query: 'como cadastrar um equipamento?' },
    { text: '📝 Campos obrigatórios', query: 'quais campos são obrigatórios para equipamentos?' },
    { text: '🏷️ Sobre patrimônio', query: 'como funciona o número de patrimônio?' },
    { text: '📦 Sobre tipos de equipamento', query: 'quais são os tipos de equipamento?' }
  ],

  // Dashboard
  '/dashboard': [
    { text: '📊 O que tem no Dashboard?', query: 'o que tem no módulo Dashboard?' },
    { text: '📈 O que são os KPIs?', query: 'quais são os KPIs principais do Dashboard?' },
    { text: '⚠️ Ações pendentes', query: 'o que são as ações pendentes?' },
    { text: '🔔 Como usar notificações?', query: 'como funcionam as notificações do Dashboard?' }
  ],

  // Movimentações de Colaboradores
  '/relatorios/movimentacoes-colaboradores': [
    { text: '📊 O que são as 4 abas?', query: 'quais são as 4 abas de movimentações de colaboradores?' },
    { text: '📤 Como exportar tudo?', query: 'como exportar todos os dados de movimentações?' },
    { text: '🔍 Como buscar?', query: 'como buscar movimentações de um colaborador?' },
    { text: '📈 O que significam os totalizadores?', query: 'o que são os totalizadores nas abas?' }
  ],

  // Custos de Manutenção
  '/relatorios/custos-de-manutencao': [
    { text: '📊 Como funciona?', query: 'como funciona o relatório de custos de manutenção?' },
    { text: '🔍 Filtros disponíveis', query: 'quais filtros estão disponíveis em custos de manutenção?' },
    { text: '📤 Como exportar?', query: 'como exportar dados de custos de manutenção?' },
    { text: '📈 Sobre as métricas', query: 'quais são as métricas de custos de manutenção?' }
  ],

  // Recursos
  '/recursos': [
    { text: '➕ Como cadastrar novo?', query: 'como cadastrar um novo recurso?' },
    { text: '🔍 Como filtrar?', query: 'como filtrar recursos por status ou tipo?' },
    { text: '📤 Como exportar?', query: 'como exportar lista de recursos?' },
    { text: '📊 Sobre os status', query: 'quais são os status de recursos?' }
  ],

  // Telecom
  '/telecom': [
    { text: '📞 O que é Telefonia?', query: 'o que é o módulo de Telefonia?' },
    { text: '📱 Linhas vs Equipamentos', query: 'qual a diferença entre linhas telefônicas e equipamentos?' },
    { text: '➕ Como cadastrar linha?', query: 'como cadastrar uma linha telefônica?' },
    { text: '📤 Importação em lote', query: 'como importar linhas telefônicas em lote?' }
  ],

  // Cadastros
  '/cadastros': [
    { text: '🏢 O que tem em Cadastros?', query: 'o que tem no módulo Cadastros?' },
    { text: '🏗️ Estrutura organizacional', query: 'como funciona a hierarquia de empresas e localidades?' },
    { text: '⚡ Cadastro rápido', query: 'como usar o cadastro rápido de empresa?' },
    { text: '📦 Estoque mínimo', query: 'como configurar estoque mínimo?' }
  ],

  // Configurações
  '/configuracoes': [
    { text: '⚙️ O que tem em Configurações?', query: 'o que tem no módulo Configurações?' },
    { text: '👥 Gestão de usuários', query: 'como gerenciar usuários do sistema?' },
    { text: '🔐 Sobre 2FA', query: 'como funciona a autenticação de dois fatores?' },
    { text: '🦉 Configurar Oni', query: 'o que são as configurações do Oni o Sábio?' }
  ],

  // Entregas e Devoluções
  '/movimentacoes/entregas-devolucoes': [
    { text: '📦 Como fazer entrega?', query: 'como fazer entrega de recurso?' },
    { text: '↩️ Como fazer devolução?', query: 'como fazer devolução de recurso?' },
    { text: '📄 Gerar termo', query: 'como gerar termo de responsabilidade?' },
    { text: '🔄 Como compartilhar?', query: 'como compartilhar recursos entre colaboradores?' }
  ]
};

/**
 * Obtém sugestões contextuais baseadas na rota atual
 */
export function getSuggestionsByRoute(route: string): TinOneSuggestion[] {
  // Tenta match exato primeiro
  if (TINONE_SUGGESTIONS[route]) {
    return TINONE_SUGGESTIONS[route];
  }

  // Tenta match parcial (para rotas dinâmicas como /equipamentos/123)
  const partialMatch = Object.keys(TINONE_SUGGESTIONS).find(key => 
    route.startsWith(key)
  );
  if (partialMatch) {
    return TINONE_SUGGESTIONS[partialMatch];
  }

  return [];
}

/**
 * Obtém o contexto amigável da rota para exibição
 */
export function getRouteContext(route: string): string | null {
  const contexts: { [key: string]: string } = {
    '/dashboard': '📊 Dashboard',
    '/movimentacoes/contestacoes': '📋 Contestações de Patrimônio',
    '/requisicoes': '📝 Requisições',
    '/requisicoes/nova': '➕ Nova Requisição',
    '/equipamentos/movimentacoes': '🔄 Movimentações',
    '/relatorios/movimentacoes-colaboradores': '📊 Movimentações de Colaboradores',
    '/relatorios/custos-de-manutencao': '💰 Custos de Manutenção',
    '/relatorios/gestao-garantias': '📊 Gestão de Garantias',
    '/relatorios/equipamentos-status': '📊 Equipamentos por Status',
    '/equipamentos/byod': '📱 BYOD',
    '/configuracoes/estoque-minimo': '📦 Estoque Mínimo',
    '/relatorios/auditoria-acessos': '🔍 Auditoria de Acessos',
    '/relatorios/sinalizacao-suspeita': '⚠️ Sinalizações de Suspeita',
    '/termo-eletronico': '📧 Termo Eletrônico e Campanhas',
    '/colaboradores': '👥 Colaboradores',
    '/equipamentos': '💻 Equipamentos',
    '/recursos': '💻 Recursos',
    '/telecom': '📞 Telefonia',
    '/cadastros': '🏢 Cadastros',
    '/configuracoes': '⚙️ Configurações',
    '/movimentacoes/entregas-devolucoes': '📦 Entregas e Devoluções'
  };

  // Match exato
  if (contexts[route]) {
    return contexts[route];
  }

  // Match parcial
  const partialMatch = Object.keys(contexts).find(key => 
    route.startsWith(key)
  );
  return partialMatch ? contexts[partialMatch] : null;
}

