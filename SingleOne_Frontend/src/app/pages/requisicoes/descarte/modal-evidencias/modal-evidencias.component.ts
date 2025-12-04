import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { EvidenciaApiService } from 'src/app/api/evidencias/evidencia-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-modal-evidencias',
  templateUrl: './modal-evidencias.component.html',
  styleUrls: ['./modal-evidencias.component.scss']
})
export class ModalEvidenciasComponent implements OnInit {

  public session: any = {};
  public evidencias: any[] = [];
  public arquivosSelecionados: File[] = [];
  public tipoProcesso: string = 'EVIDENCIAS_GERAIS';
  public descricao: string = '';
  public carregando: boolean = false;

  public tiposProcesso = [
    { valor: 'SANITIZACAO', label: 'Sanitização' },
    { valor: 'DESCARACTERIZACAO', label: 'Descaracterização' },
    { valor: 'PERFURACAO_DISCO', label: 'Perfuração de Disco' },
    { valor: 'EVIDENCIAS_GERAIS', label: 'Evidências Gerais' }
  ];

  constructor(
    public dialogRef: MatDialogRef<ModalEvidenciasComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private util: UtilService,
    private api: EvidenciaApiService
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.carregarEvidencias();
  }

  carregarEvidencias() {
    if (!this.session || !this.session.token) return;

    this.carregando = true;
    this.api.listarEvidencias(this.data.equipamento.id, this.session.token).then(res => {
      this.carregando = false;
      if (res.status === 200) {
        this.evidencias = res.data || [];
      }
    }).catch(() => {
      this.carregando = false;
    });
  }

  onFileSelected(event: any) {
    const files: FileList = event.target.files;
    if (files && files.length > 0) {
      // Adicionar novos arquivos à lista
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        
        // Validar tamanho (máx 10MB)
        if (file.size > 10 * 1024 * 1024) {
          this.util.exibirMensagemToast(`Arquivo ${file.name} muito grande (máx 10MB)`, 5000);
          continue;
        }

        // Validar tipo
        const extensoesPermitidas = ['jpg', 'jpeg', 'png', 'gif', 'pdf', 'webp'];
        const extensao = file.name.split('.').pop()?.toLowerCase();
        
        if (!extensao || !extensoesPermitidas.includes(extensao)) {
          this.util.exibirMensagemToast(`Tipo de arquivo não permitido: ${file.name}`, 5000);
          continue;
        }

        this.arquivosSelecionados.push(file);
      }
    }
  }

  removerArquivo(index: number) {
    this.arquivosSelecionados.splice(index, 1);
  }

  async uploadArquivos() {
    if (this.arquivosSelecionados.length === 0) {
      this.util.exibirMensagemToast('Selecione pelo menos um arquivo', 3000);
      return;
    }

    if (!this.session || !this.session.token) return;

    this.util.aguardar(true);
    
    let sucessos = 0;
    let erros = 0;

    for (const arquivo of this.arquivosSelecionados) {
      try {
        const res = await this.api.uploadEvidencia(
          this.data.equipamento.id,
          this.tipoProcesso,
          this.descricao,
          arquivo,
          this.session.token
        );
        if (res.status === 200) {
          sucessos++;
        } else {
          erros++;
        }
      } catch (err) {
        erros++;
      }
    }

    this.util.aguardar(false);

    if (sucessos > 0) {
      this.util.exibirMensagemToast(
        `${sucessos} arquivo(s) enviado(s) com sucesso!`, 
        5000
      );
      this.arquivosSelecionados = [];
      this.descricao = '';
      this.carregarEvidencias();
    }

    if (erros > 0) {
      this.util.exibirMensagemToast(
        `${erros} arquivo(s) com erro no envio`, 
        5000
      );
    }
  }

  downloadEvidencia(evidencia: any) {
    if (!this.session || !this.session.token) return;

    this.api.downloadEvidencia(evidencia.id, this.session.token).then(res => {
      if (res.status === 200) {
        // Criar URL e forçar download
        const url = window.URL.createObjectURL(new Blob([res.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', evidencia.nomearquivo);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      } else {
        this.util.exibirFalhaComunicacao();
      }
    });
  }

  excluirEvidencia(evidencia: any) {
    this.util.exibirMensagemPopUp('Tem certeza que deseja excluir esta evidência?', true).then(confirmado => {
      if (confirmado && this.session && this.session.token) {
        this.util.aguardar(true);
        this.api.excluirEvidencia(evidencia.id, this.session.token).then(res => {
          this.util.aguardar(false);
          if (res.status === 200) {
            this.util.exibirMensagemToast('Evidência excluída com sucesso', 3000);
            this.carregarEvidencias();
          } else {
            this.util.exibirFalhaComunicacao();
          }
        });
      }
    });
  }

  fechar() {
    this.dialogRef.close(this.evidencias.length);
  }

  formatarTamanho(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  formatarData(data: string): string {
    return new Date(data).toLocaleString('pt-BR');
  }

  getTipoProcessoLabel(tipo: string): string {
    const encontrado = this.tiposProcesso.find(t => t.valor === tipo);
    return encontrado ? encontrado.label : tipo;
  }

  isImagem(nomeArquivo: string): boolean {
    const extensao = nomeArquivo.split('.').pop()?.toLowerCase();
    return ['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(extensao || '');
  }
}

