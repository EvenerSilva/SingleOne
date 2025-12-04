import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { Observable, Subject } from 'rxjs';
import { MessageboxComponent } from '../pages/messagebox/messagebox.component';

@Injectable({
  providedIn: 'root'
})
export class UtilService {

  private logger = new Subject<boolean>();
  private loggedIn:boolean = false;
  private paginas:any = [];
  
  // Observable para notificar mudanças na sessão
  private sessaoMudouSubject = new Subject<any>();
  public sessaoMudou = this.sessaoMudouSubject.asObservable();

  constructor(private spinner: NgxSpinnerService, private toast: MatSnackBar, private route: Router,
    private dialog: MatDialog) { }

  public aguardar(carregando:boolean) {
    if(carregando){
      // this.spinner.show("Aguarde", {type: "timer"});
      this.spinner.show();
    }
    else {
      this.spinner.hide();
    }
  }

  public exibirMensagemToast(mensagem:string, duracao) {
    this.toast.open(mensagem, 'X', {duration: duracao});
  }

  public exibirFalhaComunicacao() {
    this.toast.open('Falha de comunicação com o serviço', 'X', {duration: 5000});
  }

  public exibirMensagemPopUp(mensagem:string, exibeCancelar:boolean) {
    let these = this;
    return new Promise(function(resolve, reject) {
      const modal = these.dialog.open(MessageboxComponent, {
        width: '500px',
        data: {
          mensagem: mensagem,
          exibeCancelar: exibeCancelar
        }
      });

      modal.afterClosed().subscribe(r => {
        resolve(r);
      })
    })
  }

  groupBy = function(xs, key) {
    return xs.reduce(function(rv, x) {
      (rv[x[key]] = rv[x[key]] || []).push(x);
      return rv;
    }, {});
  };

  taLogado(): Observable<boolean> {
    return this.logger.asObservable();
  }

  public salvarSessao(nome:string, objeto:any){
    try {
      const jsonString = JSON.stringify(objeto);
      localStorage.setItem(nome, jsonString);
      const itemSalvo = localStorage.getItem(nome);
      if (nome === 'usuario') {
        this.sessaoMudouSubject.next(objeto);
      }
    } catch (error) {
      console.error(`[UTIL] Erro ao salvar sessão '${nome}':`, error);
    }
  }

  public getSession(nome:string) {
    try {
      const item = localStorage.getItem(nome);
      if (item) {
        return JSON.parse(item);
      }
      return null;
    } catch (error) {
      console.error('Erro ao ler sessão:', error);
      localStorage.removeItem(nome); // Remove item corrompido
      return null;
    }
  }

  public sair() {
    localStorage.removeItem('usuario');
    
    // Notificar mudança na sessão (usuário saiu)
    this.sessaoMudouSubject.next(null);
    
    // this.loggedIn = false;
    // this.logger.next(this.loggedIn);
    this.route.navigate(['/']);
  }

  public registrarStatus(status:boolean){
    this.loggedIn = status;
    this.logger.next(this.loggedIn);
  }

  private convertBase64ToArrayBuffer(base64) {
    if(base64.includes('data:application/pdf;base64,')) {
      var binary_string = window.atob(base64.replace('data:application/pdf;base64,', ''));
    }
    else {
      var binary_string = window.atob(base64);
    }
    var binary_string = window.atob(base64.replace('data:application/pdf;base64,', ''));
    var len = binary_string.length;
    var bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
        bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes.buffer;
  }

