import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';
import { UtilService } from 'src/app/util/util.service';

interface TinOneConfigItem {
  id?: number;
  cliente?: number;
  chave: string;
  valor: string;
  descricao: string;
  ativo: boolean;
}

@Component({
  selector: 'app-tinone-config',
  templateUrl: './tinone-config.component.html',
  styleUrls: ['./tinone-config.component.scss']
})
export class TinOneConfigComponent implements OnInit {
  
  configuracoes: TinOneConfigItem[] = [];
  loading = false;
  salvando = false;
  mensagemSucesso = '';
  mensagemErro = '';
  private session: any = {};

  constructor(
    private http: HttpClient,
    private router: Router,
    private util: UtilService
  ) {}

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.carregarConfiguracoes();
  }

  private getHeaders(): HttpHeaders {
    const token = this.session?.token || '';
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  carregarConfiguracoes(): void {
    this.loading = true;
    this.http.get<TinOneConfigItem[]>(`${environment.apiUrl}/tinone/configuracoes`, {
      headers: this.getHeaders()
    })
      .subscribe({
        next: (configs) => {
          this.configuracoes = configs && configs.length > 0 ? configs : [];
          if (this.configuracoes.length === 0) {
            this.criarConfiguracoesPadrao();
          }
          this.loading = false;
        },
        error: (err) => {
          console.error('[TinOne Config] Erro ao carregar configurações:', err);
          this.criarConfiguracoesPadrao();
          this.loading = false;
        }
      });
  }

  criarConfiguracoesPadrao(): void {
    this.configuracoes = [
      {
        chave: 'TINONE_HABILITADO',
        valor: 'true',
        descricao: 'Habilita/desabilita o assistente Oni o Sábio globalmente',
        ativo: true
      },
      {
        chave: 'TINONE_CHAT_HABILITADO',
        valor: 'true',
        descricao: 'Habilita funcionalidade de chat do Oni',
        ativo: true
      },
      {
        chave: 'TINONE_TOOLTIPS_HABILITADO',
        valor: 'true',
        descricao: 'Habilita tooltips contextuais nos campos',
        ativo: true
      },
      {
        chave: 'TINONE_GUIAS_HABILITADO',
        valor: 'false',
        descricao: 'Habilita guias passo-a-passo (em desenvolvimento)',
        ativo: true
      },
      {
        chave: 'TINONE_SUGESTOES_PROATIVAS',
        valor: 'false',
        descricao: 'Habilita sugestões proativas do Oni (beta)',
        ativo: true
      },
      {
        chave: 'TINONE_IA_HABILITADA',
        valor: 'false',
        descricao: 'Habilita processamento com IA/NLP (requer Ollama local)',
        ativo: true
      },
      {
        chave: 'TINONE_ANALYTICS',
        valor: 'true',
        descricao: 'Habilita coleta de analytics de uso do Oni',
        ativo: true
      },
      {
        chave: 'TINONE_DEBUG_MODE',
        valor: 'false',
        descricao: 'Modo debug para desenvolvimento do Oni',
        ativo: true
      },
      {
        chave: 'TINONE_POSICAO',
        valor: 'bottom-right',
        descricao: 'Posição do widget do Oni: inferior direito ou esquerdo',
        ativo: true
      },
      {
        chave: 'TINONE_COR_PRIMARIA',
        valor: '#4a90e2',
        descricao: 'Cor primária do Oni (hex)',
        ativo: true
      }
    ];
  }

  getConfigLabel(chave: string): string {
    const labels: { [key: string]: string } = {
      'TINONE_HABILITADO': 'Habilitar Oni o Sábio',
      'TINONE_CHAT_HABILITADO': 'Habilitar Chat',
      'TINONE_TOOLTIPS_HABILITADO': 'Habilitar Tooltips Contextuais',
      'TINONE_GUIAS_HABILITADO': 'Habilitar Guias Interativos',
      'TINONE_SUGESTOES_PROATIVAS': 'Habilitar Sugestões Proativas',
      'TINONE_IA_HABILITADA': 'Habilitar IA/NLP',
      'TINONE_ANALYTICS': 'Habilitar Analytics',
      'TINONE_DEBUG_MODE': 'Modo Debug',
      'TINONE_POSICAO': 'Posição do Widget',
      'TINONE_COR_PRIMARIA': 'Cor Primária'
    };
    return labels[chave] || chave;
  }

  isBoolean(chave: string): boolean {
    return chave !== 'TINONE_POSICAO' && chave !== 'TINONE_COR_PRIMARIA';
  }

  toggleConfig(config: TinOneConfigItem): void {
    if (this.isBoolean(config.chave)) {
      config.valor = config.valor === 'true' ? 'false' : 'true';
    }
  }

  salvarConfiguracoes(): void {
    this.salvando = true;
    this.mensagemSucesso = '';
    this.mensagemErro = '';
    this.http.post(`${environment.apiUrl}/tinone/configuracoes`, this.configuracoes, {
      headers: this.getHeaders()
    })
      .subscribe({
        next: (response) => {
          this.mensagemSucesso = 'Configurações salvas com sucesso!';
          this.salvando = false;
          
          // Recarregar configurações sem recarregar a página inteira
          setTimeout(() => {
            this.carregarConfiguracoes();
            // Notificar widget TinOne para recarregar configuração
            window.dispatchEvent(new Event('tinone-config-reload'));
          }, 1000);
        },
        error: (err) => {
          console.error('[TinOne Config] Erro completo:', err);
          console.error('[TinOne Config] Status:', err.status);
          console.error('[TinOne Config] Mensagem:', err.message);
          console.error('[TinOne Config] Erro do servidor:', err.error);
          
          let mensagemErro = 'Erro ao salvar configurações. ';
          if (err.status === 401) {
            mensagemErro += 'Sessão expirada. Faça login novamente.';
          } else if (err.status === 500) {
            mensagemErro += 'Erro no servidor. Verifique os logs.';
          } else if (err.error?.erro) {
            mensagemErro += err.error.erro;
          } else {
            mensagemErro += 'Tente novamente.';
          }
          
          this.mensagemErro = mensagemErro;
          this.salvando = false;
        }
      });
  }

  voltar(): void {
    this.router.navigate(['/configuracoes']);
  }

  getIcon(chave: string): string {
    const icons: { [key: string]: string } = {
      'TINONE_HABILITADO': 'cil-power-standby',
      'TINONE_CHAT_HABILITADO': 'cil-chat-bubble',
      'TINONE_TOOLTIPS_HABILITADO': 'cil-info',
      'TINONE_GUIAS_HABILITADO': 'cil-map',
      'TINONE_SUGESTOES_PROATIVAS': 'cil-lightbulb',
      'TINONE_IA_HABILITADA': 'cil-speedometer',
      'TINONE_ANALYTICS': 'cil-chart-line',
      'TINONE_DEBUG_MODE': 'cil-bug',
      'TINONE_POSICAO': 'cil-location-pin',
      'TINONE_COR_PRIMARIA': 'cil-color-palette'
    };
    return icons[chave] || 'cil-settings';
  }

  isBeta(chave: string): boolean {
    return chave === 'TINONE_SUGESTOES_PROATIVAS' || 
           chave === 'TINONE_GUIAS_HABILITADO' ||
           chave === 'TINONE_IA_HABILITADA';
  }

  isOniDesabilitado(): boolean {
    const config = this.configuracoes.find(c => c.chave === 'TINONE_HABILITADO');
    return config ? config.valor === 'false' : false;
  }

  /**
   * Verifica se uma configuração deve estar desabilitada
   * Configurações dependentes do mestre ficam desabilitadas quando ele está off
   */
  isConfigDesabilitada(chave: string): boolean {
    // O próprio mestre nunca está desabilitado
    if (chave === 'TINONE_HABILITADO') {
      return false;
    }

    // Se o Oni estiver desabilitado, todas as outras configs ficam desabilitadas
    return this.isOniDesabilitado();
  }
}

