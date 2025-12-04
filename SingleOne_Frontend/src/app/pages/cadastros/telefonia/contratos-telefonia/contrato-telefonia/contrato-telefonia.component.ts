import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-contrato-telefonia',
  templateUrl: './contrato-telefonia.component.html',
  styleUrls: ['./contrato-telefonia.component.scss']
})
export class ContratoTelefoniaComponent implements OnInit {

  @Input() contrato: any = {};
  @Input() modo: 'criar' | 'editar' = 'criar';
  @Output() contratoSalvo = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  private session:any = {};
  public operadoras:any = [];
  public form: FormGroup;

  constructor(private fb: FormBuilder, 
    private util: UtilService, 
    private api: TelefoniaApiService) { 
      this.form = this.fb.group({
        operadora: ['', Validators.required],
        nome: ['', Validators.required],
        descricao: ['']
      })
    }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    // Carregar operadoras
    this.util.aguardar(true);
    this.api.listarOperadoras(this.session.token).then(res => {
      this.util.aguardar(false);
      this.operadoras = res.data;
      
      // Se for edição, preencher o formulário
      if (this.modo === 'editar' && this.contrato) {
        this.preencherFormulario();
      }
    })
  }

  preencherFormulario() {
    this.form.patchValue({
      operadora: this.contrato.operadora,
      nome: this.contrato.nome,
      descricao: this.contrato.descricao
    });
  }

  salvar() {
    if(this.form.valid) {
      this.util.aguardar(true);
      
      // Preparar dados para salvar
      const dadosContrato = {
        id: this.contrato?.id || 0,
        operadora: this.form.value.operadora,
        nome: this.form.value.nome,
        descricao: this.form.value.descricao,
        cliente: this.session.usuario.cliente,
        ativo: this.contrato?.ativo !== undefined ? this.contrato.ativo : true
      };
      
      this.api.salvarContrato(dadosContrato, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Contrato salvo com sucesso!', 5000);
          // Emitir evento para o componente pai
          this.contratoSalvo.emit(res.data || dadosContrato);
        }
      })
    }
  }

  cancelar() {
    this.cancelado.emit();
  }

}
