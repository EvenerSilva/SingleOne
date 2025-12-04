import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UtilService } from 'src/app/util/util.service';
import { RequisicaoApiService } from 'src/app/api/requisicoes/requisicao-api.service';

// Declaração do tipo GeolocationPosition para TypeScript
interface GeolocationPosition {
  coords: {
    latitude: number;
    longitude: number;
    accuracy: number;
  };
  timestamp: number;
}

@Component({
  selector: 'app-termos',
  templateUrl: './termos.component.html',
  styleUrls: ['./termos.component.scss']
})
export class TermosComponent implements OnInit {

  private session:any = {};
  isByod: boolean;
  public form: FormGroup;
  public termo:any = {};
  public equipamentos:any = {
    requisicao: {
      colaboradorfinal: '',
      assinaturaeletronica: false,
    },
    equipamentosRequisicao: []
  };

  // Dados de geolocalização
  public locationData: any = {
    latitude: null,
    longitude: null,
    accuracy: null,
    timestamp: null,
    city: null,
    region: null,
    country: null
  };
  public ipAddress: string = '';
  public geolocationStatus: string = 'capturando'; // 'capturando', 'sucesso', 'negado', 'erro', 'fallback'
  public showLocationInfo: boolean = false;

  constructor(private fb: FormBuilder, private util: UtilService, private api: RequisicaoApiService, private ar: ActivatedRoute, private cdr: ChangeDetectorRef, private router: Router) {
    this.form = this.fb.group({
      cpf: ['', Validators.required],
      palavrachave: ['', Validators.required]
    })
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.ar.paramMap.subscribe(param => {
      var parametro = param.get('hash');
      this.isByod = param.get('isByod') === 'true';
      if(parametro != null) {
        this.termo.hashRequisicao = parametro;
        this.listarEquipamentos();
      }
    })
    
    // Capturar geolocalização e IP
    this.captureLocationAndIP();
  }

  async captureLocationAndIP(): Promise<void> {
    try {
      this.geolocationStatus = 'capturando';
      
      // Capturar IP
      try {
        const ipResponse = await fetch('https://api.ipify.org?format=json');
        const ipData = await ipResponse.json();
        this.ipAddress = ipData.ip;
      } catch (ipError) {
        console.warn('⚠️ Erro ao capturar IP:', ipError);
        this.ipAddress = 'Não informado';
      }
      
      // Capturar geolocalização
      if (navigator.geolocation) {
        try {
          const position = await new Promise<GeolocationPosition>((resolve, reject) => {
            navigator.geolocation.getCurrentPosition(resolve, reject, {
              enableHighAccuracy: true,
              timeout: 10000, // Reduzido para 10 segundos
              maximumAge: 300000
            });
          });
          this.locationData.latitude = position.coords.latitude;
          this.locationData.longitude = position.coords.longitude;
          this.locationData.accuracy = position.coords.accuracy;
          this.locationData.timestamp = new Date(position.timestamp);
          this.determineLocationByCoordinates(position.coords.latitude, position.coords.longitude);
          
          this.geolocationStatus = 'sucesso';
        } catch (geolocationError: any) {
          console.warn('⚠️ Erro ao obter geolocalização:', geolocationError);
          
          // Verificar tipo de erro
          if (geolocationError.code === 1) {
            this.geolocationStatus = 'negado';
          } else if (geolocationError.code === 2) {
            this.geolocationStatus = 'erro';
          } else if (geolocationError.code === 3) {
            this.geolocationStatus = 'timeout';
          } else {
            this.geolocationStatus = 'erro';
          }
          
          // Fallback: tentar obter localização via IP
          await this.getLocationByIPFallback();
        }
      } else {
        console.warn('⚠️ Geolocalização não suportada pelo navegador');
        this.geolocationStatus = 'erro';
        // Fallback: tentar obter localização via IP
        await this.getLocationByIPFallback();
      }
    } catch (error) {
      console.error('❌ Erro geral ao capturar dados:', error);
      this.geolocationStatus = 'erro';
      this.ipAddress = 'Não informado';
      this.locationData.city = 'Localização não disponível';
      this.locationData.region = 'Não informado';
      this.locationData.country = 'Brasil';
    }
  }

