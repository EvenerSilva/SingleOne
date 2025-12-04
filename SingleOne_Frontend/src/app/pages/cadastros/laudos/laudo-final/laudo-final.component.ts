import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-laudo-final',
  templateUrl: './laudo-final.component.html',
  styleUrls: ['./laudo-final.component.scss']
})
export class LaudoFinalComponent implements OnInit {

  @Input() laudo: any = null; // ✅ ADICIONADO: Input para receber o sinistro
  @Input() mostrar: boolean = false; // ✅ ADICIONADO: Input para controlar visibilidade
  @Output() fechar = new EventEmitter<void>(); // ✅ ADICIONADO: Evento para fechar modal
  @Output() finalizado = new EventEmitter<any>(); // ✅ ADICIONADO: Evento quando finalizado

  private session:any = {};
  public form: FormGroup;
  public salvando = false;

  constructor(private fb: FormBuilder, private util: UtilService, private api: ConfiguracoesApiService) {
    this.form = this.fb.group({
      laudo: ['', Validators.required],
      temconserto: [true],
      mauuso: [false],
      valormanutencao: ['', [this.valorValidoValidator()]] // Removido Validators.required - será condicional
    })

    // Observar mudanças no toggle "temconserto" para ajustar validação do valor
    this.form.get('temconserto')?.valueChanges.subscribe((temConserto: boolean) => {
      this.atualizarValidacaoValor(temConserto);
    });
  }

  // ✅ ADICIONADO: Método para atualizar validação do valor baseado no toggle temconserto
  atualizarValidacaoValor(temConserto: boolean) {
    const valorControl = this.form.get('valormanutencao');
    if (!valorControl) return;

    if (temConserto) {
      // Se tem conserto, valor é obrigatório
      valorControl.setValidators([Validators.required, this.valorValidoValidator()]);
    } else {
      // Se não tem conserto, valor é opcional
      valorControl.setValidators([this.valorValidoValidator()]);
    }
    
    valorControl.updateValueAndValidity();
  }

  // ✅ ADICIONADO: Validador customizado para valor monetário
  valorValidoValidator() {
    return (control: any) => {
      const valor = control.value;
      
      // Se não há valor, não é erro (pode ser opcional)
      if (!valor || valor === '') {
        return null;
      }
      
      // Verificar se é um número válido ou string numérica
      const valorNumerico = parseFloat(valor.toString().replace(/[^\d,.-]/g, '').replace(',', '.'));
      if (isNaN(valorNumerico) || valorNumerico <= 0) {
        return { valorInvalido: true };
      }
      
      return null;
    };
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.inicializarFormulario();
  }

  ngOnChanges() {
    if (this.laudo && this.mostrar) {
      this.inicializarFormulario();
    }
  }

  inicializarFormulario() {
    if (this.laudo) {
      this.buscar();
    }
  }

  buscar() {
    if (!this.laudo?.id) return;
    
    this.util.aguardar(true);
    this.api.buscarLaudoPorId(this.laudo.id, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        this.laudo = res.data;
        this.preencherFormulario();
      }
    }).catch(() => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    })
  }

  preencherFormulario() {
    if (this.laudo) {
      const temConserto = this.laudo.temconserto !== undefined ? this.laudo.temconserto : true;
      
      const formData = {
        laudo: this.laudo.laudo1 || '',
        temconserto: temConserto,
        mauuso: this.laudo.mauuso || false,
        valormanutencao: this.laudo.valormanutencao || ''
      };
      
      this.form.patchValue(formData);
      
      // Aplicar validação baseada no valor inicial do temconserto
      this.atualizarValidacaoValor(temConserto);
    }
  }

  salvar() {
    if(this.form.valid) {
      this.salvando = true;
      this.util.aguardar(true);
      
      const dadosFinalizacao = {
        ...this.laudo,
        laudo1: this.form.value.laudo,
        temconserto: this.form.value.temconserto,
        mauuso: this.form.value.mauuso,
        valormanutencao: this.form.value.valormanutencao
      };
      
      this.api.encerrarLaudo(dadosFinalizacao, this.session.token).then(res => {
        this.util.aguardar(false);
        this.salvando = false;
        
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.finalizado.emit(res.data);
        }
      }).catch(() => {
        this.util.aguardar(false);
        this.salvando = false;
        this.util.exibirFalhaComunicacao();
      })
    }
  }

  cancelar() {
    this.fechar.emit();
  }
}
