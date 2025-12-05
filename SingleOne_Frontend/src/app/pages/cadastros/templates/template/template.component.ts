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
    this.api.visualizarTemplate(tmp, this.session.token).then(res => {
      this.util.aguardar(false);
      
      // ✅ CORREÇÃO: Verificar se a resposta é válida e é um PDF
      if (!res || !res.data) {
        console.error('[TEMPLATE] Resposta inválida:', res);
        this.util.exibirMensagemToast('Erro ao visualizar template: resposta inválida do servidor', 5000);
        return;
      }

      // Verificar se é um Blob (PDF)
      if (res.data instanceof Blob) {
        // Verificar Content-Type para distinguir PDF de erro JSON
        const contentType = res.headers?.['content-type'] || res.data.type || '';
        
        if (contentType.includes('application/json')) {
          // É um erro JSON dentro de um Blob
          this.tratarErroResposta(res.data);
        } else if (contentType.includes('application/pdf') || res.data.size > 0) {
          // É um PDF válido
          this.util.gerarDocumentoNovaGuia(res.data);
        } else {
          // Tipo desconhecido, tentar tratar como erro
          this.tratarErroResposta(res.data);
        }
      } else {
        // Não é um Blob, tratar como erro
        this.tratarErroResposta(res.data);
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('[TEMPLATE] Erro ao visualizar template:', err);
      
      // ✅ CORREÇÃO: Tentar extrair mensagem de erro do backend
      if (err && err.response) {
        if (err.response.data) {
          this.tratarErroResposta(err.response.data);
        } else {
          this.util.exibirMensagemToast(`Erro ${err.response.status || 500}: Erro ao visualizar template`, 5000);
        }
      } else if (err && err.message) {
        this.util.exibirMensagemToast(`Erro: ${err.message}`, 5000);
      } else {
        this.util.exibirMensagemToast('Erro ao visualizar template. Verifique sua conexão e tente novamente.', 5000);
      }
    })
  }

  // ✅ NOVO: Método auxiliar para tratar erros do backend
  private tratarErroResposta(data: any) {
    console.log('[TEMPLATE] Tratando erro:', data);
    
    if (!data) {
      this.util.exibirMensagemToast('Erro ao visualizar template: nenhuma resposta do servidor', 5000);
      return;
    }

    if (data instanceof Blob) {
      // Se for um Blob, tentar ler como JSON (erro do backend)
      const reader = new FileReader();
      reader.onload = () => {
        try {
          const errorData = JSON.parse(reader.result as string);
          const mensagem = errorData.Mensagem || errorData.mensagem || 'Erro ao visualizar template';
          console.error('[TEMPLATE] Erro do servidor:', mensagem);
          this.util.exibirMensagemToast(mensagem, 5000);
        } catch (e) {
          console.error('[TEMPLATE] Erro ao parsear JSON do erro:', e);
          this.util.exibirMensagemToast('Erro ao visualizar template: resposta inválida do servidor', 5000);
        }
      };
      reader.onerror = () => {
        console.error('[TEMPLATE] Erro ao ler Blob de erro');
        this.util.exibirMensagemToast('Erro ao visualizar template', 5000);
      };
      reader.readAsText(data);
    } else if (data && typeof data === 'object') {
      // Objeto JavaScript direto
      const mensagem = data.Mensagem || data.mensagem || data.message || 'Erro ao visualizar template';
      console.error('[TEMPLATE] Erro do servidor:', mensagem);
      this.util.exibirMensagemToast(mensagem, 5000);
    } else if (typeof data === 'string') {
      // String que pode ser JSON
      try {
        const errorData = JSON.parse(data);
        const mensagem = errorData.Mensagem || errorData.mensagem || 'Erro ao visualizar template';
        console.error('[TEMPLATE] Erro do servidor:', mensagem);
        this.util.exibirMensagemToast(mensagem, 5000);
      } catch (e) {
        // Se não for JSON, usar a string como mensagem
        console.error('[TEMPLATE] Erro:', data);
        this.util.exibirMensagemToast(data || 'Erro ao visualizar template', 5000);
      }
    } else {
      console.error('[TEMPLATE] Tipo de erro desconhecido:', typeof data, data);
      this.util.exibirMensagemToast('Erro ao visualizar template', 5000);
    }
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
