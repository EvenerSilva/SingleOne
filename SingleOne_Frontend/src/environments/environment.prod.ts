// Declaração para TypeScript reconhecer window.__env
declare const window: any;

// Função para obter apiUrl do env.js injetado pelo Docker ou usar padrão
function getApiUrl(): string {
  // Se window.__env existe e tem apiUrl, usar ele
  if (typeof window !== 'undefined' && window.__env && window.__env.apiUrl) {
    const apiUrl = window.__env.apiUrl;
    // Garantir que termina com /api (sem barra final)
    return apiUrl.endsWith('/api/') ? apiUrl.slice(0, -1) : (apiUrl.endsWith('/api') ? apiUrl : apiUrl + '/api');
  }
  // Caso contrário, usar URL relativa (Nginx faz proxy)
  return '/api';
}

export const environment = {
    production: true,
    apiUrl: getApiUrl()
};
