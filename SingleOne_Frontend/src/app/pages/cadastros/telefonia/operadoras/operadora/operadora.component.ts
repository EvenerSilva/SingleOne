import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-operadora',
  templateUrl: './operadora.component.html',
  styleUrls: ['./operadora.component.scss']
})
export class OperadoraComponent implements OnInit {

  @Input() operadora: any = {};
  @Input() modo: 'criar' | 'editar' = 'criar';
  @Output() operadoraSalva = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  private session:any = {};
  public form: FormGroup;
  public carregando = false;

  constructor(private fb: FormBuilder, private util: UtilService, private api: TelefoniaApiService) {
      this.form = this.fb.group({
        operadora: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    if (this.modo === 'editar' && this.operadora) {
      this.form.patchValue({
        operadora: this.operadora.nome
      });
    } else {
    }
  }

  getTitulo(): string {
    return this.modo === 'criar' ? 'Nova Operadora' : 'Editar Operadora';
  }

  getBotaoTexto(): string {
    return this.modo === 'criar' ? 'Criar Operadora' : 'Salvar Alterações';
  }

  salvar() {
    if (this.form.valid) {
      this.carregando = true;
      
      // Construir objeto da operadora de forma simples
      let operadoraData: any;
      
      if (this.modo === 'editar' && this.operadora?.id) {
        // Modo edição
        operadoraData = {
          id: this.operadora.id,
          nome: this.form.value.operadora,
          ativo: this.form.value.ativo
        };
      } else {
        // Modo criação - estrutura mínima
        operadoraData = {
          nome: this.form.value.operadora,
          ativo: true
        };
      }
      this.api.salvarOperadora(operadoraData, this.session.token).then(res => {
        this.carregando = false;
        if (res.status === 200) {
          if (res.data && res.data !== '') {
            this.util.exibirMensagemToast('Operadora salva com sucesso!', 5000);
            this.operadoraSalva.emit(res.data);
          } else {
            this.util.exibirFalhaComunicacao();
          }
        } else {
          this.util.exibirFalhaComunicacao();
        }
      }).catch(error => {
        this.carregando = false;
        this.util.exibirFalhaComunicacao();
      });
    }
  }

  cancelar() {
    this.cancelado.emit();
  }

}
