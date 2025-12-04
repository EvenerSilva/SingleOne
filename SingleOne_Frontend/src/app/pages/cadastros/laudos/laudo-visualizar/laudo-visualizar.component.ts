import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-laudo-visualizar',
  templateUrl: './laudo-visualizar.component.html',
  styleUrls: ['./laudo-visualizar.component.scss', './laudo-visualizar.component.css']
})
export class LaudoVisualizarComponent implements OnInit, OnDestroy {

  @Input() laudo: any = {};
  @Input() mostrar: boolean = false;
  @Output() fechar = new EventEmitter<void>();

  public evidencias: any[] = [];
  public carregandoEvidencias = false;
  public editandoEvidencia: any = null;
  private recarregarEvidenciasHandler: any;
  public novaOrdem: number = 0;
  
  // Cache para miniaturas das evidências
  public evidenciasPreviews: Map<number, SafeUrl> = new Map();

  constructor(
    private api: ConfiguracoesApiService,
    private util: UtilService,
    private sanitizer: DomSanitizer
  ) { }

  ngOnInit(): void {
    // Só carregar evidências se o laudo for válido
    if (this.laudo && this.laudo.id && this.laudo.id > 0) {
      this.carregarEvidencias();
    }
    
    // Listener para recarregar evidências após upload
    this.recarregarEvidenciasHandler = (event: any) => {
      if (this.laudo && this.laudo.id && this.laudo.id > 0 && event.detail.laudoId === this.laudo.id) {
        this.carregarEvidencias();
      }
    };
    window.addEventListener('recarregarEvidencias', this.recarregarEvidenciasHandler);
  }
  
  ngOnDestroy() {
    // Remover listener para evitar memory leaks
    if (this.recarregarEvidenciasHandler) {
      window.removeEventListener('recarregarEvidencias', this.recarregarEvidenciasHandler);
    }
    
    // Limpar URLs de objetos para evitar memory leaks
    this.evidenciasPreviews.forEach(safeUrl => {
      // Extrair a URL original do SafeUrl para revogar
      const urlString = (safeUrl as any).changingThisBreaksApplicationSecurity;
      if (urlString && urlString.startsWith('blob:')) {
        URL.revokeObjectURL(urlString);
      }
    });
    this.evidenciasPreviews.clear();
  }

  carregarEvidencias() {
    // Verificar se o laudo é válido antes de fazer a requisição
    if (!this.laudo || !this.laudo.id || this.laudo.id <= 0) {
      return;
    }
    
    this.carregandoEvidencias = true;
    
    this.api.listarEvidenciasLaudo(this.laudo.id)
      .then(res => {
        this.carregandoEvidencias = false;
        if (res.status === 200) {
          this.evidencias = res.data || [];
          // Carregar miniaturas para evidências de imagem
          this.carregarMiniaturas();
        } else {
          this.util.exibirFalhaComunicacao();
        }
      })
      .catch(err => {
        this.carregandoEvidencias = false;
        console.error('Erro ao carregar evidências:', err);
        this.util.exibirFalhaComunicacao();
      });
  }

  iniciarEdicao(evidencia: any) {
    this.editandoEvidencia = { ...evidencia };
    this.novaOrdem = evidencia.ordem;
  }

  cancelarEdicao() {
    this.editandoEvidencia = null;
    this.novaOrdem = 0;
  }

  salvarEdicao() {
    if (!this.editandoEvidencia || this.novaOrdem <= 0) {
      this.util.exibirMensagemToast('Ordem inválida!', 3000);
      return;
    }

    this.util.aguardar(true);
    this.api.reordenarEvidenciasLaudo({
      laudoId: this.laudo.id,
      ordemEvidencias: [this.editandoEvidencia.id]
    })
      .then(res => {
        this.util.aguardar(false);
        if (res.status === 200) {
          this.util.exibirMensagemToast('Evidência reordenada com sucesso!', 3000);
          this.editandoEvidencia = null;
          this.novaOrdem = 0;
          this.carregarEvidencias(); // Recarregar lista
        } else {
          this.util.exibirFalhaComunicacao();
        }
      })
      .catch(err => {
        this.util.aguardar(false);
        console.error('Erro ao reordenar evidência:', err);
        this.util.exibirFalhaComunicacao();
      });
  }

  excluirEvidencia(evidencia: any) {
    if (confirm(`Deseja realmente excluir a evidência "${evidencia.nomearquivo}"?`)) {
      this.util.aguardar(true);
      this.api.excluirEvidenciaLaudo(evidencia.id)
        .then(res => {
          this.util.aguardar(false);
          if (res.status === 200) {
            this.util.exibirMensagemToast('Evidência excluída com sucesso!', 3000);
            this.carregarEvidencias(); // Recarregar lista
          } else {
            this.util.exibirFalhaComunicacao();
          }
        })
        .catch(err => {
          this.util.aguardar(false);
          console.error('Erro ao excluir evidência:', err);
          this.util.exibirFalhaComunicacao();
        });
    }
  }

  downloadEvidencia(evidencia: any) {
    this.util.aguardar(true);
    this.api.downloadEvidenciaLaudo(evidencia.id)
      .then(res => {
        this.util.aguardar(false);
        if (res.status === 200) {
          // Criar link de download
          const blob = new Blob([res.data], { type: 'application/octet-stream' });
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = evidencia.nomearquivo;
          link.click();
          window.URL.revokeObjectURL(url);
        } else {
          this.util.exibirFalhaComunicacao();
        }
      })
      .catch(err => {
        this.util.aguardar(false);
        console.error('Erro ao fazer download:', err);
        this.util.exibirFalhaComunicacao();
      });
  }

