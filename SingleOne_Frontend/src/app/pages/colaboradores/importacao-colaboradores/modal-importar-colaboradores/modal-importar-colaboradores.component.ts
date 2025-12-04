import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ImportacaoColaboradoresService, ResultadoValidacaoColaboradores, ResultadoImportacaoColaboradores } from 'src/app/services/importacao-colaboradores.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-modal-importar-colaboradores',
  templateUrl: './modal-importar-colaboradores.component.html',
  styleUrls: ['./modal-importar-colaboradores.component.scss']
})
export class ModalImportarColaboradoresComponent implements OnInit {
  passoImportacao = 1;
  arquivoSelecionadoImport: File | null = null;
  uploadandoImportacao = false;
  importandoColaboradores = false;
  resultadoValidacaoImport: ResultadoValidacaoColaboradores | null = null;
  resultadoImportacaoFinal: ResultadoImportacaoColaboradores | null = null;
  loteAtualImport: string | null = null;
  baixandoErros = false;
  session: any = null;

  constructor(
    public dialogRef: MatDialogRef<ModalImportarColaboradoresComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private importacaoService: ImportacaoColaboradoresService,
    private util: UtilService
  ) {
    this.session = this.util.getSession('usuario');
  }

  ngOnInit(): void {
  }

  onArquivoSelecionadoImportacao(event: any): void {
    const files: FileList = event.target.files;
    
    if (!files || files.length === 0) {
      return;
    }

    // Se múltiplos arquivos foram selecionados, usar apenas o primeiro e avisar
    if (files.length > 1) {
      this.util.exibirMensagemToast('⚠️ Apenas um arquivo pode ser processado por vez. Processando o primeiro arquivo selecionado.', 5000);
    }

    const arquivo: File = files[0];

    const extensoesValidas = ['.xlsx', '.xls'];
    const extensao = arquivo.name.substring(arquivo.name.lastIndexOf('.')).toLowerCase();

    if (!extensoesValidas.includes(extensao)) {
      this.util.exibirMensagemToast('Formato de arquivo inválido. Use apenas arquivos Excel (.xlsx, .xls)', 5000);
      event.target.value = '';
      return;
    }

    const tamanhoMaximo = 10 * 1024 * 1024;
    if (arquivo.size > tamanhoMaximo) {
      this.util.exibirMensagemToast('Arquivo muito grande. Tamanho máximo: 10MB', 5000);
      event.target.value = '';
      return;
    }

    // Se já está processando, não permitir novo upload
    if (this.uploadandoImportacao || this.importandoColaboradores) {
      this.util.exibirMensagemToast('⚠️ Aguarde o processamento atual terminar antes de fazer um novo upload.', 5000);
      event.target.value = '';
      return;
    }

    this.arquivoSelecionadoImport = arquivo;
    
    // Resetar o input para permitir selecionar o mesmo arquivo novamente se necessário
    event.target.value = '';
    
    this.fazerUploadImportacao();
  }

  fazerUploadImportacao(): void {
    if (!this.arquivoSelecionadoImport) {
      this.util.exibirMensagemToast('Selecione um arquivo primeiro', 3000);
      return;
    }

    this.uploadandoImportacao = true;
    this.passoImportacao = 2;

    this.importacaoService.uploadArquivo(this.arquivoSelecionadoImport).subscribe({
      next: (resultado) => {
        this.resultadoValidacaoImport = resultado;
        this.loteAtualImport = resultado.loteId;
        this.uploadandoImportacao = false;

        if (resultado.podeImportar) {
          this.util.exibirMensagemToast('✅ ' + resultado.mensagem, 5000);
        } else {
          this.util.exibirMensagemToast('⚠️ ' + resultado.mensagem, 5000);
        }
      },
      error: (erro) => {
        this.uploadandoImportacao = false;
        this.passoImportacao = 1;
        const mensagem = erro.error?.mensagem || 'Erro ao processar arquivo';
        this.util.exibirMensagemToast('❌ ' + mensagem, 5000);
        console.error('Erro no upload:', erro);
      }
    });
  }