  private async getLocationByIPFallback(): Promise<void> {
    try {
      if (this.ipAddress && this.ipAddress !== 'Não informado') {
        this.geolocationStatus = 'fallback';
        
        // Usar uma API simples e confiável
        const response = await fetch(`https://ipapi.co/${this.ipAddress}/json/`);
        if (response.ok) {
          const data = await response.json();
          
          if (data.city) {
            this.locationData.city = data.city;
            this.locationData.region = data.region || 'Não informado';
            this.locationData.country = data.country === 'BR' ? 'Brasil' : (data.country || 'Brasil');
            return;
          }
        }
      }
      
      // Se não conseguir via IP, usar valores padrão
      this.locationData.city = 'Localização não disponível';
      this.locationData.region = 'Não informado';
      this.locationData.country = 'Brasil';
      this.geolocationStatus = 'erro';
      
    } catch (error) {
      console.warn('⚠️ Erro ao obter localização via IP:', error);
      this.locationData.city = 'Localização não disponível';
      this.locationData.region = 'Não informado';
      this.locationData.country = 'Brasil';
      this.geolocationStatus = 'erro';
    }
  }

  private determineLocationByCoordinates(lat: number, lon: number): void {
    if (lat >= -33.75 && lat <= 5.27) {
      if (lon >= -73.99 && lon <= -34.79) {
        this.locationData.city = 'Região Sudeste';
        this.locationData.region = 'SP/RJ/MG/ES';
      } else if (lon >= -34.79 && lon <= -34.47) {
        this.locationData.city = 'Região Nordeste';
        this.locationData.region = 'NE';
      } else if (lon >= -73.99 && lon <= -50.0) {
        this.locationData.city = 'Região Sul';
        this.locationData.region = 'RS/SC/PR';
      } else if (lon >= -74.0 && lon <= -50.0) {
        this.locationData.city = 'Região Centro-Oeste';
        this.locationData.region = 'GO/MT/MS/DF';
      } else {
        this.locationData.city = 'Região Norte';
        this.locationData.region = 'AM/PA/AC/RO/AP/RR/TO';
      }
    } else {
      this.locationData.city = 'Localização no Brasil';
      this.locationData.region = 'Brasil';
    }
    
    this.locationData.country = 'Brasil';
  }

  listarEquipamentos(){
    this.util.aguardar(true);
    this.api.listarEquipamentosDaRequisicao(this.termo.hashRequisicao, this.isByod).then(res => {
      this.util.aguardar(false);
      this.equipamentos = res.data;
      if(this.equipamentos.requisicao.assinaturaeletronica) {
        this.util.exibirMensagemToast('Termo assinado com sucesso.', 'n')
      }

    })
  }

