import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TwoFactorVerificationRequest {
  userId: number;
  code: string;
  verificationType: 'totp' | 'backup' | 'email';
  storedCode?: string;
}

export interface TwoFactorSetupRequest {
  userId: number;
}

export interface TwoFactorVerificationResponse {
  success: boolean;
  usuario: any;
  token: string;
  message?: string;
}

@Injectable({
  providedIn: 'root'
})
export class TwoFactorAuthService {
  private apiUrl = environment.apiUrl;
  private isVerificationRequiredSubject = new BehaviorSubject<boolean>(false);
  private currentUserSubject = new BehaviorSubject<any>(null);

  public isVerificationRequired$ = this.isVerificationRequiredSubject.asObservable();
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  setVerificationRequired(required: boolean, usuario?: any): void {
    this.isVerificationRequiredSubject.next(required);
    if (usuario) {
      this.currentUserSubject.next(usuario);
    }
  }

  getCurrentUser(): any {
    return this.currentUserSubject.value;
  }

  isVerificationRequired(): boolean {
    return this.isVerificationRequiredSubject.value;
  }

  verifyCode(request: TwoFactorVerificationRequest): Observable<TwoFactorVerificationResponse> {
    return this.http.post<TwoFactorVerificationResponse>(`${this.apiUrl}/Usuario/VerifyTwoFactor`, request);
  }

  enableTwoFactor(request: TwoFactorSetupRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/Usuario/EnableTwoFactor`, request);
  }

  disableTwoFactor(userId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/Usuario/DisableTwoFactor`, userId);
  }

  generateTOTPSecret(): Observable<any> {
    return this.http.post(`${this.apiUrl}/Usuario/EnableTwoFactor`, {});
  }

  clearVerificationState(): void {
    this.isVerificationRequiredSubject.next(false);
    this.currentUserSubject.next(null);
  }

  resendCode(userId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/Usuario/SendTwoFactorCode`, { userId });
  }

  processLoginResponse(response: any): { requires2FA: boolean; user?: any; token?: string } {
    if (response.twoFactorRequired) {
      this.setVerificationRequired(true, response.usuario);
      return { requires2FA: true, user: response.usuario };
    } else {
      this.setVerificationRequired(false);
      return { requires2FA: false, token: response.token };
    }
  }
}