  confirmarImportacao(): void {
    if (!this.loteAtualImport || !this.resultadoValidacaoImport) {
      return;
    }

    const message =
      `Tem certeza que deseja confirmar esta importação?<br><br>` +
      `📊 <strong>Resumo:</strong><br>` +
      `• <strong>Total de colaboradores:</strong> ${this.resultadoValidacaoImport.totalRegistros}<br>` +
      (this.resultadoValidacaoImport.totalAtualizacoes && this.resultadoValidacaoImport.totalAtualizacoes > 0 ?
        `• <strong>Atualizações detectadas:</strong> ${this.resultadoValidacaoImport.totalAtualizacoes}<br>` : '') +
      (this.resultadoValidacaoImport.totalNovos && this.resultadoValidacaoImport.totalNovos > 0 ?
        `• <strong>Novos colaboradores:</strong> ${this.resultadoValidacaoImport.totalNovos}<br>` : '') +
      (this.resultadoValidacaoImport.totalSemAlteracao && this.resultadoValidacaoImport.totalSemAlteracao > 0 ?
        `• <strong>Sem movimentação:</strong> ${this.resultadoValidacaoImport.totalSemAlteracao}<br>` : '') +
      (this.resultadoValidacaoImport.novasEmpresas > 0 ?
        `• <strong>Empresas a criar:</strong> ${this.resultadoValidacaoImport.novasEmpresas}<br>` : '') +
      (this.resultadoValidacaoImport.novasLocalidades > 0 ?
        `• <strong>Localidades a criar:</strong> ${this.resultadoValidacaoImport.novasLocalidades}<br>` : '') +
      (this.resultadoValidacaoImport.novoscentrosCusto > 0 ?
        `• <strong>Centros de Custo a criar:</strong> ${this.resultadoValidacaoImport.novoscentrosCusto}<br>` : '') +
      (this.resultadoValidacaoImport.novasFiliais > 0 ?
        `• <strong>Filiais a criar:</strong> ${this.resultadoValidacaoImport.novasFiliais}<br>` : '') +
      `<br>⚠️ <strong>Atenção:</strong> Esta ação criará novos registros no banco de dados e não poderá ser desfeita.`;

    this.util.exibirMensagemPopUp(message, true).then(res => {
      if (res) {
        this.importandoColaboradores = true;
        this.passoImportacao = 3;

        this.importacaoService.confirmarImportacao(this.loteAtualImport!).subscribe({
          next: (resultado) => {
            this.resultadoImportacaoFinal = resultado;
            this.importandoColaboradores = false;
            this.passoImportacao = 4;

            this.util.exibirMensagemToast('✅ ' + resultado.mensagem, 5000);
            // Fechar modal após sucesso com delay para garantir que o backend finalizou
            setTimeout(() => {
              this.dialogRef.close({ sucesso: true, loteId: resultado.loteId, resultado });
            }, 3000);
          },
          error: (erro) => {
            this.importandoColaboradores = false;
            this.passoImportacao = 2;
            const mensagem = erro.error?.mensagem || 'Erro ao importar dados';
            this.util.exibirMensagemToast('❌ ' + mensagem, 5000);
            console.error('Erro na importação:', erro);
          }
        });
      }
    });
  }

  baixarErrosValidacao(): void {
    if (!this.loteAtualImport || this.baixandoErros) {
      return;
    }

    this.baixandoErros = true;
    const url = `${this.importacaoService.getUrlErros(this.loteAtualImport)}`;
    const token = this.session?.token;

    this.importacaoService.baixarErros(url, token).subscribe({
      next: (blob) => {
        const link = document.createElement('a');
        const objectUrl = window.URL.createObjectURL(blob);
        link.href = objectUrl;
        link.download = `erros_importacao_${this.loteAtualImport}.csv`;
        link.click();
        window.URL.revokeObjectURL(objectUrl);
        this.baixandoErros = false;
      },
      error: (erro) => {
        console.error('Erro ao baixar erros:', erro);
        this.util.exibirMensagemToast('❌ Não foi possível baixar o arquivo de erros.', 5000);
        this.baixandoErros = false;
      }
    });
  }

  cancelarImportacaoModal(): void {
    if (!this.loteAtualImport) {
      this.dialogRef.close();
      return;
    }

    const message =
      `Tem certeza que deseja cancelar esta importação?<br><br>` +
      `⚠️ <strong>Atenção:</strong> Os dados validados serão descartados e você precisará fazer o upload novamente.<br><br>` +
      `📋 <strong>Lote:</strong> ${this.loteAtualImport}`;

    this.util.exibirMensagemPopUp(message, true).then(res => {
      if (res) {
        this.importacaoService.cancelarImportacao(this.loteAtualImport!).subscribe({
          next: () => {
            this.util.exibirMensagemToast('ℹ️ Importação cancelada', 3000);
            this.dialogRef.close();
          },
          error: (erro) => {
            console.error('Erro ao cancelar:', erro);
            this.dialogRef.close();
          }
        });
      }
    });
  }

  baixarTemplate(): void {
    const url = this.importacaoService.getUrlTemplate();
    window.open(url, '_blank');
  }

  fechar(): void {
    this.dialogRef.close();
  }

  resetarImportacao(): void {
    this.passoImportacao = 1;
    this.arquivoSelecionadoImport = null;
    this.uploadandoImportacao = false;
    this.importandoColaboradores = false;
    this.resultadoValidacaoImport = null;
    this.resultadoImportacaoFinal = null;
    this.loteAtualImport = null;
  }
}

