import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { CategoriasApiService } from 'src/app/api/categorias/categorias-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-tipo-recurso',
  templateUrl: './tipo-recurso.component.html',
  styleUrls: ['./tipo-recurso.component.scss']
})
export class TipoRecursoComponent implements OnInit {

  @Input() tiporecurso: any = {};
  @Input() modo: 'criar' | 'editar' = 'criar';
  @Output() tipoSalvo = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  private session: any = {};
  public categorias: any[] = [];
  public form: FormGroup;
  public carregando = false;

  constructor(
    private fb: FormBuilder, 
    private util: UtilService, 
    private api: ConfiguracoesApiService,
    private categoriasApi: CategoriasApiService
  ) { 
    this.form = this.fb.group({
      categoriaId: ['', [Validators.required, Validators.min(1)]],
      descricao: ['', [Validators.required, Validators.minLength(2)]],
      transitolivre: [false] // Padrão false para maior segurança
    })
  }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    // Carregar categorias disponíveis
    this.carregarCategorias();
    
    // Se for edição, preencher o formulário
    if (this.modo === 'editar' && this.tiporecurso?.id) {
      this.form.patchValue({
        categoriaId: this.tiporecurso.categoriaId || this.tiporecurso.categoria_id,
        descricao: this.tiporecurso.descricao,
        transitolivre: this.tiporecurso.transitolivre || false
      });
    }
  }

  /**
   * Carrega as categorias disponíveis (apenas ativas)
   */
  private async carregarCategorias() {
    try {
      this.util.aguardar(true);
      const response = await this.categoriasApi.listarCategorias();
      this.util.aguardar(false);
      
      if (response && response.data) {
        if (response.data.dados) {
          // Filtrar apenas categorias ativas
          this.categorias = response.data.dados.filter((cat: any) => cat.ativo);
        } else if (Array.isArray(response.data)) {
          this.categorias = response.data.filter((cat: any) => cat.ativo);
        }
      }
    } catch (error) {
      this.util.aguardar(false);
      console.error('Erro ao carregar categorias:', error);
      this.categorias = [];
      this.util.exibirMensagemToast('Erro ao carregar categorias. Tente novamente.', 5000);
    }
  }

  getTitulo(): string {
    return this.modo === 'criar' ? 'Novo Tipo de Recurso' : 'Editar Tipo de Recurso';
  }

  getBotaoTexto(): string {
    return this.modo === 'criar' ? 'Criar' : 'Salvar';
  }

  salvar() {
    // Verificar se categoria foi selecionada
    if (!this.form.get('categoriaId')?.value) {
      this.util.exibirMensagemToast('Por favor, selecione uma categoria.', 5000);
      this.form.get('categoriaId')?.markAsTouched();
      return;
    }

    if(this.form.valid) {
      this.carregando = true;
      
      // Desabilitar o formulário durante o carregamento
      this.form.disable();
      
      this.util.aguardar(true);
      
      // Preparar dados para salvar
      const dadosTipo = {
        id: this.tiporecurso?.id,
        categoriaId: this.form.get('categoriaId')?.value,
        categoria_id: this.form.get('categoriaId')?.value, // 🆕 Incluir ambos os campos para compatibilidade
        descricao: this.form.get('descricao')?.value,
        TransitoLivre: this.form.get('transitolivre')?.value || false, // 🆕 Campo trânsito livre
        ativo: this.tiporecurso?.ativo ?? true, // 🆕 Preservar status ativo (true para novos, manter atual para edições)
        tipoequipamentosclientes: [{ cliente: this.session.usuario.cliente }]
      };
      
      this.api.salvarTipoRecurso(dadosTipo, this.session.token).then(res => {
        this.carregando = false;
        this.form.enable(); // Reabilitar o formulário
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Tipo de recurso salvo com sucesso!', 5000);
          this.tipoSalvo.emit(res.data); // Emitir evento para o componente pai
        }
      }).catch(() => {
        this.carregando = false;
        this.form.enable(); // Reabilitar o formulário em caso de erro
        this.util.aguardar(false);
      });
    }
  }

  cancelar() {
    this.cancelado.emit();
  }

}
