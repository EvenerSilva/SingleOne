import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { CategoriasApiService } from 'src/app/api/categorias/categorias-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-modelo',
  templateUrl: './modelo.component.html',
  styleUrls: ['./modelo.component.scss']
})
export class ModeloComponent implements OnInit {

  private session:any = {};
  public modelo:any = {};
  public fabricantes:any = [];
  public tiporecursos:any = [];
  public categorias:any = [];
  public form: FormGroup;
  public modo: 'criar' | 'editar' = 'criar';
  public carregando = false;

  constructor(
    private fb: FormBuilder, private util: UtilService, private api: ConfiguracoesApiService,
    private categoriasApi: CategoriasApiService, private ar: ActivatedRoute, private route: Router) {
      this.form = this.fb.group({
        categoria: ['', Validators.required],
        tipo: ['', Validators.required],
        fabricante: ['', Validators.required],
        descricao: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]]
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.modelo.cliente = this.session.usuario.cliente;
    this.util.aguardar(true);
    this.categoriasApi.listarCategorias().then(res => {
      if(res.status != 200 && res.status != 204){
        this.util.exibirFalhaComunicacao();
        this.util.aguardar(false);
      }
      else {
        // ✅ CORREÇÃO: Verificar estrutura da resposta (res.data.dados ou res.data)
        if (res.data && res.data.dados && Array.isArray(res.data.dados)) {
          this.categorias = res.data.dados;
        } else if (Array.isArray(res.data)) {
          this.categorias = res.data;
        } else {
          this.categorias = [];
        }
        this.util.aguardar(false);
        
        // ✅ CORREÇÃO: Verificar parâmetro de rota ANTES de carregar tipos
        this.ar.paramMap.subscribe(param => {
          var parametro = param.get('id');
          if(parametro != null) {
            // MODO EDIÇÃO: Carregar todos os tipos e fabricantes
            this.modo = 'editar';
            this.modelo = JSON.parse(atob(parametro));
            this.util.aguardar(true);
            
            // Carregar tipos de recursos para popular o dropdown
            this.api.listarTiposRecursos("null", this.session.usuario.cliente, this.session.token).then(res => {
              if(res.status != 200 && res.status != 204){
                this.util.exibirFalhaComunicacao();
                this.util.aguardar(false);
              }
              else {
                // ✅ CORREÇÃO: Verificar estrutura da resposta
                if (res.data && res.data.dados && Array.isArray(res.data.dados)) {
                  this.tiporecursos = res.data.dados;
                } else if (Array.isArray(res.data)) {
                  this.tiporecursos = res.data;
                } else {
                  this.tiporecursos = [];
                }
                this.modelo.tipo = this.modelo.fabricanteNavigation.tipoequipamento;
                
                // Filtrar tipos pela categoria do modelo
                const categoriaModelo = this.modelo.fabricanteNavigation.tipoequipamentoNavigation.categoria;
                // ✅ CORREÇÃO: A API retorna 'categoriaId', não 'categoria'
                this.tiporecursos = this.tiporecursos.filter((tipo: any) => tipo.categoriaId === categoriaModelo);
                
                // Carregar fabricantes
                this.listarFabricantesPorTipoRecurso();
                
                // Inicializar o formulário com os dados existentes
                this.form.patchValue({
                  categoria: categoriaModelo,
                  tipo: this.modelo.tipo,
                  fabricante: this.modelo.fabricante,
                  descricao: this.modelo.descricao
                });
                
                this.util.aguardar(false);
              }
            });
          } else {
            // MODO CRIAÇÃO: Formulário limpo
            this.modo = 'criar';
            this.limparFormularioPrivado();
          }
        });
      }
    });
  }

  private limparFormularioPrivado() {
    this.modelo = {
      cliente: this.session.usuario.cliente,
      ativo: true
    };
    this.form.reset();
    this.tiporecursos = [];
    this.fabricantes = [];
    
    // ✅ Desabilitar campos dependentes no início
    this.form.get('tipo')?.disable();
    this.form.get('fabricante')?.disable();
  }

  getTitulo(): string {
    return this.modo === 'criar' ? 'Novo Modelo' : 'Editar Modelo';
  }

  getBotaoTexto(): string {
    return this.modo === 'criar' ? 'Criar' : 'Salvar';
  }

