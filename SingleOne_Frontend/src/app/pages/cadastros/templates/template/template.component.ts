import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service.js';
import { UtilService } from 'src/app/util/util.service.js';
import * as Editor from '../../../../../ckeditor5/build/ckeditor.js';
import { UploadAdapter } from './UploadAdapter.js';

@Component({
  selector: 'app-template',
  templateUrl: './template.component.html',
  styleUrls: ['./template.component.scss']
})
export class TemplateComponent implements OnInit {

  public Editor = Editor;
  public configEditor:any = {
    // toolbar: {
    //   alignment: {
    //     options: [ 'left', 'right' ]
    //   },
    //   items: [
    //     'fontfamily', 'fontsize', 'fontColor', 'fontBackgroundColor',
    //     'heading',
    //     'bold',
    //     'italic',
    //     'underline',
    //     'link',
    //     'bulletedList',
    //     'numberedList',
    //     '|',
    //     'indent',
    //     'outdent',
    //     '|',
    //     'imageUpload',
    //     'blockQuote',
    //     'insertTable',
    //     'undo',
    //     'redo',
    //     'alignment:left',
    //     'alignment:right', 
    //     'alignment:center', 
    //     'alignment:justify'
    //   ],
    // },
    // image: {
    //   toolbar: [
    //     'imageStyle:full',
    //     'imageStyle:side',
    //     '|',
    //     'imageTextAlternative'
    //   ]
    // },
    // table: {
    //   contentToolbar: [
    //     'tableColumn',
    //     'tableRow',
    //     'mergeTableCells',
    //     'tableCellProperties',
		// 		'tableProperties'
    //   ]
    // },
    // // This value must be kept in sync with the language defined in webpack.config.js.
    // language: 'pt-BR'
    toolbar: {
      items: [
        'heading',
        '|',
        'fontBackgroundColor',
        'fontFamily',
        'fontColor',
        'fontSize',
        'alignment',
        'bold',
        'italic',
        'link',
        'bulletedList',
        'numberedList',
        '|',
        'outdent',
        'indent',
        '|',
        'imageUpload',
        'blockQuote',
        'insertTable',
        'undo',
        'redo'
      ]
    },
    language: 'pt-br',
    image: {
      toolbar: [
        'imageTextAlternative',
        'imageStyle:inline',
        'imageStyle:block',
        'imageStyle:side'
      ]
    },
    table: {
      contentToolbar: [
        'tableColumn',
        'tableRow',
        'mergeTableCells',
        'tableCellProperties',
        'tableProperties'
      ]
    },
  }
  public template:any = {
    conteudo: ''
  };
  public tiposTemplate:any = [];
  private session:any = {};

  public form: FormGroup;

constructor(private fb: FormBuilder, private util: UtilService, private api: ConfiguracoesApiService, 
    private route: Router, private ar: ActivatedRoute) {
    this.form = this.fb.group({
      tipo: ['', Validators.required],
      titulo: ['', Validators.required],
      conteudo: ['']
    })
   }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.template.cliente = this.session.usuario.cliente;
    
    this.util.aguardar(true);
    this.api.listarTiposDeTemplates(this.session.token).then(res => {
      this.util.aguardar(false);
      this.tiposTemplate = res.data;

      this.ar.paramMap.subscribe(param => {
        var parametro = param.get('id');
        if(parametro != null) {
          this.template.id = parametro;
          this.buscarTemplatePorId();
        }
      })
    })
  }

  onReady(eventData) {
    eventData.plugins.get('FileRepository').createUploadAdapter = function (loader) {
      return new UploadAdapter(loader);
    };
  }

  buscarTemplatePorId(){
    this.util.aguardar(true);
    this.api.obterTemplatePorId(this.template.id, this.session.token).then(res => {
      this.util.aguardar(false);
      this.template = res.data;
      
      // ✅ CORREÇÃO: Sincronizar formulário com dados do template
      this.form.patchValue({
        tipo: this.template.tipo,
        titulo: this.template.titulo,
        conteudo: this.template.conteudo
      });
    })
  }

  preview() {
    // ✅ CORREÇÃO: Validar conteúdo antes de enviar
    if (!this.template.conteudo || this.template.conteudo.trim() === '') {
      this.util.exibirMensagemToast('Por favor, adicione conteúdo ao template antes de visualizar', 5000);
      return;
    }

    this.util.aguardar(true);
    var tmp:any = {
      conteudo: this.template.conteudo,
      usuarioLogado: this.session.usuario.id
    }
    // ✅ CORREÇÃO: Manter a mesma lógica de gerarLaudoEmPDF para consistência
    this.api.visualizarTemplate(tmp, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        this.util.gerarDocumentoNovaGuia(res.data);
      } else {
        this.util.exibirMensagemToast('Falha de comunicação com o serviço...', 5000);
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('Erro ao visualizar template:', err);
      this.util.exibirMensagemToast('Erro ao visualizar template', 5000);
    })
  }

  salvar() {
    if (this.form.valid) {
      // ✅ CORREÇÃO: Usar valores do formulário e atualizar o template
      this.template.tipo = this.form.get('tipo')?.value;
      this.template.titulo = this.form.get('titulo')?.value;
      this.template.conteudo = this.form.get('conteudo')?.value;
      this.template.cliente = this.session.usuario.cliente;
      
      this.util.aguardar(true);
      this.api.salvarTemplate(this.template, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Template salvo com sucesso!', 5000);
          this.route.navigate(['/templates']);
        }
      }).catch(err => {
        this.util.aguardar(false);
        console.error('Erro ao salvar template:', err);
        this.util.exibirFalhaComunicacao();
      });
    } else {
      this.util.exibirMensagemToast('Por favor, preencha todos os campos obrigatórios!', 3000);
    }
  }

}
