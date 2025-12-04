import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-bo',
  templateUrl: './bo.component.html',
  styleUrls: ['./bo.component.scss']
})
export class BoComponent implements OnInit {

  private session:any = {};
  public equipamento:any = {};
  public form: FormGroup;
  public anexo:any = {};
  public anexos:any = [];

  constructor(private fb: FormBuilder, private util: UtilService, private api: EquipamentoApiService, 
    private apiCad: ConfiguracoesApiService, private ar: ActivatedRoute, private route: Router) { 
      this.form = this.fb.group({
        descricaobo: ['', Validators.required],
        anexo: ['']
      })
    }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.ar.paramMap.subscribe(param => {
      var parametro = param.get('id');
      if(parametro != null) {
        this.equipamento.id = parametro;
        this.buscarEquipamentoPorId();
      }
    })
  }

buscarEquipamentoPorId(){
    this.util.aguardar(true);
    this.api.buscarEquipamentoPorId(this.equipamento.id, this.session.token).then(res => {
      this.util.aguardar(false);
      this.equipamento = res.data;
      this.api.listarAnexosDoEquipamento(this.equipamento.id, this.session.token).then(res => {
        this.anexos = res.data;
      })
    })
  }

  salvar() {
    var mensagem='';
    if(this.equipamento.descricaobo == null || this.equipamento.descricaobo == '') {
      mensagem += '\n- Descrever o que ocorreu;';
    }
    if(this.anexo.arquivo == undefined) {
      mensagem += '\n- Anexar o BO'
    }
    if(mensagem != '') {
      this.util.exibirMensagemToast('Você deve :' + mensagem, 5000);
    }
    else {
      var eqpto:any = {};
      eqpto.id = this.equipamento.id;
      eqpto.possuiBo = 1;
      eqpto.equipamentoStatusId = 8 //Roubado
      eqpto.descricaobo = this.equipamento.descricaobo;
      eqpto.usuario = this.session.usuario.id;
      this.util.aguardar(true);
      this.api.registrarBO(eqpto, this.session.token).then(res => {
        this.util.aguardar(false);
        if(this.anexo.arquivo == undefined) {
          this.util.exibirMensagemToast("Equipamento reportado com sucesso", 5000);
        }
        else {
          this.salvarAnexo();
        }
      })
    }
  }

converterAnexoParaByteArray(event) {
    this.anexo.nome = event.target.files[0].name;
    this.getBuffer(event.target.files[0]).then(res => {
      this.anexo.arquivo = res;
    })
  }

  getBuffer(fileData) {
    return new Promise(function(resolve, reject){
        var reader = new FileReader();
        reader.readAsDataURL(fileData);
        reader.onload = () => resolve(reader.result);
        reader.onerror = error => reject(error);
    })
  }

  salvarAnexo(){
    var file:any = {};
    file.equipamento = this.equipamento.id;
    file.usuario = this.session.usuario.id;
    file.arquivo = this.anexo.arquivo;
    file.nome = this.anexo.nome;
    file.islaudo = false;
    file.isbo = true;
    this.api.incluirAnexo(file, this.session.token).then(res => {
      this.route.navigate(['/recursos']);
    }).catch(err => {
      this.util.exibirMensagemToast('Não conseguimos anexar o documento ao boletim de ocorrencia', 5000);
    })
  }

}
