import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-plano',
  templateUrl: './plano.component.html',
  styleUrls: ['./plano.component.scss']
})
export class PlanoComponent implements OnInit {

  @Input() plano: any = {};
  @Input() modo: 'novo' | 'editar' = 'novo';
  @Output() planoSalvo = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  private session:any = {};
  public operadoras:any = [];
  public contratos:any = [];
  public form: FormGroup;
  public carregando = false;

  constructor(private fb: FormBuilder, private util: UtilService, private api: TelefoniaApiService) {
      this.form = this.fb.group({
        operadora: ['', Validators.required],
        contrato: ['', Validators.required],
        nome: ['', Validators.required],
        valor: ['', Validators.required]
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');

    this.util.aguardar(true);
    this.api.listarOperadoras(this.session.token).then(res => {
      this.util.aguardar(false);
      this.operadoras = res.data;

      // Se for edição, preencher o formulário
      if (this.modo === 'editar' && this.plano) {
        this.form.patchValue({
          operadora: this.plano.operadoraId,    // ✅ operadoraId (com I maiúsculo)
          contrato: this.plano.contratoId,      // ✅ contratoId (com I maiúsculo)
          nome: this.plano.plano,               // ✅ plano (nome da view)
          valor: this.plano.valor
        });
        // ✅ Carregar contratos da operadora selecionada
        this.listarContratos();
      }
    })
  }

  listarContratos(){
    this.util.aguardar(true);
    // ✅ Usar o valor do formulário para operadora
    const operadoraId = this.form.get('operadora')?.value;
    this.api.listarContratos("null", operadoraId, this.session.usuario.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      this.contratos = res.data;
    })
  }

  salvar() {
    if(this.form.valid) {
      this.carregando = true;
      
      // ✅ Criar ou atualizar o objeto plano com os campos corretos para PlanosVM
      const planoParaSalvar = {
        id: this.plano?.id || 0,
        operadoraId: this.form.value.operadora,
        contratoId: this.form.value.contrato,
        plano: this.form.value.nome,
        valor: this.form.value.valor,
        ativo: true
      };

      this.api.salvarPlano(planoParaSalvar, this.session.token).then(res => {
        this.carregando = false;
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Plano salvo com sucesso!', 5000);
          this.planoSalvo.emit(planoParaSalvar);
        }
      }).catch(() => {
        this.carregando = false;
      });
    }
  }

  // 🎯 MÉTODOS AUXILIARES
  cancelar() {
    this.cancelado.emit();
  }

  getTitulo(): string {
    return this.modo === 'editar' ? 'Editar Plano' : 'Novo Plano';
  }

  getBotaoTexto(): string {
    return this.modo === 'editar' ? 'Atualizar' : 'Salvar';
  }

}
