import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { RequisicaoApiService } from 'src/app/api/requisicoes/requisicao-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-adicionar-observacao',
  templateUrl: './adicionar-observacao.component.html',
  styleUrls: ['./adicionar-observacao.component.scss']
})
export class AdicionarObservacaoComponent implements OnInit {

  private session:any = {};
  public form:any = {};
  public equipamento:any = {};

  constructor(public dialogRef: MatDialogRef<AdicionarObservacaoComponent>,
    @Inject(MAT_DIALOG_DATA) public data:any , private util: UtilService,
    private api: RequisicaoApiService, private fb: FormBuilder) {
      this.form = fb.group({
        observacao: ['', Validators.required]
      });
     }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.equipamento = this.data.eqp;
  }

  salvar() {
    if(this.equipamento.observacaoEntrega != null) {
      this.util.aguardar(true);
      this.api.adicionarObservacaoEquipamentoVM(this.equipamento, this.session.token).then(res => {
        this.util.aguardar(false);
        this.util.exibirMensagemToast('Observação salva com sucesso.', 5000);
        this.dialogRef.close();
      })
    }
    else {
      this.util.exibirMensagemToast('Informe a observação antes de salvar.', 5000);
    }
  }

}
