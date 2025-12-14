import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { RequisicaoApiService } from 'src/app/api/requisicoes/requisicao-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-agendamento',
  templateUrl: './agendamento.component.html',
  styleUrls: ['./agendamento.component.scss']
})
export class AgendamentoComponent implements OnInit {

  private session:any = {};
  public form:any = {};
  public equipamento:any = {};
  public today = new Date();

  constructor(public dialogRef: MatDialogRef<AgendamentoComponent>,
    @Inject(MAT_DIALOG_DATA) public data:any , private util: UtilService,
    private api: RequisicaoApiService, private fb: FormBuilder) {
      this.form = fb.group({
        dtagendamento: ['', Validators.required]
      });
     }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.equipamento = this.data.requisicao;
    
    const dataExistente = this.equipamento?.dtprogramadaretorno || this.equipamento?.dtProgramadaRetorno;

    if (dataExistente) {
      this.equipamento.dtprogramadaretorno = new Date(dataExistente);
    } else {
      this.equipamento.dtprogramadaretorno = new Date();
    }

    // Garantir identificador do item
    this.equipamento.requisicaoItemId = this.equipamento?.requisicaoItemId
      || this.equipamento?.requisicoesItemId
      || this.equipamento?.id;
  }

  limparDataDevolucao(): void {
    this.equipamento.dtprogramadaretorno = null;
  }

  salvar() {
    const payload: any = {
      requisicaoItemId: this.equipamento?.requisicaoItemId
        || this.equipamento?.requisicoesItemId
        || this.equipamento?.id,
      equipamentoId: this.equipamento?.equipamentoId
        || this.equipamento?.equipamentoid
        || this.equipamento?.equipamento?.id,
      equipamento: this.equipamento?.equipamento,
      numeroSerie: this.equipamento?.numeroSerie || this.equipamento?.numeroserie,
      patrimonio: this.equipamento?.patrimonio,
      observacaoentrega: this.equipamento?.observacaoEntrega || this.equipamento?.observacaoentrega,
      dtprogramadaretorno: this.equipamento?.dtprogramadaretorno || this.equipamento?.dtProgramadaRetorno
    };

    if (payload.dtprogramadaretorno instanceof Date) {
      const date = payload.dtprogramadaretorno as Date;
      const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
      payload.dtprogramadaretorno = local.toISOString().replace('Z', '');
    }
    this.util.aguardar(true);
    this.api.adicionarAgendamentoEquipamentoVM(payload, this.session.token).then(res => {
      this.util.aguardar(false);
      this.util.exibirMensagemToast('Agendamento salvo com sucesso.', 5000);
      this.dialogRef.close(true);
    }).catch(() => {
      this.util.aguardar(false);
      this.util.exibirMensagemToast('Falha ao salvar agendamento.', 5000);
    })
  }

}
