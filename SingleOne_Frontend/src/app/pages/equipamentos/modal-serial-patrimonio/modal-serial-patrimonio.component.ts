import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-modal-serial-patrimonio',
  templateUrl: './modal-serial-patrimonio.component.html',
  styleUrls: ['./modal-serial-patrimonio.component.scss']
})
export class ModalSerialPatrimonioComponent implements OnInit {

  private session:any = {};
  public form: FormGroup;
  public equipamento:any = {};
  public nomeEquipamento = '';
  public empresas:any = [];
  public centros:any = [];

  constructor(public dialogRef: MatDialogRef<ModalSerialPatrimonioComponent>,  @Inject(MAT_DIALOG_DATA) public data:any, 
    private fb: FormBuilder, private apiCad: ConfiguracoesApiService, private util: UtilService, private api: EquipamentoApiService) { 
    this.form = this.fb.group({
      empresa: ['', Validators.required],
      centrocusto: ['', Validators.required],
    })
  }

  ngOnInit(): void {
    // Verificar se os dados foram passados corretamente
    if (!this.data || !this.data.recurso) {
      console.error('[MODAL] Dados do recurso não foram passados corretamente:', this.data);
      this.dialogRef.close(false);
      return;
    }
    
    this.equipamento = this.data.recurso;
    if (!this.equipamento.tipoequipamento || !this.equipamento.fabricante || !this.equipamento.modelo) {
      console.error('[MODAL] Equipamento com dados incompletos:', this.equipamento);
      this.dialogRef.close(false);
      return;
    }
    
    this.nomeEquipamento = this.equipamento.tipoequipamento + ' ' + this.equipamento.fabricante + ' ' + this.equipamento.modelo + ' S/N:' + this.equipamento.numeroserie + ' Patrimônio:' + this.equipamento.patrimonio;
    this.session = this.util.getSession('usuario');
    this.util.aguardar(true);
    this.apiCad.listarEmpresas("null", this.equipamento.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        console.error('[MODAL] Erro ao carregar empresas:', res);
        this.util.exibirFalhaComunicacao();
      }
      else {
        this.empresas = res.data;
        this.api.buscarEquipamentoPorId(this.equipamento.id, this.session.token).then(res => {
          if (res.status === 200 && res.data) {
            this.equipamento = res.data;
          } else {
            console.error('[MODAL] Erro ao buscar equipamento:', res);
          }
        }).catch(error => {
          console.error('[MODAL] Erro na API do equipamento:', error);
        });
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('[MODAL] Erro na API de empresas:', error);
      this.util.exibirFalhaComunicacao();
    });
  }

  listarCentrosCustos() {
    if (!this.equipamento.empresa) {
      console.error('[MODAL] Empresa não selecionada');
      return;
    }
    this.util.aguardar(true);
    this.apiCad.listarCentroCustoDaEmpresa(this.equipamento.empresa, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        console.error('[MODAL] Erro ao carregar centros de custo:', res);
        this.util.exibirFalhaComunicacao();
      }
      else {
        this.centros = res.data;
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('[MODAL] Erro na API de centros de custo:', error);
      this.util.exibirFalhaComunicacao();
    });
  }

  salvar() {
    if(!this.form.valid) {
      console.error('[MODAL] Formulário inválido');
      this.util.exibirMensagemToast('Por favor, preencha todos os campos obrigatórios', 3000);
      return;
    }
    
    if(!this.equipamento.empresa || !this.equipamento.centrocusto) {
      console.error('[MODAL] Empresa ou centro de custo não selecionados');
      this.util.exibirMensagemToast('Por favor, selecione empresa e centro de custo', 3000);
      return;
    }
    this.util.aguardar(true);
    this.api.salvarEquipamento(this.equipamento, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200) {
        console.error('[MODAL] Erro ao salvar equipamento:', res);
        this.util.exibirFalhaComunicacao();
      }
      else {
        this.dialogRef.close(true);
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('[MODAL] Erro na API ao salvar equipamento:', error);
      this.util.exibirFalhaComunicacao();
    });
  }

}