  abrirArquivoNovaJanela(data) {
    try {
      // Validar se data é uma string válida
      if (!data || typeof data !== 'string' || data.trim().length === 0) {
        console.error('[UTIL] Erro: Dados inválidos para abrir arquivo');
        this.exibirMensagemToast('Erro: Dados do arquivo inválidos', 5000);
        return;
      }

      // Remover prefixo data:application/pdf;base64, se existir
      let base64Data = data;
      if (data.includes(',')) {
        base64Data = data.split(',')[1];
      }

      // Validar formato Base64 básico
      const base64Regex = /^[A-Za-z0-9+/]*={0,2}$/;
      if (!base64Regex.test(base64Data)) {
        console.error('[UTIL] Erro: String não é Base64 válido');
        this.exibirMensagemToast('Erro: Formato de dados inválido', 5000);
        return;
      }

      const arr = this._base64ToArrayBuffer(base64Data);
      const blob = new Blob([arr], { type: 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      window.open(url);
    } catch (error) {
      console.error('[UTIL] Erro ao abrir arquivo:', error);
      this.exibirMensagemToast('Erro ao abrir arquivo PDF', 5000);
    }
  }

  private _base64ToArrayBuffer(base64) {
    try {
      var binary_string = window.atob(base64);
      var len = binary_string.length;
      var bytes = new Uint8Array(len);
      for (var i = 0; i < len; i++) {
          bytes[i] = binary_string.charCodeAt(i);
      }
      return bytes.buffer;
    } catch (error) {
      console.error('[UTIL] Erro ao converter Base64 para ArrayBuffer:', error);
      throw new Error('Erro ao decodificar Base64: ' + error.message);
    }
  }

  gerarDocumentoNovaGuia(data){
    if (data instanceof Blob) {
      try {
        // Se já é um Blob, usar diretamente
        const url = window.URL.createObjectURL(data);
        const newWindow = window.open(url, '_blank');
        if (newWindow) {
        } else {
          console.error('[UTIL] Popup bloqueado pelo navegador');
          // Fallback: download do arquivo
          const link = document.createElement('a');
          link.href = url;
          link.download = 'laudo-tecnico.pdf';
          link.click();
        }
      } catch (error) {
        console.error('[UTIL] Erro ao processar Blob:', error);
      }
    } else if (typeof data === 'string') {
      const arr = this.convertBase64ToArrayBuffer(data);
      const blob = new Blob([arr], { type: 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      window.open(url, '_blank');
    } else {
      console.error('[UTIL] Tipo de dados não suportado:', typeof data);
    }
  }

  temPermissao(url) {
    if(url != undefined) {
      url = url.toLowerCase();
      if(url != '/' && url != '/login' && url != "/esqueci-senha" && !url.includes('validar-token') && !url.includes('termos') && this.getSession('usuario') == null) {
        this.semPermissao();
      }
      else if(url != '/' && url != '/login'  && url != "/esqueci-senha" && !url.includes('validar-token') && !url.includes('termos') && this.getSession('usuario') != null) {
        var page = url.split('/')[1];
        var estaNoMenu = this.paginas.filter(x => {
          // return x.url == url && x.visivel
          return x.visivel && x.url.includes(page);
        });
        if(estaNoMenu.length == 0) {
          this.semPermissao();
        }
      }
    }
  }
  semPermissao() {
    localStorage.removeItem('usuario');
    this.exibirMensagemToast('Acesso negado.', 'x');
    this.route.navigate(['/login']);
  }

montarMenuDeAcesso(usuario) {
    try {
      if (!usuario) {
        console.error('[UTIL] Usuário é null ou undefined');
        return this.getMenuPadrao();
      }
      var appPages = [
        {
          title: 'Dashboard',
          url: '/dashboard',
          icon: 'dashboard_outline',
          visivel: true,
          listado: true
        },
        {
          title: 'Solicitações',
          url: '/movimentacoes',
          icon: 'sync_alt',
          visivel: !usuario.consulta,
          listado: true
        },
        {
          title: 'Colaboradores',
          url: '/colaboradores',
          icon: 'people',
          visivel: !usuario.consulta,
          listado: true
        },
        {
          title: 'Relatórios e Timeline',
          url: '/relatorios',
          icon: 'access_time',
          visivel: true,
          listado: true
        },
        {
          title: 'Cadastros',
          url: '/cadastros',
          icon: 'business',
          visivel: !usuario.consulta,
          listado: true
        },
        {
          title: 'Configurações',
          url: '/configuracoes',
          icon: 'build',
          visivel: !usuario.consulta,
          listado: true
        },
        /********************************************************************* */
        /****************** funcionalidades não listadas ***********************/
        /********************************************************************* */
        /******************* cadastros *************************************** */
        {
          url: '/nada-consta',
          visivel: !usuario.consulta,
          listado: false
          },
        {
          url: '/descarte',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/empresas',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/empresa',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/centros-custos',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/centro-custo',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/tipos-recursos',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/tipo-recurso',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/fabricantes',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/fabricante',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/modelos',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/modelo',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/telefonia',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/operadoras',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/operadora',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/contratos-telefonia',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/contrato-telefonia',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/planos',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/plano',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/linhas',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/linha',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/telecom/importar-linhas',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/laudos',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/abrir-laudo',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/encerrar-laudo',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/fornecedores',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/fornecedor',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/notas-fiscais',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/nota-fiscal',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/clientes',
          visivel: usuario.su,
          listado: false
        },
        {
          url: '/cliente',
          visivel: usuario.su,
          listado: false
        },
        {
          url: '/templates',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/template',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/usuarios',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/usuario',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/meu-usuario',
          visivel: true,
          listado: false
        },
        {
          url: '/equipamentos-status-detalhe',
          visivel: true,
          listado: false
        },
        /******************* recursos **************************************** */
        {
          url: '/recurso',
          visivel: !usuario.consulta,
          listado: false
        },
        {
          url: '/visualizar-recurso',
          visivel: !usuario.consulta,
          listado: false
        },
        /******************* colaboradores *********************************** */
        {
          url: '/colaborador',
          visivel:true,
          listado:false
        },
        {
          url: '/esqueci-senha',
          visivel: true,
          listado: false
        },
        {
          url: '/validar-token',
          visivel: true,
          listado: false
        },
        {
          url: '/parametros',
          visivel: true,
          listado: false
        },
      ]
      
      // Filtrar apenas páginas visíveis e listadas
      const menuFiltrado = appPages.filter(p => p.visivel && p.listado);
      this.paginas = appPages;
      return appPages;
      
    } catch (error) {
      console.error('[UTIL] Erro ao montar menu de acesso:', error);
      return this.getMenuPadrao();
    }
  }

  // Menu de emergência caso algo dê errado
  private getMenuPadrao() {
    return [
      {
        title: 'Dashboard',
        url: '/dashboard',
        icon: 'dashboard_outline',
        visivel: true,
        listado: true
      },
      {
        title: 'Configurações',
        url: '/cadastros',
        icon: 'build',
        visivel: true,
        listado: true
      },
      {
        title: 'Sair',
        url: '/login',
        icon: 'logout',
        visivel: true,
        listado: true
      }
    ];
  }
}
