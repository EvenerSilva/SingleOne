import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { UtilService } from '../util/util.service';
import axios, { AxiosInstance } from 'axios';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ConfigApiService {

  protected instance = axios.create({
    baseURL: environment.apiUrl
  })

  constructor(private route: Router, protected util: UtilService) {
    this.instance.interceptors.request.use(config => {
      // Adicionar token de autorização automaticamente se disponível
      const token = localStorage.getItem('token');
      if (token && !config.url?.includes('login') && !config.url?.includes('SendTwoFactorCode') && !config.url?.includes('VerifyTwoFactor')) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      
      return config;
    });
    
    this.instance.interceptors.response.use(response => {
        return response;
      }, error => {
        if (error.response?.status == 401) {
          // Verificar se é uma rota pública (termos, patrimônio, etc)
          const currentPath = this.route.url || '';
          const isPublicRoute = this.isPublicRoute(error.config?.url) || 
                                this.isPublicPath(currentPath) ||
                                this.isPatrimonioRoute(error.config?.url);
          
          if (isPublicRoute) {
            // Rotas públicas podem retornar 401 sem redirecionar
            console.log('[CONFIG-API] ✅ 401 em rota pública, não redirecionando');
            return Promise.reject(error);
          }
          
          if(error.response.data == ""){
            this.util.exibirMensagemToast('Sessão expirada. Por favor, entre novamente', 5000);
            this.route.navigate(['/']);
          }
          else{
            this.util.exibirMensagemToast('Usuário/senha inválido', 5000);
            this.route.navigate(['/']);
          }
        }
        
        // Tratar especificamente o erro 2FA global desabilitado
        if (error.response?.data?.codigoErro === '2FA_GLOBAL_DESABILITADO') {
          // 2FA global desabilitado - não fazer nada especial
        }
        
        // ✅ NOVO: Tratar requisições abortadas (backend offline)
        if (error.code === 'ECONNABORTED' || error.message === 'Request aborted') {
          error.backendOffline = true;
          error.userMessage = 'Servidor não está respondendo. Verifique se o backend está rodando.';
        }
        
        // ✅ NOVO: Tratar erros de rede
        if (error.code === 'NETWORK_ERROR' || error.message.includes('Failed to fetch')) {
          error.networkError = true;
          error.userMessage = 'Erro de conexão. Verifique sua internet e se o servidor está rodando.';
        }
        
        return Promise.reject(error);
      });
  }

protected instanceCep = axios.create({
    baseURL: "https://viacep.com.br/ws/"
  })

  /**
   * Retorna a instância Axios configurada
   */
  public getInstance(): AxiosInstance {
    return this.instance;
  }

  /**
   * Verifica se a URL é de uma rota de patrimônio
   */
  private isPatrimonioRoute(url: string): boolean {
    return url && (url.includes('/MeuPatrimonio') || url.includes('/patrimonio'));
  }

  /**
   * Verifica se a URL da API é uma rota pública
   */
  private isPublicRoute(url: string | undefined): boolean {
    if (!url) return false;
    
    const publicRoutes = [
      '/requisicoes/ListarEquipamentosDaRequisicao',
      '/requisicoes/AceitarTermoResponsabilidade',
      '/configuracoes/BuscarLogoCliente',
      '/TermosPublicos/validacao',
      '/MeuPatrimonio',
      '/patrimonio'
    ];
    
    return publicRoutes.some(route => url.includes(route));
  }

  /**
   * Verifica se o path atual do Angular é público
   */
  private isPublicPath(path: string): boolean {
    if (!path) return false;
    
    const publicPaths = [
      '/login',
      '/esqueci-senha',
      '/two-factor-verification',
      '/patrimonio',
      '/portaria',
      '/verificar-termo',
      '/termos'
    ];
    
    return publicPaths.some(p => path.startsWith(p));
  }
}
