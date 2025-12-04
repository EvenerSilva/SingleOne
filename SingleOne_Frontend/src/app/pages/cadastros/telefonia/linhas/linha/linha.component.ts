import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-linha',
  templateUrl: './linha.component.html',
  styleUrls: ['./linha.component.scss']
})
export class LinhaComponent implements OnInit {

  @Input() linha: any = null;
  @Input() modo: 'novo' | 'editar' = 'novo';
  @Output() linhaSalva = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  private session:any = {};
  public operadoras:any = [];
  public contratos:any = [];
  public planos:any = [];
  public form: FormGroup;
  public carregando = false;

  constructor(private fb: FormBuilder, private util: UtilService, private api: TelefoniaApiService,
    private ar: ActivatedRoute, private route: Router) {
      this.form = this.fb.group({
        operadora: ['', Validators.required],
        contrato: ['', Validators.required],
        plano: ['', Validators.required],
        numero: ['', Validators.required],
        iccid: ['']
      })
    }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');

    this.util.aguardar(true);
    this.api.listarOperadoras(this.session.token).then(res => {
      this.util.aguardar(false);
      this.operadoras = res.data;

      // ✅ Se for modo editar, preencher o formulário
      if (this.modo === 'editar' && this.linha) {
        this.preencherFormulario();
      }
      
      // ✅ TESTE: Carregar planos diretamente para debug
      if (this.modo === 'novo') {
        this.testarCarregamentoPlanos();
      }
    })
  }

  // ✅ MÉTODO DE TESTE: Carregar planos diretamente para debug
  testarCarregamentoPlanos() {
    this.api.listarPlanos("null", 1, this.session.usuario.cliente, this.session.token).then(res => {
      if (res.data) {
      }
    }).catch(error => {
      console.error('[LINHA-COMPONENT] Teste de planos - Erro:', error);
    });
  }

  // ✅ Método para preencher o formulário no modo editar
  preencherFormulario() {
    if (this.linha) {
      this.form.patchValue({
        operadora: this.linha.planoNavigation?.contratoNavigation?.operadora || '',
        numero: this.linha.numero || '',
        iccid: this.linha.iccid || ''
      });
      if (this.linha.planoNavigation?.contratoNavigation?.operadora) {
        this.listarContratos().then(() => {
          // ✅ Após carregar contratos, preencher o contrato
          this.form.patchValue({
            contrato: this.linha.planoNavigation?.contratoNavigation?.id || ''
          });
          
          // ✅ Carregar planos baseado no contrato selecionado
          if (this.linha.planoNavigation?.contratoNavigation?.id) {
            this.listarPlanos().then(() => {
              // ✅ Após carregar planos, preencher o plano
              this.form.patchValue({
                plano: this.linha.planoNavigation?.id || ''
              });
            });
          }
        });
      }
    }
  }

  // ✅ Método para obter o título do formulário
  getTitulo(): string {
    return this.modo === 'novo' ? 'Nova Linha Telefônica' : 'Editar Linha Telefônica';
  }

  // ✅ Método para obter o texto do botão
  getBotaoTexto(): string {
    return this.modo === 'novo' ? 'Criar Linha' : 'Salvar Alterações';
  }

  listarContratos(){
    const operadoraId = this.form.get('operadora')?.value;
    if (!operadoraId) return Promise.resolve();
    this.util.aguardar(true);
    return this.api.listarContratos("null", operadoraId, this.session.usuario.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      this.contratos = res.data;
      if (this.modo !== 'editar') {
        this.form.patchValue({
          contrato: '',
          plano: ''
        });
        this.planos = []; // Limpar planos
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('[LINHA-COMPONENT] Erro ao carregar contratos:', error);
    });
  }

  listarPlanos(){
    const contratoId = this.form.get('contrato')?.value;
    if (!contratoId) return Promise.resolve();
    this.util.aguardar(true);
    return this.api.listarPlanos("null", contratoId, this.session.usuario.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      this.planos = res.data;
      if (this.modo !== 'editar') {
        this.form.patchValue({
          plano: ''
        });
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('[LINHA-COMPONENT] Erro ao carregar planos:', error);
    });
  }

  salvar() {
    if(this.form.valid) {
      this.carregando = true;
      
      // ✅ Criar objeto linha para salvar
      const linhaParaSalvar = {
        id: this.linha?.id || 0,
        operadora: this.form.value.operadora,
        contrato: this.form.value.contrato,
        plano: this.form.value.plano,
        numero: this.form.value.numero,
        iccid: this.form.value.iccid
      };

      this.api.salvarLinha(linhaParaSalvar, this.session.token).then(res => {
        this.carregando = false;
        if(res.status == 200){
          this.util.exibirMensagemToast('Linha salva com sucesso!', 5000);
          // ✅ Emitir evento para o componente pai
          this.linhaSalva.emit(linhaParaSalvar);
        }
        else{
          if(res.response?.status == 409){
            this.util.exibirMensagemToast(res.response.data, 5000);
          }
          else if(res.status != 200) {
            this.util.exibirFalhaComunicacao();
          }
        }        
      }).catch(error => {
        this.carregando = false;
        this.util.exibirFalhaComunicacao();
      });
    }
  }

  // ✅ Método para cancelar
  cancelar() {
    this.cancelado.emit();
  }
}
