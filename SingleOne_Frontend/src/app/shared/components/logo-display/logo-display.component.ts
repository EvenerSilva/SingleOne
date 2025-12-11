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
  
  onLogoError(event: any): void {
    console.error('[LOGO-DISPLAY] ❌ Erro ao carregar imagem da logo:', event);
    console.error('[LOGO-DISPLAY] ❌ URL da logo que falhou:', this.clienteLogo);
    // Não limpar logo em caso de erro (pode ser erro temporário)
  }
  
  onLogoLoad(): void {
    console.log('[LOGO-DISPLAY] ✅ Logo do cliente carregada com sucesso:', this.clienteLogo);
  }

  private async carregarLogoCliente() {
    try {
      console.log('[LOGO-DISPLAY] 🔍 Iniciando busca da logo...');
      const response = await this.configuracoesApi.buscarLogoCliente();
      
      console.log('[LOGO-DISPLAY] 📦 Resposta recebida:', response);
      console.log('[LOGO-DISPLAY] 📦 response.data:', response?.data);
      
      // A resposta do axios vem em response.data
      // O backend retorna: { Logo: "/api/logos/{fileName}", ClienteNome: "...", Mensagem: "..." }
      // O axios já extrai o body da resposta HTTP, então response.data já é o objeto JSON
      const logoData = response?.data;
      
      console.log('[LOGO-DISPLAY] 📦 logoData:', logoData);
      
      // Aceitar tanto maiúsculas quanto minúsculas, e priorizar logoUrl (com timestamp) se disponível
      if (logoData && (logoData.Logo || logoData.logo || logoData.LogoUrl || logoData.logoUrl)) {
        // Priorizar logoUrl (com timestamp) se disponível, senão usar Logo/logo
        let logoUrl = logoData.LogoUrl || logoData.logoUrl || logoData.Logo || logoData.logo;
        
        console.log('[LOGO-DISPLAY] 🔗 URL da logo (antes):', logoUrl);
        
        // Se a URL já começa com /api/, verificar se precisa adicionar baseURL
        if (logoUrl && logoUrl.startsWith('/api/')) {
          // Se estiver em desenvolvimento (ng serve), usar baseURL completo
          if (!environment.production && environment.apiUrl) {
            // Remover /api do baseURL se existir e construir URL completa
            const baseUrl = environment.apiUrl.replace('/api', '');
            logoUrl = baseUrl + logoUrl;
            console.log('[LOGO-DISPLAY] 🔗 URL da logo (desenvolvimento):', logoUrl);
          } else {
            // Em produção, manter URL relativa (nginx faz proxy)
            console.log('[LOGO-DISPLAY] 🔗 URL da logo (produção, relativa):', logoUrl);
          }
        }
        
        console.log('[LOGO-DISPLAY] ✅ Logo definida:', logoUrl);
        this.clienteLogo = logoUrl;
      } else {
        console.warn('[LOGO-DISPLAY] ⚠️ Nenhuma logo encontrada na resposta');
        console.warn('[LOGO-DISPLAY] ⚠️ logoData:', logoData);
        this.clienteLogo = null;
      }
    } catch (error) {
      console.error('[LOGO-DISPLAY] ❌ Erro ao carregar logo do cliente:', error);
      console.error('[LOGO-DISPLAY] ❌ Detalhes do erro:', error);
      this.clienteLogo = null;
    } finally {
      this.loading = false;
      console.log('[LOGO-DISPLAY] ✅ Carregamento finalizado. Logo:', this.clienteLogo);
    }
  }
}