  onCategoriaChange() {
    // Limpar campos dependentes
    this.modelo.tipo = '';
    this.modelo.fabricante = '';
    this.form.patchValue({ tipo: '', fabricante: '' });
    this.tiporecursos = [];
    this.fabricantes = [];
    
    // Desabilitar campos dependentes
    this.form.get('fabricante')?.disable();
    
    // Filtrar tipos de recursos por categoria
    if (this.modelo.categoria) {
      this.util.aguardar(true);
      
      this.api.listarTiposRecursos("null", this.session.usuario.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        
        if(res.status == 200 || res.status == 204) {
          // ✅ CORREÇÃO: Verificar estrutura da resposta
          let todosTipos = [];
          if (res.data && res.data.dados && Array.isArray(res.data.dados)) {
            todosTipos = res.data.dados;
          } else if (Array.isArray(res.data)) {
            todosTipos = res.data;
          }
          
          // Filtrar apenas tipos da categoria selecionada
          // ✅ CORREÇÃO: A API retorna 'categoriaId', não 'categoria'
          const categoriaId = Number(this.modelo.categoria);
          this.tiporecursos = todosTipos.filter((tipo: any) => {
            return Number(tipo.categoriaId) === categoriaId;
          });
          if (this.tiporecursos.length > 0) {
            this.form.get('tipo')?.enable();
          }
        }
      }).catch(err => {
        console.error('[MODELO] Erro ao carregar tipos:', err);
        this.util.aguardar(false);
      });
    } else {
      // Se não há categoria, desabilitar tipo e fabricante
      this.form.get('tipo')?.disable();
      this.form.get('fabricante')?.disable();
    }
  }

  onTipoRecursoChange() {
    // Limpar campo dependente
    this.modelo.fabricante = '';
    this.form.patchValue({ fabricante: '' });
    this.fabricantes = [];
    
    // Carregar fabricantes do tipo de recurso selecionado
    if (this.modelo.tipo) {
      this.listarFabricantesPorTipoRecurso();
    } else {
      // Se não há tipo, desabilitar fabricante
      this.form.get('fabricante')?.disable();
    }
  }

  listarFabricantesPorTipoRecurso(){
    this.util.aguardar(true);
    
    this.api.listarFabricantesPorTipoRecurso(this.modelo.tipo, this.session.usuario.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      
      if(res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        // ✅ CORREÇÃO: Verificar estrutura da resposta
        if (res.data && res.data.dados && Array.isArray(res.data.dados)) {
          this.fabricantes = res.data.dados;
        } else if (Array.isArray(res.data)) {
          this.fabricantes = res.data;
        } else {
          this.fabricantes = [];
        }
        if (this.fabricantes.length > 0) {
          this.form.get('fabricante')?.enable();
        }
      }
    }).catch(err => {
      console.error('[MODELO] Erro ao carregar fabricantes:', err);
      this.util.aguardar(false);
    });
  }

  salvar() {
    // ✅ Habilitar todos os campos temporariamente para validação e salvamento
    this.form.get('tipo')?.enable();
    this.form.get('fabricante')?.enable();
    
    if(this.form.valid) {
      // Validar se o modelo já existe
      if (!this.modelo.id) {
        this.validarModeloExistente();
        return;
      }
      
      this.executarSalvamento();
    } else {
      this.marcarCamposInvalidos();
    }
  }

  private validarModeloExistente() {
    const dadosModelo = {
      cliente: this.modelo.cliente,
      fabricante: this.form.value.fabricante,
      descricao: this.form.value.descricao
    };

    this.util.aguardar(true);
    this.api.listarModelos("null", this.modelo.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200 || res.status === 204) {
        const modeloExistente = res.data?.find((m: any) => 
          m.fabricante === dadosModelo.fabricante && 
          m.descricao.toLowerCase() === dadosModelo.descricao.toLowerCase() &&
          m.ativo
        );
        if (modeloExistente) {
          this.util.exibirMensagemToast('Já existe um modelo ativo com este nome para este fabricante!', 5000);
          return;
        }
      }
      
      this.executarSalvamento();
    }).catch(() => {
      this.util.aguardar(false);
      this.executarSalvamento();
    });
  }

  private executarSalvamento() {
    // Preparar os dados para envio
    const dadosModelo = {
      id: this.modelo.id || 0,
      cliente: this.modelo.cliente,
      fabricante: this.form.value.fabricante,
      descricao: this.form.value.descricao,
      ativo: this.modelo.ativo !== undefined ? this.modelo.ativo : true
    };
    this.util.aguardar(true);
    this.api.salvarModelo(dadosModelo, this.session.token).then(res => {
      this.util.aguardar(false);

      if(res.status !== undefined && res.status !== null && res.status === 200){
        this.util.exibirMensagemToast('Modelo salvo com sucesso!', 5000);
        this.route.navigate(['/modelos']);
      }
      else if(res.response && res.response.status == 409) {
        this.util.exibirMensagemToast(res.response.data.Mensagem || res.response.data, 5000);
      }
      else  {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(error => {
      console.error('=== ERRO AO SALVAR ===');
      console.error('Erro completo:', error);
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  private marcarCamposInvalidos() {
    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      if (control?.invalid) {
        control.markAsTouched();
      }
    });
    
    this.util.exibirMensagemToast('Por favor, preencha todos os campos obrigatórios corretamente.', 5000);
  }

  limparFormulario() {
    if (confirm('Tem certeza que deseja limpar o formulário? Todos os dados serão perdidos.')) {
      this.limparFormularioPrivado();
    }
  }
}
