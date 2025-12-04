import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { Observable, Subject } from 'rxjs';
import { MessageboxComponent } from '../../pages/messagebox/messagebox.component';

@Injectable({
  providedIn: 'root'
})
export class UtilService {

  private logger = new Subject<boolean>();
  private loggedIn: boolean = false;
  private paginas: any = [];

  constructor(
    private spinner: NgxSpinnerService, 
    private toast: MatSnackBar, 
    private route: Router,
    private dialog: MatDialog
  ) { }

  public aguardar(carregando: boolean) {
    if (carregando) {
      this.spinner.show();
    } else {
      this.spinner.hide();
    }
  }

  public exibirMensagemToast(mensagem: string, duracao: number, classes?: string[]) {
    this.toast.open(mensagem, 'X', { 
      duration: duracao,
      panelClass: classes || []
    });
  }

  public exibirFalhaComunicacao() {
    this.toast.open('Falha de comunicação com o serviço', 'X', { duration: 5000 });
  }

  public exibirErro2FA(erro: any) {
    // Verificar se é um erro específico do 2FA global desabilitado
    if (erro?.response?.data?.CodigoErro === '2FA_GLOBAL_DESABILITADO') {
      const mensagem = '⚠️ 2FA não pode ser habilitado porque a funcionalidade está desabilitada globalmente para este cliente. Entre em contato com o administrador para ativar o 2FA nas configurações globais.';
      this.toast.open(mensagem, 'X', { 
        duration: 8000,
        panelClass: ['error-toast']
      });
    } else if (erro?.response?.data?.Mensagem) {
      // Exibir mensagem específica do backend
      this.toast.open(erro.response.data.Mensagem, 'X', { duration: 5000 });
    } else {
      // Fallback para mensagem genérica
      this.exibirFalhaComunicacao();
    }
  }

  public exibirMensagemPopUp(mensagem: string, exibeCancelar: boolean) {
    return new Promise((resolve, reject) => {
      const modal = this.dialog.open(MessageboxComponent, {
        width: '500px',
        data: {
          mensagem: mensagem,
          exibeCancelar: exibeCancelar
        }
      });

      modal.afterClosed().subscribe(r => {
        resolve(r);
      });
    });
  }

  groupBy(xs: any[], key: string) {
    return xs.reduce((rv, x) => {
      (rv[x[key]] = rv[x[key]] || []).push(x);
      return rv;
    }, {});
  }

  taLogado(): Observable<boolean> {
    return this.logger.asObservable();
  }

  public salvarSessao(nome: string, objeto: any) {
    localStorage.setItem(nome, JSON.stringify(objeto));
  }

  public getSession(nome: string) {
    return JSON.parse(localStorage.getItem(nome));
  }

  public sair() {
    localStorage.removeItem('usuario');
    this.route.navigate(['/']);
  }

  public registrarStatus(status: boolean) {
    this.loggedIn = status;
    this.logger.next(this.loggedIn);
  }

  private convertBase64ToArrayBuffer(base64: string) {
    if (base64.includes('data:application/pdf;base64,')) {
      const binary_string = window.atob(base64.replace('data:application/pdf;base64,', ''));
    } else {
      const binary_string = window.atob(base64);
    }
    const binary_string_clean = window.atob(base64.replace('data:application/pdf;base64,', ''));
    const len = binary_string_clean.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
      bytes[i] = binary_string_clean.charCodeAt(i);
    }
    return bytes.buffer;
  }

  abrirArquivoNovaJanela(data: any) {
    const blob = new Blob([this.convertBase64ToArrayBuffer(data)], { type: 'application/pdf' });
    const url = window.URL.createObjectURL(blob);
    window.open(url);
  }

  private _base64ToArrayBuffer(base64: string) {
    const binary_string = window.atob(base64);
    const len = binary_string.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
      bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes.buffer;
  }

  gerarDocumentoNovaGuia(data: any) {
    const blob = new Blob([this._base64ToArrayBuffer(data)], { type: 'application/pdf' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'documento.pdf';
    link.click();
    window.URL.revokeObjectURL(url);
  }

  temPermissao(url: string) {
    const usuario = this.getSession('usuario');
    if (usuario && usuario.paginas) {
      return usuario.paginas.some((pagina: any) => pagina.url === url);
    }
    return false;
  }

  semPermissao() {
    this.toast.open('Você não tem permissão para acessar esta página', 'X', { duration: 5000 });
  }

  montarMenuDeAcesso(usuario: any) {
    if (usuario && usuario.paginas) {
      this.paginas = usuario.paginas;
    }
  }
}
