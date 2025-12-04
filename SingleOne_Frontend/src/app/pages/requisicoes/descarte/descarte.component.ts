import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { EvidenciaApiService } from 'src/app/api/evidencias/evidencia-api.service';
import { UtilService } from 'src/app/util/util.service';
import { ModalEvidenciasComponent } from './modal-evidencias/modal-evidencias.component';

@Component({
  selector: 'app-descarte',
  templateUrl: './descarte.component.html',
  styleUrls: ['./descarte.component.scss']
})
export class DescarteComponent implements OnInit {

  public session:any = {};
  public equipamento:any = {};
  public equipamentos:any = [];
  public eqtpsForm = new FormControl();
  public resultadoEquipamento: Observable<any>;
  public descartes:any = [];
  public colunas = ['equipamento', 'numeroserie', 'patrimonio', 'criticidade', 'processos', 'acao'];
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  public dataSource!: MatTableDataSource<any>;

  constructor(
    private util: UtilService, 
    private apiCol: ColaboradorApiService, 
    private api: EquipamentoApiService,
    private evidenciaApi: EvidenciaApiService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    //Busca de equipamentos
    this.resultadoEquipamento = this.eqtpsForm.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.listarEquipamentos(value))
    );
    this.resultadoEquipamento.subscribe();
  }

  listarEquipamentos(valor) {
    if (valor != '' && this.session && this.session.usuario) {
      this.util.aguardar(true);
      this.api.listarEquipamentosDisponiveisParaDescarte(this.session.usuario.cliente, valor, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          console.error('❌ [DESCARTE-COMPONENT] Erro na resposta:', res.status);
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.equipamentos = res.data;
        }
      }).catch(err => {
        this.util.aguardar(false);
        console.error('❌ [DESCARTE-COMPONENT] Erro na requisição:', err);
      })
    } else {
    }
  }
  adicionarEquipamento() {
    if(!this.session || !this.session.usuario) return;
    if(this.equipamento != {} && this.equipamento.equipamento){
      this.equipamento.usuarioDescarte = this.session.usuario.id;
      
      // Se não vier processosObrigatorios do backend, inicializar como false
      if(!this.equipamento.processosObrigatorios) {
        this.equipamento.processosObrigatorios = false;
      }
      
      // Inicializar processos executados como false
      this.equipamento.sanitizacaoExecutada = false;
      this.equipamento.descaracterizacaoExecutada = false;
      this.equipamento.perfuracaoDiscoExecutada = false;
      this.equipamento.evidenciasExecutadas = false;
      
      // Inicializar lista de evidências se não existir
      if(!this.equipamento.evidencias) {
        this.equipamento.evidencias = [];
      }
      
      // Se exige evidências, validar se tem evidências de CADA processo obrigatório
      if(this.equipamento.obrigarEvidencias) {
        this.equipamento.evidenciasExecutadas = this.validarEvidenciasCompletas(this.equipamento);
      }
      
      this.descartes.push(this.equipamento);
      this.dataSource = new MatTableDataSource<any>(this.descartes);
      this.dataSource.paginator = this.paginator;
      this.equipamento = {};
    }
  }
  removerAtivo(ativo:any){
    this.util.exibirMensagemPopUp('Tem certeza que deseja remover este recurso da lista de descarte?', true).then(confirmado => {
      if(confirmado) {
        this.descartes = this.descartes.filter((x:any) => {
          return x.equipamento.numeroserie !== ativo.equipamento.numeroserie && x.equipamento.patrimonio !== ativo.equipamento.patrimonio
        });
        this.dataSource = new MatTableDataSource<any>(this.descartes);
        this.dataSource.paginator = this.paginator;
        this.util.exibirMensagemToast('Recurso removido da lista de descarte', 3000);
      }
    });
  }

  realizarDescarte() {
    // Validar se todos os processos obrigatórios foram executados
    const errosValidacao = [];
    for(const dsc of this.descartes) {
      if(dsc.processosObrigatorios) {
        const processosFaltando = [];
        
        // Se NÃO exige evidências: validar apenas checkboxes
        if(!dsc.obrigarEvidencias) {
          if(dsc.obrigarSanitizacao && !dsc.sanitizacaoExecutada) {
            processosFaltando.push('Sanitização');
          }
          if(dsc.obrigarDescaracterizacao && !dsc.descaracterizacaoExecutada) {
            processosFaltando.push('Descaracterização');
          }
          if(dsc.obrigarPerfuracaoDisco && !dsc.perfuracaoDiscoExecutada) {
            processosFaltando.push('Perfuração de Disco');
          }
        }
        // Se EXIGE evidências: validar se tem evidências de CADA processo
        else {
          if(dsc.obrigarSanitizacao && this.getCountEvidencias(dsc, 'SANITIZACAO') === 0) {
            processosFaltando.push('Evidência de Sanitização (foto/arquivo)');
          }
          if(dsc.obrigarDescaracterizacao && this.getCountEvidencias(dsc, 'DESCARACTERIZACAO') === 0) {
            processosFaltando.push('Evidência de Descaracterização (foto/arquivo)');
          }
          if(dsc.obrigarPerfuracaoDisco && this.getCountEvidencias(dsc, 'PERFURACAO_DISCO') === 0) {
            processosFaltando.push('Evidência de Perfuração de Disco (foto/arquivo)');
          }
        }
        
        if(processosFaltando.length > 0) {
          const equipId = dsc.equipamento.numeroserie || dsc.equipamento.patrimonio;
          errosValidacao.push(`Recurso ${equipId}: ${processosFaltando.join(', ')}`);
        }
      }
    }
    
    if(errosValidacao.length > 0) {
      this.util.exibirMensagemToast(
        'Processos obrigatórios não executados: ' + errosValidacao.join('; '), 
        8000
      );
      return;
    }

    this.util.exibirMensagemPopUp(
      `Tem certeza que deseja realizar o descarte de ${this.descartes.length} recurso(s)? Esta ação não poderá ser desfeita.`, 
      true
    ).then(confirmado => {
      if(confirmado && this.session) {
        this.util.aguardar(true);
        this.api.realizarDescarte(this.descartes, this.session.token).then(res => {
          this.util.aguardar(false);
          if(res.status != 200) {
            if(res.data && typeof res.data === 'string') {
              this.util.exibirMensagemToast(res.data, 8000);
            } else {
              this.util.exibirFalhaComunicacao();
            }
          }
          else {
            this.descartes = [];
            this.dataSource = new MatTableDataSource<any>(this.descartes);
            this.util.exibirMensagemToast('Descarte realizado com sucesso!', 5000);
          }
        })
      }
    })
  }

  // Métodos auxiliares para trabalhar com processos
  toggleProcesso(item: any, processo: string) {
    switch(processo) {
      case 'sanitizacao':
        item.sanitizacaoExecutada = !item.sanitizacaoExecutada;
        break;
      case 'descaracterizacao':
        item.descaracterizacaoExecutada = !item.descaracterizacaoExecutada;
        break;
      case 'perfuracao':
        item.perfuracaoDiscoExecutada = !item.perfuracaoDiscoExecutada;
        break;
      case 'evidencias':
        item.evidenciasExecutadas = !item.evidenciasExecutadas;
        break;
    }
  }

  getNivelCriticidadeClass(nivel: string): string {
    if(!nivel) return '';
    switch(nivel.toUpperCase()) {
      case 'ALTO': return 'criticidade-alto';
      case 'MEDIO': return 'criticidade-medio';
      case 'BAIXO': return 'criticidade-baixo';
      default: return '';
    }
  }

  temProcessosObrigatorios(): boolean {
    return this.descartes.some((d: any) => d.processosObrigatorios);
  }

  abrirModalEvidencias(item: any) {
    const dialogRef = this.dialog.open(ModalEvidenciasComponent, {
      width: '850px',
      maxWidth: '90vw',
      data: {
        equipamento: item.equipamento
      },
      disableClose: false,
      panelClass: 'custom-modal-panel'
    });

    dialogRef.afterClosed().subscribe(totalEvidencias => {
      if (totalEvidencias !== undefined && this.session && this.session.token) {
        this.evidenciaApi.listarEvidencias(item.equipamento.id, this.session.token).then(res => {
          if (res.status === 200) {
            item.evidencias = res.data || [];
            
            // Se exige evidências, verificar se tem evidências de CADA processo
            if (item.obrigarEvidencias) {
              item.evidenciasExecutadas = this.validarEvidenciasCompletas(item);
            }
            this.dataSource = new MatTableDataSource<any>(this.descartes);
            this.dataSource.paginator = this.paginator;
          }
        });
      }
    });
  }

  // Contar evidências de um tipo específico
  getCountEvidencias(item: any, tipo: string): number {
    if (!item.evidencias || item.evidencias.length === 0) return 0;
    return item.evidencias.filter((e: any) => e.tipoprocesso === tipo).length;
  }

  // Validar se tem evidências de TODOS os processos obrigatórios
  validarEvidenciasCompletas(item: any): boolean {
    if (!item.evidencias || item.evidencias.length === 0) return false;
    
    let completo = true;
    
    if (item.obrigarSanitizacao) {
      const count = this.getCountEvidencias(item, 'SANITIZACAO');
      if (count === 0) completo = false;
    }
    
    if (item.obrigarDescaracterizacao) {
      const count = this.getCountEvidencias(item, 'DESCARACTERIZACAO');
      if (count === 0) completo = false;
    }
    
    if (item.obrigarPerfuracaoDisco) {
      const count = this.getCountEvidencias(item, 'PERFURACAO_DISCO');
      if (count === 0) completo = false;
    }
    
    return completo;
  }

  // Verificar se TODOS os equipamentos estão prontos para descarte
  podeRealizarDescarte(): boolean {
    if (this.descartes.length === 0) return false;
    
    for (const item of this.descartes) {
      if (item.processosObrigatorios) {
        // Se NÃO exige evidências: verificar checkboxes
        if (!item.obrigarEvidencias) {
          if (item.obrigarSanitizacao && !item.sanitizacaoExecutada) return false;
          if (item.obrigarDescaracterizacao && !item.descaracterizacaoExecutada) return false;
          if (item.obrigarPerfuracaoDisco && !item.perfuracaoDiscoExecutada) return false;
        }
        // Se EXIGE evidências: verificar se tem evidências de CADA processo
        else {
          if (item.obrigarSanitizacao && this.getCountEvidencias(item, 'SANITIZACAO') === 0) return false;
          if (item.obrigarDescaracterizacao && this.getCountEvidencias(item, 'DESCARACTERIZACAO') === 0) return false;
          if (item.obrigarPerfuracaoDisco && this.getCountEvidencias(item, 'PERFURACAO_DISCO') === 0) return false;
        }
      }
    }
    
    return true;
  }
}
