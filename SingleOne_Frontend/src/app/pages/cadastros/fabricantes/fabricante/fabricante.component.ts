import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { CategoriasApiService } from 'src/app/api/categorias/categorias-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-fabricante',
  templateUrl: './fabricante.component.html',
  styleUrls: ['./fabricante.component.scss']
})
export class FabricanteComponent implements OnInit {

  @Input() fabricante: any = {};
  @Input() modo: 'criar' | 'editar' = 'criar';
  @Output() fabricanteSalvo = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  private session: any = {};
  public tiposrecursos: any = [];
  public categorias: any = [];
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
      tipo: ['', Validators.required],
      descricao: ['', Validators.required]
    })
  }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    if (!this.fabricante) {
      this.fabricante = {};
    }
    
    // Definir cliente se for criação
    if (this.modo === 'criar') {
      this.fabricante.cliente = this.session.usuario.cliente;
    }
    
    // Carregar categorias disponíveis
    this.carregarCategorias();
    
    // Configurar validações condicionais
    this.configurarValidacoes();
    
    // Se for edição, preencher o formulário
    if (this.modo === 'editar' && this.fabricante?.id) {
      this.form.patchValue({
        categoriaId: this.fabricante.categoriaId || this.fabricante.categoria_id,
        tipo: this.fabricante.tipoequipamento,
        descricao: this.fabricante.descricao
      });
      if (this.fabricante.categoriaId || this.fabricante.categoria_id) {
        this.carregarTiposRecursos(this.fabricante.categoriaId || this.fabricante.categoria_id);
      }
    }
  }

  getTitulo(): string {
    return this.modo === 'criar' ? 'Novo Fabricante' : 'Editar Fabricante';
  }

  getBotaoTexto(): string {
    return this.modo === 'criar' ? 'Criar' : 'Salvar';
  }

  /**
   * Configura validações condicionais do formulário
   */
  private configurarValidacoes() {
    // Desabilitar campo tipo até categoria ser selecionada
    this.form.get('tipo')?.disable();
    
    // Habilitar/desabilitar campo tipo baseado na categoria
    this.form.get('categoriaId')?.valueChanges.subscribe(categoriaId => {
      if (categoriaId) {
        this.form.get('tipo')?.enable();
      } else {
        this.form.get('tipo')?.disable();
        this.form.get('tipo')?.setValue('');
      }
    });
  }

  /**
   * Remove duplicatas por ID (mantém apenas o primeiro registro de cada ID)
   */
  private removerDuplicatasPorId(tipos: any[]): any[] {
    const idsVistos = new Set();
    return tipos.filter(tipo => {
      if (idsVistos.has(tipo.id)) {
        return false; // Já vimos este ID, remover
      }
      idsVistos.add(tipo.id);
      return true; // Primeira vez vendo este ID, manter
    });
  }

  /**
   * Filtra tipos de recursos que não devem ser exibidos na grid
   * Mantém configurações internas como LINHA TELEFÔNICA ocultas
   */
  private filtrarTiposExcluidos(tipos: any[]): any[] {
    return tipos.filter(tipo => {
      const descricao = tipo.descricao?.toLowerCase() || '';
      
      // Tipos que não devem ser exibidos (configurações internas)
      const tiposExcluidos = [
        'linha telefônica',
        'linha telefonica',
        'linha telefónica',
        'linha telefonica'
      ];
      
      return !tiposExcluidos.includes(descricao);
    });
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

  /**
   * Carrega tipos de recursos filtrados por categoria
   */
  private async carregarTiposRecursos(categoriaId: number) {
    try {
      this.util.aguardar(true);
      
      const response = await this.api.listarTiposRecursos("null", this.session.usuario.cliente, this.session.token);
      this.util.aguardar(false);
      if (response && response.status === 200) {
        const tiposFiltrados = response.data.filter((tipo: any) => {
          const tipoCategoriaId = Number(tipo.categoriaId);
          const categoriaSelecionada = Number(categoriaId);
          const match = tipoCategoriaId === categoriaSelecionada;
          return match;
        });
        
        // Remover duplicatas por ID
        const tiposSemDuplicatas = this.removerDuplicatasPorId(tiposFiltrados);
        
        // Filtrar tipos que não devem ser exibidos (configurações internas)
        this.tiposrecursos = this.filtrarTiposExcluidos(tiposSemDuplicatas);
      } else {
        this.tiposrecursos = [];
      }
    } catch (error) {
      this.util.aguardar(false);
      console.error('[FABRICANTE-COMPONENT] Erro ao carregar tipos de recursos:', error);
      this.tiposrecursos = [];
    }
  }

  /**
   * Handler para mudança de categoria
   */
  onCategoriaChange(event: any) {
    const categoriaId = event.target.value;
    this.form.patchValue({ tipo: '' });
    this.tiposrecursos = [];
    
    if (categoriaId) {
      this.carregarTiposRecursos(Number(categoriaId));
    } else {
    }
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
      // Preparar os dados para envio
      const dadosFabricante = {
        id: this.fabricante.id || 0,
        cliente: this.fabricante.cliente,
        categoriaId: this.form.get('categoriaId')?.value,
        categoria_id: this.form.get('categoriaId')?.value, // 🆕 Incluir ambos os campos para compatibilidade
        tipoequipamento: this.form.value.tipo,
        descricao: this.form.value.descricao,
        ativo: this.fabricante.ativo !== undefined ? this.fabricante.ativo : true
      };
      this.util.aguardar(true);
      this.api.salvarFabricante(dadosFabricante, this.session.token).then(res => {
        this.carregando = false;
        this.util.aguardar(false);
        if(res.status !== undefined && res.status !== null && res.status === 200){
          this.util.exibirMensagemToast('Fabricante salvo com sucesso!', 5000);
          this.fabricanteSalvo.emit(res.data); // Emitir evento para o componente pai
        }
        else if(res.response && res.response.status == 409) {
          this.util.exibirMensagemToast(res.response.data.Mensagem || res.response.data, 5000);
        }
        else  {
          this.util.exibirFalhaComunicacao();
        }
      }).catch(error => {
        this.carregando = false;
        console.error('=== ERRO AO SALVAR ===');
        console.error('Erro completo:', error);
        this.util.aguardar(false);
        this.util.exibirFalhaComunicacao();
      });
    } else {
    }
  }

  cancelar() {
    this.cancelado.emit();
  }

}
