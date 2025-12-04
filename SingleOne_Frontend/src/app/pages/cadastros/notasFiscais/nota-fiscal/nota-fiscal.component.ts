import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { ContratoApiService } from 'src/app/api/contratos/contrato-api.service';
import { UtilService } from 'src/app/util/util.service';
import { MatDialog } from '@angular/material/dialog';
import { MessageboxComponent } from '../../../messagebox/messagebox.component';

@Component({
  selector: 'app-nota-fiscal',
  templateUrl: './nota-fiscal.component.html',
  styleUrls: ['./nota-fiscal.component.scss']
})
export class NotaFiscalComponent implements OnInit {

  private session:any = {};
  public nota:any = {
    id: 0,
    notasfiscaisitens: []
  };
  public notaItem:any = {};
  public fornecedores:any = [];
  public tipos:any = [];
  public fabricantes:any = [];
  public modelos:any = [];
  public contratos:any = [];
  public form: FormGroup;
  public colunas = ['produto', 'quantidade', 'valorunitario', 'valortotal', 'acao'];
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;

  // Controle do Stepper Moderno
  public step1Completed: boolean = false;
  public step2Active: boolean = false;
  public step2Completed: boolean = false;

  constructor(private fb: FormBuilder, 
              private util: UtilService, 
              private api: ConfiguracoesApiService,
              private apiContratos: ContratoApiService,
              private ar: ActivatedRoute, 
              private route: Router,
              private dialog: MatDialog) {
      this.form = this.fb.group({
        fornecedor: ['', Validators.required],
        numero: ['', [Validators.required, Validators.min(1), Validators.max(999999999)]],
        emissao: ['', Validators.required],
        valor: ['',],
        tipoequipamento: [''],
        fabricante: [''],
        modelo:[''],
        quantidade: [''],
        valorunitario: [''],
        virtual: [''],
        descricao: [''],
        tipoaquisicao: [''],
        dtlimitegarantia: [''],
        contrato: ['']
      })
    }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.nota.cliente = this.session.usuario.cliente;

