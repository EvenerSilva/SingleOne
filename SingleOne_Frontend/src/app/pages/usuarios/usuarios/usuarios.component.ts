import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { UsuarioApiService } from 'src/app/api/usuarios/usuario-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-usuarios',
  templateUrl: './usuarios.component.html',
  styleUrls: ['./usuarios.component.scss']
})
export class UsuariosComponent implements OnInit, AfterViewInit {

  private session:any = {};
  public colunas = ['nome', 'email', 'adm', 'operador', 'consulta', 'twoFactorEnabled', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  
  // Getter para dados paginados
  get dadosPaginados(): any[] {
    if (!this.dataSource || !this.dataSource.paginator) {
      return this.dataSource?.data || [];
    }
    
    const startIndex = this.dataSource.paginator.pageIndex * this.dataSource.paginator.pageSize;
    const endIndex = startIndex + this.dataSource.paginator.pageSize;
    return this.dataSource.data.slice(startIndex, endIndex);
  }
  
  constructor(
    private util: UtilService, 
    private api: UsuarioApiService, 
    private route: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    try {
      this.session = this.util.getSession('usuario');
      if (!this.session || !this.session.usuario) {
        console.error('Sessão inválida ou usuário não encontrado');
        this.util.exibirFalhaComunicacao();
        return;
      }
      
      this.cliente = this.session.usuario.cliente || this.session.usuario.Cliente;
      this.dataSource = new MatTableDataSource<any>([]);
      
      this.resultado = this.consulta.valueChanges.pipe(
        debounceTime(1000),
        tap(value => this.buscar(value))
      );
      this.resultado.subscribe();
      this.listar();
    } catch (error) {
      console.error('Erro no ngOnInit usuarios:', error);
      this.util.exibirFalhaComunicacao();
    }
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  // 🔧 MÉTODO AUXILIAR PARA CONFIGURAR PAGINADOR
  private configurarPaginador() {
    if (!this.paginator || !this.dataSource) {
      return;
    }
    
    // CONFIGURAÇÃO SIMPLES E DIRETA
    this.dataSource.paginator = this.paginator;
    
    // CONFIGURAR TAMANHO INICIAL
    this.paginator.pageSize = 10;
    this.paginator.pageIndex = 0;
    
    // ADICIONAR LISTENER PARA MUDANÇAS
    this.paginator.page.subscribe(() => {
      // FORÇAR ATUALIZAÇÃO DA VIEW
      this.cdr.detectChanges();
      this.cdr.markForCheck();
    });
  }

  listar() {
    if (this.cliente === null || this.cliente === undefined) {
      console.error('❌ Cliente inválido:', this.cliente);
      this.util.exibirMensagemToast('Cliente inválido!', 5000);
      return;
    }
    
    this.util.aguardar(true);
    this.api.listarUsuarios(null, this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        console.error('❌ Erro ao listar usuarios:', res);
        this.util.exibirFalhaComunicacao();
      }
      else {
        if (res.data && res.data.length > 0) {
        }
        this.dataSource = new MatTableDataSource<any>(res.data || []);
        this.configurarPaginador();
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('❌ Erro na API usuarios:', err);
      console.error('❌ Status:', err.response?.status);
      console.error('❌ Data:', err.response?.data);
      console.error('❌ Headers:', err.response?.headers);
      console.error('❌ URL da requisição:', err.config?.url);
      console.error('❌ Método da requisição:', err.config?.method);
      console.error('❌ Headers da requisição:', err.config?.headers);
      this.util.exibirFalhaComunicacao();
    })
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarUsuarios(valor, this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.dataSource = new MatTableDataSource(res.data);
          this.configurarPaginador();
        }
      })
    }
    else {
      this.listar();
    }
  }

  limparBusca(): void {
    this.consulta.setValue('');
    this.listar();
  }

  editar(obj) {
    this.route.navigate(['/usuario', btoa(JSON.stringify(obj))]);
  }

  excluir(obj) {
    this.util.exibirMensagemPopUp(
      `Deseja realmente desativar o usuário?<br><br>` +
      `👤 <strong>Usuário:</strong> ${obj.nome}<br>` +
      `📧 <strong>Email:</strong> ${obj.email || 'N/A'}<br><br>` +
      `⚠️ <strong>Atenção:</strong> O usuário será marcado como inativo no sistema, mas os dados serão preservados para auditoria.<br><br>` +
      `💡 <strong>Para reativar:</strong> Acesse o banco de dados e altere a coluna "ativo" para true.`,
      true
    ).then(res => {
      if (res) {
        this.util.aguardar(true);
        this.api.desativarUsuario(obj.id, this.session.token).then(res => {
          this.util.aguardar(false);
          if(res.status != 200) {
            this.util.exibirFalhaComunicacao();
          }
          else {
            this.util.exibirMensagemToast('Usuário desativado com sucesso! Os dados foram preservados para auditoria.', 8000);
            this.listar();
          }
        }).catch(err => {
          this.util.aguardar(false);
          console.error('Erro ao desativar usuário:', err);
          
          // ✅ TRATAMENTO ESPECÍFICO PARA DIFERENTES TIPOS DE ERRO
          if (err.response?.status === 404) {
            this.util.exibirMensagemToast('⚠️ Endpoint de desativação não encontrado. Entre em contato com o administrador para implementar a funcionalidade de desativação no backend.', 8000);
          } else if (err.response?.status === 401) {
            this.util.exibirMensagemToast('Sessão expirada. Por favor, faça login novamente.', 5000);
            this.route.navigate(['/']);
          } else if (err.response?.status === 403) {
            this.util.exibirMensagemToast('Acesso negado. Você não tem permissão para desativar usuários.', 5000);
          } else {
            this.util.exibirFalhaComunicacao();
          }
        });
      }
    });
  }

  toggleTwoFactor(usuario: any, event: any) {
    // Prevenir mudança automática do toggle
    event.preventDefault();
    event.stopPropagation();
    const novoEstado = !usuario.twoFactorEnabled;
    const action = novoEstado ? 'habilitar' : 'desabilitar';
    const actionTitle = novoEstado ? 'Habilitar' : 'Desabilitar';
    const message = 
      `Tem certeza que deseja ${action} a autenticação de dois fatores (2FA)?<br><br>` +
      `👤 <strong>Usuário:</strong> ${usuario.nome}<br>` +
      `📧 <strong>Email:</strong> ${usuario.email || 'N/A'}<br><br>` +
      (novoEstado ? 
        `✅ <strong>O que acontecerá:</strong> O usuário precisará informar um código de verificação a cada login para maior segurança.` :
        `⚠️ <strong>O que acontecerá:</strong> O usuário não precisará mais informar código de verificação no login.`
      );
    this.util.exibirMensagemPopUp(message, true).then(res => {
      if (res) {
        this.util.aguardar(true);
      
      if (novoEstado) {
        this.api.enableTwoFactor(usuario.id, this.session.token).then(res => {
          this.util.aguardar(false);
          if (res.status === 200) {
            this.util.exibirMensagemToast(`2FA habilitado com sucesso para ${usuario.nome}!`, 5000);
            this.listar(); // Recarregar lista para atualizar status
          } else {
            this.util.exibirFalhaComunicacao();
            // Não precisa reverter pois não aplicamos a mudança ainda
          }
        }).catch(err => {
          this.util.aguardar(false);
          console.error('=== DEBUG ERRO 2FA ===');
          console.error('Erro completo:', err);
          console.error('Tipo do erro:', typeof err);
          console.error('Estrutura completa do erro:', JSON.stringify(err, null, 2));
          console.error('Response:', err.response);
          console.error('Response.data:', err.response?.data);
          console.error('Response.status:', err.response?.status);
          console.error('Response.data.mensagem:', err.response?.data?.mensagem);
          console.error('Response.data.codigoErro:', err.response?.data?.codigoErro);
          console.error('=== FIM DEBUG ===');
          
          // Tratamento específico para erro de 2FA global desabilitado
          if (err?.response?.data?.codigoErro === '2FA_GLOBAL_DESABILITADO') {
            const mensagem = '⚠️ 2FA não pode ser habilitado porque a funcionalidade está desabilitada globalmente para este cliente. Entre em contato com o administrador para ativar o 2FA nas configurações globais.';
            this.util.exibirMensagemToast(mensagem, 8000);
          } else if (err?.response?.data?.mensagem) {
            this.util.exibirMensagemToast(err.response.data.mensagem, 5000);
          } else {
            this.util.exibirFalhaComunicacao();
          }
          // Não precisa reverter pois não aplicamos a mudança ainda
        });
      } else {
        this.api.disableTwoFactor(usuario.id, this.session.token).then(res => {
          this.util.aguardar(false);
          if (res.status === 200) {
            this.util.exibirMensagemToast(`2FA desabilitado com sucesso para ${usuario.nome}!`, 5000);
            this.listar(); // Recarregar lista para atualizar status
          } else {
            this.util.exibirFalhaComunicacao();
            // Não precisa reverter pois não aplicamos a mudança ainda
          }
        }).catch(err => {
          this.util.aguardar(false);
          console.error('=== DEBUG ERRO 2FA DESABILITAR ===');
          console.error('Erro completo:', err);
          console.error('Tipo do erro:', typeof err);
          console.error('Estrutura completa do erro:', JSON.stringify(err, null, 2));
          console.error('Response:', err.response);
          console.error('Response.data:', err.response?.data);
          console.error('Response.status:', err.response?.status);
          console.error('Response.data.mensagem:', err.response?.data?.mensagem);
          console.error('Response.data.codigoErro:', err.response?.data?.codigoErro);
          console.error('=== FIM DEBUG ===');
          
          // Tratamento específico para erro de 2FA global desabilitado
          if (err?.response?.data?.codigoErro === '2FA_GLOBAL_DESABILITADO') {
            const mensagem = '⚠️ 2FA não pode ser desabilitado porque a funcionalidade está desabilitada globalmente para este cliente. Entre em contato com o administrador para ativar o 2FA nas configurações globais.';
            this.util.exibirMensagemToast(mensagem, 8000);
          } else if (err?.response?.data?.mensagem) {
            this.util.exibirMensagemToast(err.response.data.mensagem, 5000);
          } else {
            this.util.exibirFalhaComunicacao();
          }
          // Não precisa reverter pois não aplicamos a mudança ainda
        });
      }
      } else {
      }
    });
  }

  // Métodos de teste para debug
  testarAutenticacao() {
    if (this.session?.token) {
      this.api.listarUsuarios("null", this.cliente, this.session.token).then(res => {
        this.util.exibirMensagemToast('API funcionando!', 3000);
      }).catch(err => {
        console.error('❌ Erro na API:', err);
        this.util.exibirMensagemToast('Erro na API: ' + (err.response?.status || 'Desconhecido'), 5000);
      });
    } else {
      this.util.exibirMensagemToast('Sem token de autenticação!', 5000);
    }
  }

  debugSessao() {
    if (this.session?.usuario) {
    }
    
    this.util.exibirMensagemToast('Debug no console (F12)', 3000);
  }

  limparSessao() {
    this.util.exibirMensagemPopUp(
      `Deseja realmente limpar a sessão?<br><br>` +
      `⚠️ <strong>Atenção:</strong> Esta ação irá remover todos os dados da sessão atual e redirecionar para a tela de login.`,
      true
    ).then(res => {
      if (res) {
        localStorage.removeItem('usuario');
        this.session = {};
        this.util.exibirMensagemToast('Sessão limpa!', 3000);
        this.route.navigate(['/']);
      }
    });
  }

  fazerLogout() {
    this.util.exibirMensagemPopUp(
      `Deseja realmente fazer logout?<br><br>` +
      `⚠️ <strong>Atenção:</strong> Esta ação irá encerrar sua sessão atual e redirecionar para a tela de login.`,
      true
    ).then(res => {
      if (res) {
        this.util.sair();
      }
    });
  }

}
