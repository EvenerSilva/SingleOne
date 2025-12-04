import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { UtilService } from 'src/app/util/util.service';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';

@Component({
  selector: 'app-cargosconfianca',
  templateUrl: './cargosconfianca.component.html',
  styleUrls: ['./cargosconfianca.component.scss']
})
export class CargosconfiancaComponent implements OnInit, AfterViewInit {

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;

  private session: any = {};
  public dataSource: MatTableDataSource<any>;
  public consulta: FormControl = new FormControl('');
  public resultado: Observable<any>;
  public novoCargo: any = {};
  public editandoCargo: any = null;
  public form: FormGroup;
  public carregando = false;
  public mostrarFormulario = false;
  
  // Getter para dados paginados (igual ao padrão de usuários)
  get dadosPaginados(): any[] {
    if (!this.dataSource || !this.dataSource.paginator) {
      return this.dataSource?.data || [];
    }
    
    const startIndex = this.dataSource.paginator.pageIndex * this.dataSource.paginator.pageSize;
    const endIndex = startIndex + this.dataSource.paginator.pageSize;
    return this.dataSource.data.slice(startIndex, endIndex);
  }

  constructor(
    private fb: FormBuilder, 
    private util: UtilService, 
    private api: ColaboradorApiService,
    private cdr: ChangeDetectorRef
  ) {
    this.dataSource = new MatTableDataSource<any>([]);
    this.form = this.fb.group({
      cargo: ['', Validators.required],
      matchexato: [false],  // false = usa padrão (default), true = match exato (exceção)
      nivelcriticidade: ['ALTO', Validators.required],
      obrigarsanitizacao: [true],
      obrigardescaracterizacao: [true],
      obrigarperfuracaodisco: [true],
      obrigarevidencias: [true]
    });
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.novoCargo.cliente = this.session.usuario.cliente;
    this.novoCargo.usuariocriacao = this.session.usuario.id;
    
    // Configurar busca com debounce (igual ao padrão de usuários)
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    
    this.carregarDados();
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  // 🔧 MÉTODO AUXILIAR PARA CONFIGURAR PAGINADOR (igual ao padrão de usuários)
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

  carregarDados() {
    this.carregando = true;
    this.util.aguardar(true);
    
    // Carregar cargos de confiança já configurados
    this.api.listarCargosConfianca(this.session.usuario.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      this.carregando = false;
      
      if (res.status === 200) {
        this.dataSource = new MatTableDataSource<any>(res.data || []);
        this.configurarPaginador();
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch((err) => {
      console.error('Erro ao carregar dados:', err);
      this.util.aguardar(false);
      this.carregando = false;
      this.util.exibirFalhaComunicacao();
    });
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarCargosConfianca(this.session.usuario.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        
        if (res.status === 200) {
          const dados = res.data || [];
          const termo = valor?.toLowerCase() || '';
          
          // Aplicar filtro local
          const dadosFiltrados = dados.filter(cargo => {
            // Busca por cargo
            const cargoMatch = cargo.cargo.toLowerCase().includes(termo);
            
            // Busca por criticidade
            const criticidadeMatch = cargo.nivelcriticidade.toLowerCase().includes(termo);
            
            // Busca por tipo de match
            const tipoMatch = cargo.usarpadrao ? 
              ('padrão'.includes(termo) || 'padrao'.includes(termo) || 'padra'.includes(termo)) : 
              ('exato'.includes(termo) || 'exat'.includes(termo));
            
            // Busca por processos obrigatórios
            const processos = this.getProcessosObrigatorios(cargo);
            const processosMatch = processos.some(processo => 
              processo.toLowerCase().includes(termo)
            );
            
            // Busca por termos específicos de processos
            const sanitizacaoMatch = (termo.includes('sanit') || termo.includes('limpeza')) && cargo.obrigarsanitizacao;
            const descaracterizacaoMatch = (termo.includes('descaracter') || termo.includes('descar')) && cargo.obrigardescaracterizacao;
            const perfuracaoMatch = (termo.includes('perfur') || termo.includes('disco') || termo.includes('furo')) && cargo.obrigarperfuracaodisco;
            const evidenciasMatch = (termo.includes('eviden') || termo.includes('foto') || termo.includes('fotograf')) && cargo.obrigarevidencias;
            
            const termosMatch = sanitizacaoMatch || descaracterizacaoMatch || perfuracaoMatch || evidenciasMatch;
            
            return cargoMatch || criticidadeMatch || tipoMatch || processosMatch || termosMatch;
          });
          
          this.dataSource = new MatTableDataSource<any>(dadosFiltrados);
          this.configurarPaginador();
        } else {
          this.util.exibirFalhaComunicacao();
        }
      }).catch((err) => {
        this.util.aguardar(false);
        console.error('Erro ao buscar:', err);
        this.util.exibirFalhaComunicacao();
      });
    } else {
      this.carregarDados();
    }
  }

  limparBusca() {
    this.consulta.setValue('');
    this.carregarDados();
  }

  adicionarCargo() {
    if (this.form.valid) {
      const formValue = this.form.value;
      const dados = {
        cargo: formValue.cargo,
        usarpadrao: !formValue.matchexato,  // Inverte: matchexato false = usarpadrao true
        nivelcriticidade: formValue.nivelcriticidade,
        obrigarsanitizacao: formValue.obrigarsanitizacao,
        obrigardescaracterizacao: formValue.obrigardescaracterizacao,
        obrigarperfuracaodisco: formValue.obrigarperfuracaodisco,
        obrigarevidencias: formValue.obrigarevidencias,
        cliente: this.session.usuario.cliente,
        usuariocriacao: this.session.usuario.id,
        ativo: true
      };

      this.util.aguardar(true);
      this.api.salvarCargoConfianca(dados, this.session.token).then(res => {
        this.util.aguardar(false);
        
        if (res.status === 200) {
          this.util.exibirMensagemToast('Cargo de confiança adicionado com sucesso!', 5000);
          this.cancelarEdicao();
          this.carregarDados();
        } else if (res.error || res.status === 409) {
          // Erro de validação (duplicata)
          const mensagem = res.data || 'Este cargo já está configurado!';
          this.util.exibirMensagemToast(mensagem, 7000);
        } else {
          this.util.exibirFalhaComunicacao();
        }
      }).catch((err) => {
        this.util.aguardar(false);
        this.util.exibirFalhaComunicacao();
      });
    }
  }

  abrirFormulario() {
    this.mostrarFormulario = true;
    this.editandoCargo = null;
    this.form.reset({
      matchexato: false,
      nivelcriticidade: 'ALTO',
      obrigarsanitizacao: true,
      obrigardescaracterizacao: true,
      obrigarperfuracaodisco: true,
      obrigarevidencias: true
    });
  }

  toggleFormulario() {
    this.mostrarFormulario = !this.mostrarFormulario;
    if (!this.mostrarFormulario) {
      this.cancelarEdicao();
    }
  }

  editarCargo(cargo: any) {
    this.editandoCargo = { ...cargo };
    this.mostrarFormulario = true;
    this.form.patchValue({
      cargo: cargo.cargo,
      matchexato: !cargo.usarpadrao,  // Inverte: usarpadrao false = matchexato true
      nivelcriticidade: cargo.nivelcriticidade,
      obrigarsanitizacao: cargo.obrigarsanitizacao,
      obrigardescaracterizacao: cargo.obrigardescaracterizacao,
      obrigarperfuracaodisco: cargo.obrigarperfuracaodisco,
      obrigarevidencias: cargo.obrigarevidencias
    });
  }

  salvarEdicao() {
    if (this.form.valid && this.editandoCargo) {
      const formValue = this.form.value;
      const dados = {
        ...this.editandoCargo,
        cargo: formValue.cargo,
        usarpadrao: !formValue.matchexato,  // Inverte: matchexato false = usarpadrao true
        nivelcriticidade: formValue.nivelcriticidade,
        obrigarsanitizacao: formValue.obrigarsanitizacao,
        obrigardescaracterizacao: formValue.obrigardescaracterizacao,
        obrigarperfuracaodisco: formValue.obrigarperfuracaodisco,
        obrigarevidencias: formValue.obrigarevidencias
      };

      this.util.aguardar(true);
      this.api.atualizarCargoConfianca(dados, this.session.token).then(res => {
        this.util.aguardar(false);
        
        if (res.status === 200) {
          this.util.exibirMensagemToast('Cargo de confiança atualizado com sucesso!', 5000);
          this.cancelarEdicao();
          this.carregarDados();
        } else if (res.error || res.status === 409 || res.status === 404) {
          // Erro de validação (duplicata ou não encontrado)
          const mensagem = res.data || 'Erro ao atualizar cargo de confiança!';
          this.util.exibirMensagemToast(mensagem, 7000);
        } else {
          this.util.exibirFalhaComunicacao();
        }
      }).catch((err) => {
        this.util.aguardar(false);
        this.util.exibirFalhaComunicacao();
      });
    }
  }

  cancelarEdicao() {
    this.editandoCargo = null;
    this.mostrarFormulario = false;
    this.form.reset({
      matchexato: false,
      nivelcriticidade: 'ALTO',
      obrigarsanitizacao: true,
      obrigardescaracterizacao: true,
      obrigarperfuracaodisco: true,
      obrigarevidencias: true
    });
  }

  excluirCargo(id: number) {
    if (confirm('Tem certeza que deseja excluir este cargo de confiança?')) {
      this.util.aguardar(true);
      this.api.excluirCargoConfianca(id, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status === 200) {
          this.util.exibirMensagemToast('Cargo de confiança excluído com sucesso!', 5000);
          this.carregarDados();
        } else {
          this.util.exibirFalhaComunicacao();
        }
      }).catch(() => {
        this.util.aguardar(false);
        this.util.exibirFalhaComunicacao();
      });
    }
  }

  toggleAtivo(cargo: any) {
    const dados = { ...cargo, ativo: !cargo.ativo };
    
    this.util.aguardar(true);
    this.api.atualizarCargoConfianca(dados, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        this.util.exibirMensagemToast(
          `Cargo ${dados.ativo ? 'ativado' : 'desativado'} com sucesso!`, 
          3000
        );
        this.carregarDados();
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(() => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  getNivelCriticidadeClass(nivel: string): string {
    switch (nivel) {
      case 'ALTO': return 'criticidade-alto';
      case 'MEDIO': return 'criticidade-medio';
      case 'BAIXO': return 'criticidade-baixo';
      default: return '';
    }
  }

  getProcessosObrigatorios(cargo: any): string[] {
    const processos = [];
    if (cargo.obrigarsanitizacao) processos.push('Sanitização');
    if (cargo.obrigardescaracterizacao) processos.push('Descaracterização');
    if (cargo.obrigarperfuracaodisco) processos.push('Perfuração Disco');
    if (cargo.obrigarevidencias) processos.push('Evidências');
    return processos;
  }
}
