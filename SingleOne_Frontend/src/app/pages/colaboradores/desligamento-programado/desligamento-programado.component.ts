import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-desligamento-programado',
  templateUrl: './desligamento-programado.component.html',
  styleUrls: ['./desligamento-programado.component.scss']
})
export class DesligamentoProgramadoComponent implements OnInit {
  
  private session:any = {};
  public form:any = {};
  public colaborador:any = {};

  constructor(public dialogRef: MatDialogRef<DesligamentoProgramadoComponent>,
    @Inject(MAT_DIALOG_DATA) public data:any , private util: UtilService,
    private api: ColaboradorApiService, private fb: FormBuilder) {
      this.form = fb.group({
        dtagendamento: ['', Validators.required]
      });
     }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.colaborador = this.data.colaborador;
  }

  salvar() {
    this.util.aguardar(true);
    this.api.programarDesligamento(this.colaborador, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status !== undefined && res.status !== null && res.status === 200){
        this.util.exibirMensagemToast('Desligamento programado com sucesso.', 5000);
      }
      else if(res.response.status == 409) {
        this.util.exibirMensagemToast(res.response.data, 5000);
      }
      else  {
        this.util.exibirFalhaComunicacao();
      }
      this.dialogRef.close();
    })
  }
}
