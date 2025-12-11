import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Token expirado ou inválido - mas não redirecionar se for rota pública
          if (!this.isPublicRoute(request.url) && !this.isPublicPath(this.router.url)) {
            localStorage.removeItem('token');
            this.router.navigate(['/login']);
          } else {
            console.log('[ERROR-INTERCEPTOR] ✅ 401 em rota pública, não redirecionando');
          }
        } else if (error.status === 403) {
          // Acesso negado
          console.error('Acesso negado:', error);
        } else if (error.status === 500) {
          // Erro interno do servidor
          console.error('Erro interno do servidor:', error);
        }
        
        return throwError(error);
      })
    );
  }

  private isPatrimonioRoute(url: string): boolean {
    return url.includes('/MeuPatrimonio') || url.includes('/patrimonio');
  }

  private isPublicRoute(url: string): boolean {
    if (!url) return false;
    
    // Extrair apenas o path da URL
    let path = url;
    try {
      if (url.includes('://')) {
        const urlObj = new URL(url);
        path = urlObj.pathname;
      }
    } catch (e) {
      path = url;
    }
    
    // Normalizar path
    if (path.startsWith('/api/')) {
      path = path.substring(4);
    }
    if (!path.startsWith('/')) {
      path = '/' + path;
    }
    
    const publicRoutes = [
      '/requisicoes/ListarEquipamentosDaRequisicao',
      '/requisicoes/AceitarTermoResponsabilidade',
      '/configuracoes/BuscarLogoCliente',
      '/TermosPublicos/validacao',
      '/MeuPatrimonio',
      '/patrimonio'
    ];
    
    return publicRoutes.some(route => path.includes(route)) || this.isPatrimonioRoute(url);
  }

  private isPublicPath(path: string | undefined | null): boolean {
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
