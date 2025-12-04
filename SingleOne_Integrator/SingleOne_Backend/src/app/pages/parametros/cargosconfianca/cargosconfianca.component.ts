import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-cargosconfianca',
  templateUrl: './cargosconfianca.component.html',
  styleUrls: ['./cargosconfianca.component.scss']
})
export class CargosconfiancaComponent implements OnInit {

  private session: any = {};
  public cargosExistentes: any[] = [];        // Cargos únicos da tabela colaboradores
  public cargosConfianca: any[] = [];         // Cargos marcados como críticos
  public novoCargo: any = {};
  public editandoCargo: any = null;
  public form: FormGroup;
  public carregando = false;
  public usuarioAutorizado = false;

  constructor(
    private fb: FormBuilder, 
    private util: UtilService, 
    private api: ColaboradorApiService,
    private router: Router
  ) {
    this.form = this.fb.group({
      cargo: ['', Validators.required],
      nivelcriticidade: ['ALTO', Validators.required],
      obrigarsanitizacao: [true],
      obrigardescaracterizacao: [true],
      obrigarperfuracaodisco: [true],
      obrigarevidencias: [true]
    });
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    // 🔒 VALIDAÇÃO DE ACESSO - APENAS USUÁRIOS SU
    if (!this.session || !this.session.usuario || this.session.usuario.perfil !== 'su') {
      this.util.exibirMensagemToast('Acesso negado. Apenas usuários SU podem configurar cargos de confiança.', 5000);
      this.router.navigate(['/dashboard']);
      return;
    }

    this.usuarioAutorizado = true;
    this.novoCargo.cliente = this.session.usuario.cliente;
    this.novoCargo.usuariocriacao = this.session.usuario.id;
    
    this.carregarDados();
  }

  carregarDados() {
    if (!this.usuarioAutorizado) return;
    
    this.carregando = true;
    
    // Carregar cargos únicos existentes no sistema
    this.api.listarCargosUnicos(this.session.usuario.cliente, this.session.token).then(res => {
      if (res.status === 200) {
        this.cargosExistentes = res.data || [];
      }
      
      // Carregar cargos de confiança já configurados
      return this.api.listarCargosConfianca(this.session.usuario.cliente, this.session.token);
    }).then(res => {
      if (res.status === 200) {
        this.cargosConfianca = res.data || [];
      }
      this.carregando = false;
    }).catch(() => {
      this.carregando = false;
      this.util.exibirFalhaComunicacao();
    });
  }

  adicionarCargo() {
    if (!this.usuarioAutorizado || !this.form.valid) return;
    
    const dados = {
      ...this.form.value,
      cliente: this.session.usuario.cliente,
      usuariocriacao: this.session.usuario.id,
      ativo: true
    };

    this.util.aguardar(true);
    this.api.salvarCargoConfianca(dados, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        this.util.exibirMensagemToast('Cargo de confiança adicionado com sucesso!', 5000);
        this.form.reset({
          nivelcriticidade: 'ALTO',
          obrigarsanitizacao: true,
          obrigardescaracterizacao: true,
          obrigarperfuracaodisco: true,
          obrigarevidencias: true
        });
        this.carregarDados();
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(() => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  editarCargo(cargo: any) {
    if (!this.usuarioAutorizado) return;
    
    this.editandoCargo = { ...cargo };
    this.form.patchValue({
      cargo: cargo.cargo,
      nivelcriticidade: cargo.nivelcriticidade,
      obrigarsanitizacao: cargo.obrigarsanitizacao,
      obrigardescaracterizacao: cargo.obrigardescaracterizacao,
      obrigarperfuracaodisco: cargo.obrigarperfuracaodisco,
      obrigarevidencias: cargo.obrigarevidencias
    });
  }

  salvarEdicao() {
    if (!this.usuarioAutorizado || !this.form.valid || !this.editandoCargo) return;
    
    const dados = {
      ...this.editandoCargo,
      ...this.form.value
    };

    this.util.aguardar(true);
    this.api.atualizarCargoConfianca(dados, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        this.util.exibirMensagemToast('Cargo de confiança atualizado com sucesso!', 5000);
        this.cancelarEdicao();
        this.carregarDados();
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(() => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  cancelarEdicao() {
    this.editandoCargo = null;
    this.form.reset({
      nivelcriticidade: 'ALTO',
      obrigarsanitizacao: true,
      obrigardescaracterizacao: true,
      obrigarperfuracaodisco: true,
      obrigarevidencias: true
    });
  }

  excluirCargo(id: number) {
    if (!this.usuarioAutorizado) return;
    
    if (confirm('Tem certeza que deseja excluir este cargo de confiança? Esta ação não pode ser desfeita.')) {
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
    if (!this.usuarioAutorizado) return;
    
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
