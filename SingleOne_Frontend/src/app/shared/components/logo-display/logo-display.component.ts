import { Component, OnInit } from '@angular/core';
import { ConfiguracoesApiService } from '../../../api/configuracoes/configuracoes-api.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-logo-display',
  templateUrl: './logo-display.component.html',
  styleUrls: ['./logo-display.component.scss']
})
export class LogoDisplayComponent implements OnInit {
  public clienteLogo: string | null = null;
  public singleoneLogo = 'assets/Branding/logo.png';
  public singleoneLogoBranco = 'assets/Branding/logo_branco.png';
  public loading = true;

  constructor(private configuracoesApi: ConfiguracoesApiService) { }

  ngOnInit(): void {
    this.carregarLogoCliente();
  }

  private async carregarLogoCliente() {
    try {
      const response = await this.configuracoesApi.buscarLogoCliente();
      
      // A resposta do axios vem em response.data
      // O backend retorna: { Logo: "/api/logos/{fileName}", ClienteNome: "...", Mensagem: "..." }
      const logoData = response?.data;
      
      if (logoData && (logoData.Logo || logoData.logo)) {
        // A logo retornada é uma URL relativa como /api/logos/{fileName}
        let logoUrl = logoData.Logo || logoData.logo;
        
        // Se a URL já começa com /api/, verificar se precisa adicionar baseURL
        if (logoUrl && logoUrl.startsWith('/api/')) {
          // Se estiver em desenvolvimento (ng serve), usar baseURL completo
          if (!environment.production && environment.apiUrl) {
            // Remover /api do baseURL se existir e construir URL completa
            const baseUrl = environment.apiUrl.replace('/api', '');
            logoUrl = baseUrl + logoUrl;
          } else {
            // Em produção, manter URL relativa (nginx faz proxy)
          }
        }
        
        this.clienteLogo = logoUrl;
      } else {
        this.clienteLogo = null;
      }
    } catch (error) {
      console.error('[LOGO-DISPLAY] ❌ Erro ao carregar logo do cliente:', error);
      this.clienteLogo = null;
    } finally {
      this.loading = false;
    }
  }
}