    this.util.aguardar(true);
    this.api.listarFornecedores("null", this.nota.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      this.fornecedores = res.data;
      this.api.listarTiposRecursos("null", this.nota.cliente, this.session.token).then(res => {
        this.tipos = res.data;
        this.ar.paramMap.subscribe(param => {
          var parametro = param.get('id');
          if(parametro != null) {
            this.nota = JSON.parse(atob(parametro));
            this.buscarNotaPorId();
          }
        });
        
        // Verificar se o tipo de aquisição já está definido como "Alugado" na inicialização
        setTimeout(() => {
          if (this.notaItem.tipoaquisicao == 1 || this.notaItem.tipoaquisicao === '1') {
            this.listarContratosFornecedor();
          }
        }, 1000);
      });
    })
  }

  // 🔧 MÉTODOS DE CONTROLE DO STEPPER
  avancarPasso1() {
    if (this.form.get('fornecedor')?.valid && 
        this.form.get('numero')?.valid && 
        this.form.get('emissao')?.valid) {
      this.step1Completed = true;
      this.step2Active = true;
      this.scrollToTop();
    } else {
      this.util.exibirMensagemToast('Preencha todos os campos obrigatórios do primeiro passo', 3000);
    }
  }

  voltarPasso2() {
    this.step2Active = false;
    this.step1Completed = false;
    this.scrollToTop();
  }

  private scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  voltar() {
    this.route.navigate(['/notas-fiscais']);
  }

  cancelar() {
    // Usar modal padronizado do sistema
    const dialogRef = this.dialog.open(MessageboxComponent, {
      data: {
        mensagem: `
          <div style="text-align: center;">
            <i class="cil-warning" style="font-size: 48px; color: #f59e0b; margin-bottom: 16px;"></i>
            <h3 style="margin: 0 0 12px 0; color: #080039; font-size: 1.25rem;">Cancelar Cadastro</h3>
            <p style="margin: 0; color: #64748b; font-size: 0.95rem; line-height: 1.6;">
              Deseja realmente cancelar?<br>
              Todas as alterações serão perdidas.
            </p>
          </div>
        `,
        exibeCancelar: true
      },
      width: '450px',
      disableClose: false
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado === true) {
        // Usuário confirmou o cancelamento
        this.route.navigate(['/notas-fiscais']);
      }
    });
  }

  buscarNotaPorId() {
    this.util.aguardar(true);
    this.api.buscarNotaFiscalPorId(this.nota.id, this.session.token).then(res => {
      this.util.aguardar(false);
      this.nota = res.data;
      this.dataSource = new MatTableDataSource<any>(this.nota.notasfiscaisitens);
      this.dataSource.paginator = this.paginator;
      
      // Se é edição, marcar passos como completos
      if (this.nota.id > 0) {
        this.step1Completed = true;
        this.step2Active = true;
        this.step2Completed = true;
      }
    })
  }

  listarFabricantes(){
    this.util.aguardar(true);
    this.api.listarFabricantesPorTipoRecurso(this.notaItem.tipoequipamento, this.nota.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      this.fabricantes = res.data;
      // Limpar seleções dependentes
      this.notaItem.fabricante = '';
      this.notaItem.modelo = '';
      this.fabricantes = res.data;
      this.modelos = [];
    })
  }

  listarModelos(){
    this.util.aguardar(true);
    this.api.listarModelosDoFabricante(this.notaItem.fabricante, this.nota.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      this.modelos = res.data;
      // Limpar seleção dependente
      this.notaItem.modelo = '';
    })
  }

  onTipoAquisicaoChange(event: any) {
    if (event.value == 1 || event.value === '1') { // Alugado
      this.listarContratosFornecedor();
    } else {
      this.contratos = [];
      this.notaItem.contrato = '';
    }
  }

  listarContratosFornecedor() {
    if (!this.nota.fornecedor) {
      this.contratos = [];
      return;
    }
    
    this.util.aguardar(true);
    this.apiContratos.listarPorFornecedor(this.nota.fornecedor).then(res => {
      this.util.aguardar(false);
      if (res && res.data && res.data.result) {
        this.contratos = res.data.result;
      } else {
        this.contratos = [];
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('[NOTA-FISCAL] Erro ao carregar contratos:', err);
      this.contratos = [];
      this.util.exibirMensagemToast('Erro ao carregar contratos do fornecedor', 3000);
    });
  }

  adicionarComposicao(){
    // Validação dos campos obrigatórios
    if (!this.notaItem.tipoequipamento || !this.notaItem.fabricante || 
        !this.notaItem.modelo || !this.notaItem.quantidade || !this.notaItem.valorunitario) {
      this.util.exibirMensagemToast('Preencha todos os campos obrigatórios do item', 3000);
      return;
    }

    if(this.nota.id == 0) {
      var te = this.tipos.filter(x => {
        return x.id === this.notaItem.tipoequipamento;
      })[0];
      var fb = this.fabricantes.filter(x => {
        return x.id == this.notaItem.fabricante
      })[0];
      var md = this.modelos.filter(x => {
        return x.id == this.notaItem.modelo
      })[0];
      this.notaItem.tipoequipamentoNavigation = te;
      this.notaItem.fabricanteNavigation = fb;
      this.notaItem.modeloNavigation = md;
      
      // ✅ CORREÇÃO: Converter contrato para número se for string
      if (this.notaItem.contrato && typeof this.notaItem.contrato === 'string') {
        this.notaItem.contrato = parseInt(this.notaItem.contrato, 10);
      }
      
      this.nota.notasfiscaisitens.push(this.notaItem);
      this.notaItem = {};
      this.dataSource = new MatTableDataSource<any>(this.nota.notasfiscaisitens);
      this.dataSource.paginator = this.paginator;
      
      // Marcar passo 2 como completo se há itens
      if (this.nota.notasfiscaisitens.length > 0) {
        this.step2Completed = true;
      }
      
      this.util.exibirMensagemToast('Item adicionado com sucesso!', 2000);
    }
    else {
      this.notaItem.notafiscal = this.nota.id;
      this.util.aguardar(true);
      this.api.adicionarItemNotaFiscal(this.notaItem, this.session.token).then(res => {
        this.util.aguardar(false);
        this.notaItem = {};
        this.buscarNotaPorId();
        this.util.exibirMensagemToast('Item adicionado com sucesso!', 2000);
      })
    }
  }

  excluirComposicao(nfi){
    if(this.nota.id == 0) {
      this.nota.notasfiscaisitens.splice(nfi);
      this.dataSource = new MatTableDataSource<any>(this.nota.notasfiscaisitens);
      this.dataSource.paginator = this.paginator;
      
      // Atualizar status do passo 2
      if (this.nota.notasfiscaisitens.length == 0) {
        this.step2Completed = false;
      }
      
      this.util.exibirMensagemToast('Item removido com sucesso!', 2000);
    }
    else {
      this.util.aguardar(true);
      this.api.excluirItemNotaFiscal(nfi.id, this.session.token).then(res => {
        this.util.aguardar(false);
        this.buscarNotaPorId();
        this.util.exibirMensagemToast('Item removido com sucesso!', 2000);
      })
    }
  }

  salvar() {
    if(this.form.valid && this.nota.notasfiscaisitens.length > 0) {
      // this.nota.notasfiscaisitens.map(x => {
      //   delete (x.fabricanteNavigation)
      //   delete (x.tipoequipamentoNavigation)
      //   delete (x.modeloNavigation)
      // });
      this.util.aguardar(true);
      this.api.salvarNotaFiscal(this.nota, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Nota fiscal salva com sucesso!', 5000);
          this.route.navigate(['/notas-fiscais']);
        }
      })
    } else if (!this.form.valid) {
      this.util.exibirMensagemToast('Preencha todos os campos obrigatórios', 3000);
    } else if (this.nota.notasfiscaisitens.length == 0) {
      this.util.exibirMensagemToast('Adicione pelo menos um item à composição', 3000);
    }
  }
}