  adicionarEvidencias() {
    // Verificar se já atingiu o limite de 6 evidências
    if (this.evidencias.length >= 6) {
      this.util.exibirMensagemToast('Limite máximo de 6 evidências atingido!', 3000);
      return;
    }

    // Emitir evento para o componente pai abrir o modal
    this.fechar.emit();
    
    // Aguardar um pouco para o modal fechar e depois abrir o modal de evidências
    setTimeout(() => {
      // Disparar evento customizado para abrir modal de evidências
      const event = new CustomEvent('abrirModalEvidencias', {
        detail: { laudoId: this.laudo.id }
      });
      window.dispatchEvent(event);
    }, 300);
  }

  /* 🖼️ MÉTODOS PARA MINIATURAS */
  carregarMiniaturas() {
    this.evidencias.forEach(evidencia => {
      if (this.isImageFile(evidencia.nomearquivo)) {
        this.carregarMiniatura(evidencia);
      }
    });
  }

  carregarMiniatura(evidencia: any) {
    this.api.downloadEvidenciaLaudo(evidencia.id)
      .then(res => {
        if (res.status === 200 && res.data) {
          // Criar blob URL para a imagem
          const blob = new Blob([res.data], { type: 'image/*' });
          const imageUrl = URL.createObjectURL(blob);
          const safeUrl = this.sanitizer.bypassSecurityTrustUrl(imageUrl);
          this.evidenciasPreviews.set(evidencia.id, safeUrl);
        }
      })
      .catch(err => {
        console.error('Erro ao carregar miniatura:', err);
      });
  }

  isImageFile(fileName: string): boolean {
    if (!fileName) return false;
    const extension = fileName.toLowerCase().split('.').pop();
    return ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp'].includes(extension || '');
  }

  getFileIcon(fileName: string): string {
    if (!fileName) return 'cil-file';
    const extension = fileName.toLowerCase().split('.').pop();
    
    switch (extension) {
      case 'pdf': return 'cil-file-pdf';
      case 'doc':
      case 'docx': return 'cil-file-word';
      case 'xls':
      case 'xlsx': return 'cil-file-excel';
      case 'ppt':
      case 'pptx': return 'cil-file-presentation';
      case 'txt': return 'cil-file-text';
      case 'zip':
      case 'rar':
      case '7z': return 'cil-folder';
      default: return 'cil-file';
    }
  }

  getEvidenciaPreview(evidencia: any): SafeUrl | string {
    // Para evidências que são imagens, verificar cache primeiro
    if (this.isImageFile(evidencia.nomearquivo)) {
      const cachedUrl = this.evidenciasPreviews.get(evidencia.id);
      if (cachedUrl) {
        return cachedUrl;
      }
      
      // Se não estiver no cache, retornar placeholder temporário sanitizado
      const placeholderSvg = `data:image/svg+xml;base64,${btoa(`
        <svg width="60" height="60" xmlns="http://www.w3.org/2000/svg">
          <rect width="60" height="60" fill="#e3f2fd" stroke="#2196f3" stroke-width="2"/>
          <circle cx="30" cy="30" r="8" fill="#2196f3"/>
          <text x="30" y="50" font-family="Arial" font-size="8" text-anchor="middle" fill="#2196f3">
            Carregando...
          </text>
        </svg>
      `)}`;
      return this.sanitizer.bypassSecurityTrustUrl(placeholderSvg);
    }
    
    // Para arquivos não-imagem, retornar placeholder SVG sanitizado
    const filePlaceholderSvg = `data:image/svg+xml;base64,${btoa(`
      <svg width="60" height="60" xmlns="http://www.w3.org/2000/svg">
        <rect width="60" height="60" fill="#f8f9fa" stroke="#dee2e6" stroke-width="2"/>
        <text x="30" y="35" font-family="Arial" font-size="12" text-anchor="middle" fill="#6c757d">
          ${evidencia.nomearquivo.split('.').pop()?.toUpperCase() || 'FILE'}
        </text>
      </svg>
    `)}`;
    return this.sanitizer.bypassSecurityTrustUrl(filePlaceholderSvg);
  }

  formatFileSize(bytes: number): string {
    if (!bytes || bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getDisplayFileName(fileName: string, evidencia?: any): string {
    if (!fileName) return 'Arquivo';
    
    const extension = fileName.toLowerCase().split('.').pop();
    const ordem = evidencia?.ordem || '';
    
    // Se for imagem, mostrar "Evidência" + ordem + extensão
    if (this.isImageFile(fileName)) {
      return ordem ? `Evidência ${ordem} (${extension?.toUpperCase()})` : `Imagem ${extension?.toUpperCase() || ''}`;
    }
    
    // Para outros arquivos, mostrar tipo + ordem + extensão
    let tipo = '';
    switch (extension) {
      case 'pdf': tipo = 'PDF'; break;
      case 'doc':
      case 'docx': tipo = 'Word'; break;
      case 'xls':
      case 'xlsx': tipo = 'Excel'; break;
      case 'ppt':
      case 'pptx': tipo = 'PowerPoint'; break;
      case 'txt': tipo = 'Texto'; break;
      case 'zip':
      case 'rar':
      case '7z': tipo = 'Compactado'; break;
      default: tipo = extension?.toUpperCase() || 'Arquivo';
    }
    
    return ordem ? `Evidência ${ordem} (${tipo})` : `Arquivo ${tipo}`;
  }

  trackByEvidencia(index: number, evidencia: any): number {
    return evidencia.id;
  }

  fecharVisualizacao() {
    this.fechar.emit();
  }
}
