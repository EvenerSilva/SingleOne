import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export interface LocationData {
  ip: string;
  country: string;
  city: string;
  region: string;
  latitude: number;
  longitude: number;
  timestamp: Date;
  accuracy?: number;
}

export interface GeolocationPosition {
  latitude: number;
  longitude: number;
  accuracy: number;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class GeolocationService {

  constructor(private http: HttpClient) { }

  /**
   * Obtém o IP público do usuário usando um serviço gratuito
   */
  getUserIP(): Observable<string> {
    return this.http.get('https://api.ipify.org?format=json').pipe(
      map((response: any) => response.ip),
      catchError(error => {
        console.error('Erro ao obter IP:', error);
        return this.http.get('https://jsonip.com').pipe(
          map((response: any) => response.ip),
          catchError(() => {
            // Fallback - retorna IP local se não conseguir obter o público
            return ['127.0.0.1'];
          })
        );
      })
    );
  }

  /**
   * Obtém informações de geolocalização baseadas no IP
   */
  getLocationByIP(ip: string): Observable<any> {
    return this.http.get(`https://ipapi.co/${ip}/json/`).pipe(
      catchError(error => {
        console.error('Erro ao obter localização por IP:', error);
        // Fallback para outro serviço
        return this.http.get(`http://ip-api.com/json/${ip}`).pipe(
          map((response: any) => ({
            country: response.country,
            city: response.city,
            region: response.regionName,
            latitude: response.lat,
            longitude: response.lon
          })),
          catchError(() => {
            return [{
              country: 'Não disponível',
              city: 'Não disponível',
              region: 'Não disponível',
              latitude: 0,
              longitude: 0
            }];
          })
        );
      })
    );
  }

  /**
   * Obtém a geolocalização precisa do usuário usando GPS/Wi-Fi
   */
  getCurrentPosition(): Observable<GeolocationPosition> {
    if (!navigator.geolocation) {
      throw new Error('Geolocalização não é suportada neste navegador');
    }

    return from(new Promise<GeolocationPosition>((resolve, reject) => {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          resolve({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
            accuracy: position.coords.accuracy,
            timestamp: new Date()
          });
        },
        (error) => {
          reject(error);
        },
        {
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 300000
        }
      );
    }));
  }

  /**
   * Obtém dados completos de localização (IP + GPS)
   */
  getCompleteLocationData(): Observable<LocationData> {
    return new Observable(observer => {
      this.getUserIP().subscribe(
        ip => {
          this.getLocationByIP(ip).subscribe(
            locationData => {
              const completeData: LocationData = {
                ip: ip,
                country: locationData.country || 'Não disponível',
                city: locationData.city || 'Não disponível',
                region: locationData.region || 'Não disponível',
                latitude: locationData.latitude || 0,
                longitude: locationData.longitude || 0,
                timestamp: new Date()
              };
              observer.next(completeData);
              observer.complete();
            },
            error => {
              observer.error(error);
            }
          );
        },
        error => {
          observer.error(error);
        }
      );
    });
  }

  /**
   * Formata os dados de localização para exibição
   */
  formatLocationForDisplay(location: LocationData): string {
    const parts = [];
    
    if (location.city && location.city !== 'Não disponível') {
      parts.push(location.city);
    }
    
    if (location.region && location.region !== 'Não disponível') {
      parts.push(location.region);
    }
    
    if (location.country && location.country !== 'Não disponível') {
      parts.push(location.country);
    }
    
    return parts.length > 0 ? parts.join(', ') : 'Localização não disponível';
  }

  /**
   * Formata as coordenadas para exibição
   */
  formatCoordinates(location: LocationData): string {
    if (location.latitude && location.longitude) {
      return `${location.latitude.toFixed(6)}, ${location.longitude.toFixed(6)}`;
    }
    return 'Coordenadas não disponíveis';
  }
}
