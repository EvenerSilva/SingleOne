import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private router: Router) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('token');
    if (token) {
      // Clonar a requisição e adicionar o header de autorização
      const authRequest = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      
      // Processar a requisição e capturar erros
      return next.handle(authRequest).pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401) {
            localStorage.removeItem('token');
            localStorage.removeItem('usuario');
            
            // Redirecionar para login apenas se não estiver em rota pública
            if (!this.isPublicPath(this.router.url)) {
              this.router.navigate(['/login']);
            }
            
            // Exibir mensagem para o usuário
            console.warn('[AUTH-INTERCEPTOR] Usuário redirecionado para login devido a sessão expirada');
          }
          
          return throwError(error);
        })
      );
    } else {
      // Sem token: permitir livremente chamadas feitas a partir de rotas públicas
      // (ex.: /portaria, /patrimonio, /verificar-termo, /termos), mesmo que a URL da API
      // não esteja na lista de rotas públicas.
      const isPublicRoute = this.isPublicRoute(request.url);
      const isPublicPath = this.isPublicPath(this.router.url);
      
      if (!isPublicRoute && !isPublicPath) {
        console.log(`[AUTH-INTERCEPTOR] ⚠️ Bloqueando requisição sem token: ${request.url}`);
        console.log(`[AUTH-INTERCEPTOR] Rota pública? ${isPublicRoute}, Path público? ${isPublicPath}`);
        console.log(`[AUTH-INTERCEPTOR] Router URL atual: ${this.router.url}`);
        this.router.navigate(['/login']);
      } else {
        console.log(`[AUTH-INTERCEPTOR] ✅ Permitindo requisição pública: ${request.url}`);
      }
    }
    
    return next.handle(request);
  }

  private isPublicRoute(url: string): boolean {
    // Extrair apenas o path da URL (remover protocolo, domínio, porta, etc)
    let path = url;
    try {
      // Se for uma URL completa, extrair o path
      if (url.includes('://')) {
        const urlObj = new URL(url);
        path = urlObj.pathname;
      } else if (url.startsWith('http')) {
        // Tentar parsear mesmo sem protocolo explícito
        const match = url.match(/\/[^?]*/);
        if (match) {
          path = match[0];
        }
      }
    } catch (e) {
      // Se falhar, usar a URL original
      path = url;
    }
    
    // Normalizar path (remover /api/ se existir no início)
    if (path.startsWith('/api/')) {
      path = path.substring(4); // Remove '/api'
    }
    if (!path.startsWith('/')) {
      path = '/' + path;
    }
    
    const publicRoutes = [
      '/login',
      '/esqueci-senha',
      '/validar-token',
      '/two-factor-verification',
      '/PassCheck',
      '/MeuPatrimonio',
      '/patrimonio',
      // liberar página pública e API pública de verificação de termos
      '/verificar-termo',
      '/TermosPublicos/validacao',
      // APIs públicas para termos de responsabilidade (acesso anônimo)
      '/requisicoes/ListarEquipamentosDaRequisicao',
      '/requisicoes/AceitarTermoResponsabilidade',
      // API pública para buscar logo do cliente
      '/configuracoes/BuscarLogoCliente'
    ];
    
    // Verificar se o path corresponde a alguma rota pública
    const isPublic = publicRoutes.some(route => {
      // Verificar match exato ou se o path começa com a rota
      return path === route || path.startsWith(route + '/') || path.includes(route);
    });
    
    if (isPublic) {
      console.log(`[AUTH-INTERCEPTOR] ✅ Rota pública detectada: ${path}`);
    }
    
    return isPublic;
  }

  /**
   * Verifica se a rota atual do Angular é pública (não exige login).
   * Isso permite que páginas como /portaria e /patrimonio funcionem
   * sem autenticação, mesmo que façam chamadas para APIs diversas.
   */
  private isPublicPath(path: string | undefined | null): boolean {
    if (!path) {
      return false;
    }

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
