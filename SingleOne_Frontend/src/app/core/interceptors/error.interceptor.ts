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
          // Token expirado ou inválido - mas não redirecionar se for patrimônio
          if (!this.isPatrimonioRoute(request.url)) {
            localStorage.removeItem('token');
            this.router.navigate(['/login']);
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
}
