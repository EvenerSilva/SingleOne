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
            
            // Redirecionar para login
            this.router.navigate(['/login']);
            
            // Exibir mensagem para o usuário
            console.warn('[AUTH-INTERCEPTOR] Usuário redirecionado para login devido a sessão expirada');
          }
          
          return throwError(error);
        })
      );
    } else {
      if (!this.isPublicRoute(request.url)) {
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
      '/TermosPublicos/validacao'
    ];
    return publicRoutes.some(route => url.includes(route));
  }
}
