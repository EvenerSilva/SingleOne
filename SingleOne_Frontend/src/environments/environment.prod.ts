// Declaração para TypeScript reconhecer window.__env
declare const window: any;

// Função para obter apiUrl do env.js injetado pelo Docker ou usar padrão
// IMPORTANTE: Esta função é executada no momento da importação do módulo
// O env.js é carregado no index.html, mas pode não estar disponível ainda
// Por isso, sempre retornamos '/api' como padrão (Nginx faz proxy)
function getApiUrl(): string {
  // SEMPRE retornar '/api' - Nginx faz proxy para o backend
  // Não importa se window.__env está disponível ou não
  return '/api';
}

export const environment = {
    production: true,
    apiUrl: getApiUrl()
};