  async salvar() {
    if(this.form.valid){
      if (this.geolocationStatus === 'erro' || this.geolocationStatus === 'negado') {
        await this.captureLocationAndIP();
      } else if (this.geolocationStatus === 'capturando') {
        await new Promise(resolve => setTimeout(resolve, 2000));
      }
      this.termo.ipAddress = this.ipAddress || 'Não informado';
      this.termo.country = this.locationData?.country || 'Brasil';
      this.termo.city = this.locationData?.city || 'Não informado';
      this.termo.region = this.locationData?.region || 'Não informado';
      this.termo.latitude = this.locationData?.latitude || null;
      this.termo.longitude = this.locationData?.longitude || null;
      this.termo.accuracy = this.locationData?.accuracy || null;
      this.termo.timestamp = this.locationData?.timestamp || new Date();
      this.termo.geolocationStatus = this.geolocationStatus; // Adicionar status para debug
      this.showLocationInfo = true;

      this.util.aguardar(true);
        this.api.aceitarTermoResponsabilidade(this.termo).then(res => {
        this.util.aguardar(false);
        var retorno:any = res.data;
          if(retorno.Status == "200.1") {
            this.util.exibirMensagemToast(retorno.Mensagem, 'n');
          }
          else {
            this.util.exibirMensagemToast(retorno.Mensagem, 'n');
              // Atualizar estado local imediatamente
              try {
                this.equipamentos.requisicao.assinaturaeletronica = true;
              } catch {}
              // Recarregar da API (considerando BYOD) e forçar detecção de mudanças
              this.listarEquipamentos();
              this.cdr.detectChanges();
          // Reforço: aguardar uma fração de segundo e revalidar para garantir propagação no backend
          setTimeout(() => {
            this.listarEquipamentos();
            this.cdr.detectChanges();
          }, 600);
          // Hard refresh de rota para garantir atualização visual (mesma URL)
          // ✅ CORREÇÃO: Usar '/' como rota intermediária em vez de '/refresh' inexistente
          setTimeout(() => {
            const target = `/termos/${this.termo.hashRequisicao}/${this.isByod}`;
            this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
              this.router.navigateByUrl(target, { replaceUrl: true });
            });
          }, 800);
              // ✅ MELHORADO: Notificar outras abas/janelas para atualizarem os dados do patrimônio
              try {
                const payload = { 
                  type: 'termo_assinado',
                  hash: this.termo.hashRequisicao, 
                  timestamp: new Date().toISOString(), 
                  isByod: this.isByod 
                };
                
                // 1. LocalStorage (compatibilidade com código legado)
                localStorage.setItem('termo_assinado_sucesso', 'true');
                localStorage.setItem('termo_assinado_payload', JSON.stringify(payload));
                try {
                  const channel = new BroadcastChannel('patrimonio_updates');
                  channel.postMessage(payload);
                  channel.close();
                } catch (bcError) {
                  console.warn('⚠️ [TERMOS] BroadcastChannel não suportado:', bcError);
                }
                
                // 3. Notificar a janela pai (se esta janela foi aberta por outra)
                if (window.opener && !window.opener.closed) {
                  try {
                    window.opener.postMessage({ type: 'termo_assinado', ...payload }, '*');
                  } catch (pmError) {
                    console.warn('⚠️ [TERMOS] Erro ao enviar postMessage:', pmError);
                  }
                }
              } catch (notifyError) {
                console.error('❌ [TERMOS] Erro ao notificar outras abas:', notifyError);
              }

          // Fallback final: recarregar a página após a mensagem de sucesso para garantir refresh completo
          setTimeout(() => {
            try {
              window.location.reload();
            } catch {}
          }, 1800);
          }
      }).catch(error => {
        this.util.aguardar(false);
        console.error('❌ Erro ao enviar termo:', error);
        this.util.exibirMensagemToast('Erro ao assinar o termo. Tente novamente.', 'n');
      });
    }
  }

  /**
   * Obtém mensagem amigável sobre o status da geolocalização
   */
  getLocationStatusMessage(): string {
    switch (this.geolocationStatus) {
      case 'capturando':
        return '🔄 Capturando sua localização...';
      case 'sucesso':
        return `✅ Localização capturada: ${this.locationData.city}, ${this.locationData.region}`;
      case 'negado':
        return `⚠️ Permissão de localização negada. Usando localização aproximada: ${this.locationData.city}`;
      case 'timeout':
        return `⏱️ Tempo esgotado para capturar localização. Usando localização aproximada: ${this.locationData.city}`;
      case 'fallback':
        return `📍 Localização aproximada via IP: ${this.locationData.city}, ${this.locationData.region}`;
      case 'erro':
        return '❌ Erro ao capturar localização. Usando dados padrão.';
      default:
        return '📍 Status de localização desconhecido.';
    }
  }

  /**
   * Obtém ícone para o status da geolocalização
   */
  getLocationStatusIcon(): string {
    switch (this.geolocationStatus) {
      case 'capturando':
        return 'refresh';
      case 'sucesso':
        return 'location_on';
      case 'negado':
        return 'location_off';
      case 'timeout':
        return 'schedule';
      case 'fallback':
        return 'my_location';
      case 'erro':
        return 'error';
      default:
        return 'help';
    }
  }

}
