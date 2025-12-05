// Declaração para TypeScript reconhecer window.__env
declare const window: any;

// Função para obter apiUrl do env.js injetado pelo Docker ou usar padrão
// IMPORTANTE: Esta função é executada no momento da importação do módulo
// O env.js é carregado no index.html, mas pode não estar disponível ainda
// Por isso, sempre retornamos '/api' como padrão (Nginx faz proxy)
function getApiUrl(): string {
  // Tentar ler do window.__env se disponível
  try {
    if (typeof window !== 'undefined' && window.__env && window.__env.apiUrl) {
      const apiUrl = window.__env.apiUrl;
      // Garantir que termina com /api (sem barra final)
      if (apiUrl && apiUrl.trim() !== '') {
        return apiUrl.endsWith('/api/') ? apiUrl.slice(0, -1) : (apiUrl.endsWith('/api') ? apiUrl : apiUrl + '/api');
      }
    }
  } catch (e) {
    // Ignorar erros e usar padrão
  }
  // Sempre usar URL relativa (Nginx faz proxy)
  return '/api';
}

export const environment = {
    production: true,
    apiUrl: getApiUrl()
};
