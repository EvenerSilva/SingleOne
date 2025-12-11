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
      // (ex.: /portaria, /patrimonio, /verificar-termo), mesmo que a URL da API
      // não esteja na lista de rotas públicas.
      if (!this.isPublicRoute(request.url) && !this.isPublicPath(this.router.url)) {
        this.router.navigate(['/login']);
      }
    }
    
    return next.handle(request);
  }

  private isPublicRoute(url: string): boolean {
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
    return publicRoutes.some(route => url.includes(route));
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
