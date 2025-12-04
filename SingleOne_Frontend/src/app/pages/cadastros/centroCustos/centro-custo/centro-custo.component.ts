import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-centro-custo',
  templateUrl: './centro-custo.component.html',
  styleUrls: ['./centro-custo.component.scss']
})
export class CentroCustoComponent implements OnInit {

  @Input() centro: any = {
    id: 0,
    empresaId: 0,
    codigo: '',
    nome: '',
    createdAt: null,
    updatedAt: null
  };
  
  @Input() modo: 'criar' | 'editar' = 'criar';
  
  @Output() centroSalvo = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  private session: any = {};
  public empresas: any = [];
  public form: FormGroup;
  public salvando = false;

  constructor(private fb: FormBuilder, private util: UtilService, private api: ConfiguracoesApiService) {
      this.form = this.fb.group({
        empresaId: ['', Validators.required],
        codigo: ['', Validators.required],
        nome: ['', Validators.required]
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.util.aguardar(true);
    this.api.listarEmpresas("null", this.session.usuario.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204){
        this.util.exibirFalhaComunicacao();
      }
      else {
        this.empresas = res.data;
      }

      // Se for edição, preencher o formulário
      if (this.modo === 'editar' && this.centro && this.centro.id > 0) {
        this.preencherFormulario();
      }
    })
  }

  private preencherFormulario() {
    if (this.centro && this.centro.id > 0) {
      this.form.patchValue({
        empresaId: this.centro.empresaId || this.centro.empresa || '',
        codigo: this.centro.codigo || '',
        nome: this.centro.nome || ''
      });
    }
  }

  salvar() {
    if(this.form.valid) {
      this.salvando = true;
      this.util.aguardar(true);
      
      // Preparar dados para envio
      const dadosParaEnviar = {
        id: this.centro?.id || 0,
        empresaId: this.form.value.empresaId,
        codigo: this.form.value.codigo,
        nome: this.form.value.nome,
        ativo: true
      };
      this.api.salvarCentroCusto(dadosParaEnviar, this.session.token).then(res => {
        this.salvando = false;
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          const mensagem = this.modo === 'criar' ? 'Centro de custo criado com sucesso!' : 'Centro de custo atualizado com sucesso!';
          this.util.exibirMensagemToast(mensagem, 5000);
          this.centroSalvo.emit(dadosParaEnviar);
        }
      }).catch(err => {
        this.salvando = false;
        this.util.aguardar(false);
        console.error('❌ [CENTRO CUSTO] Erro ao salvar:', err);
        this.util.exibirFalhaComunicacao();
      });
    } else {
      this.util.exibirMensagemToast('Por favor, preencha todos os campos obrigatórios', 3000);
    }
  }

  cancelar() {
    this.cancelado.emit();
  }

}
