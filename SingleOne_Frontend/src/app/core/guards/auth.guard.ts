import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private router: Router) {}

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    
    // Verificar se o usuário está autenticado
    const token = localStorage.getItem('token');
    
    // Rota pública: verificar-termo
    if (state.url && state.url.startsWith('/verificar-termo')) {
      return true;
    }

    if (token) {
      return true;
    } else {
      // Redirecionar para login se não estiver autenticado
      this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }
  }
}
