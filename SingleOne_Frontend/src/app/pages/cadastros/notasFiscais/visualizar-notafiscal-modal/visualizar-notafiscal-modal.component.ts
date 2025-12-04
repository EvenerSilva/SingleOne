import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-visualizar-notafiscal-modal',
  templateUrl: './visualizar-notafiscal-modal.component.html',
  styleUrls: ['./visualizar-notafiscal-modal.component.scss']
})
export class VisualizarNotafiscalModalComponent implements OnInit {

  @Input() notaFiscalId: number = 0;
  @Output() fechado = new EventEmitter<void>();

  public nf: any = null;
  public carregando = false;

  constructor(
    private util: UtilService,
    private api: ConfiguracoesApiService
  ) { }

  ngOnInit(): void {
    this.carregarNota();
  }

  carregarNota() {
    if (!this.notaFiscalId) {
      console.error('[VISUALIZAR-NF-MODAL] ID da nota fiscal não fornecido');
      return;
    }

    this.carregando = true;
    this.util.aguardar(true);
    
    this.api.visualizarNotaFiscal(this.notaFiscalId).then(res => {
      if (res && res.data) {
        this.nf = res.data;
      } else {
        console.error('[VISUALIZAR-NF-MODAL] Resposta inválida da API:', res);
        this.nf = null;
      }
      this.carregando = false;
      this.util.aguardar(false);
    }).catch(error => {
      console.error('[VISUALIZAR-NF-MODAL] Erro ao carregar nota fiscal:', error);
      this.nf = null;
      this.carregando = false;
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  fechar() {
    this.fechado.emit();
  }

  formatarData(data: string): string {
    if (!data) return '';
    const dataObj = new Date(data);
    return dataObj.toLocaleDateString('pt-BR');
  }
}
