import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-equipamento',
  templateUrl: './equipamento.component.html',
  styleUrls: ['./equipamento.component.scss']
})
export class EquipamentoComponent implements OnInit {

  private session:any = {};
  public equipamento:any = {
    equipamentostatus: 3,
    ativo: true,
    possuibo: false
  };
  public tipos:any = [];
  public fabricantes:any = [];
  public modelos:any = [];
  public notasfiscais:any = [];
  public localidades:any = [];
  public form: FormGroup;
  public cep:any = {};
  public empresas:any = [];
  public centros:any = [];
  public tiposaquisicao:any = [];
  public fornecedores:any = [];
  public filiais:any = [];

  // Modal de Boletim de Ocorrência
  public showBoModal = false;
  public boData: any = {
    descricao: '',
    anexos: []
  };
  public selectedFiles: File[] = [];
  public saving = false;

  constructor(private fb: FormBuilder, private util: UtilService, private api: EquipamentoApiService,
    private apiCad: ConfiguracoesApiService, private ar: ActivatedRoute, private route: Router) {
      this.form = this.fb.group({
        tipoequipamento: ['', Validators.required],
        fabricante: ['', Validators.required],
        modelo: ['', Validators.required],
        notafiscal: [''],
        localidade: [''],
        tipoaquisicao: ['', Validators.required],
        numeroserie: ['', Validators.required],
        patrimonio: [''],
        dtlimitegarantia: [''],
        empresa: [''],
        centrocusto: [''],
        fornecedor: [''],
        filialId: [''],
        possuibo: [false],
        descricaobo: ['']
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.equipamento.cliente = this.session.usuario.cliente;
    this.equipamento.usuario = this.session.usuario.id;

    this.util.aguardar(true);
    this.apiCad.listarTiposRecursos("null", this.equipamento.cliente, this.session.token).then(res => {
      this.tipos = res.data;
      this.apiCad.listarNotasFiscais("null", this.equipamento.cliente, this.session.token).then(res => {
        this.notasfiscais = res.data;
        
        // ✅ DEBUG: Verificar estrutura das notas fiscais
        if (this.notasfiscais && this.notasfiscais.length > 0) {
          this.notasfiscais.forEach((nf, index) => {
          });
        }
        this.apiCad.listarLocalidades(this.equipamento.cliente, this.session.token).then(res => {
          this.localidades = res.data;
          this.util.aguardar(false);
          this.apiCad.listarEmpresas("null", this.equipamento.cliente, this.session.token).then(res => {
            this.empresas = res.data;
            this.apiCad.listarTiposAquisicao(this.session.token).then(res => {
              this.tiposaquisicao = res.data;
              this.carregarDadosAdicionais();
            });
          });
          this.ar.paramMap.subscribe(param => {
            var parametro = param.get('id');
            if(parametro != null) {
              this.equipamento.id = parametro;
              this.buscarEquipamentoPorId();
            }
          })
        })
      })
    })
  }

  buscarEquipamentoPorId(){
    if (!this.equipamento.id || this.equipamento.id === '') {
      this.util.exibirMensagemToast('ID do recurso inválido', 5000);
      return;
    }
    
    this.util.aguardar(true);
    this.api.buscarEquipamentoPorId(this.equipamento.id, this.session.token).then(res => {
      this.util.aguardar(false);
      
      if (res.status === 200 && res.data) {
        this.equipamento = res.data;
        
        // Mapear campos para compatibilidade
        this.mapearCamposParaCompatibilidade();
        
        // ✅ CORREÇÃO: Aguardar um pouco para garantir que as listas estejam carregadas
        setTimeout(() => {
          // Sincronizar dados com o formulário
          this.sincronizarFormulario();
          
          this.listarFabricantesDoTipoEquipamento();
          this.listarModelosDoFabricante();
          if(this.equipamento.empresa !== null){
            this.listarCentrosCustos();
          }
        }, 200);
      } else {
        console.error('❌ [EQUIPAMENTO] Erro ao carregar equipamento:', res);
        this.util.exibirMensagemToast('Erro ao carregar dados do recurso', 5000);
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('❌ [EQUIPAMENTO] Erro na API:', error);
      this.util.exibirMensagemToast('Erro ao carregar recurso', 5000);
    })
  }

  mapearCamposParaCompatibilidade() {
    if (this.equipamento.tipoequipamentoNavigation && this.equipamento.tipoequipamento === 0) {
      this.equipamento.tipoequipamento = this.equipamento.tipoequipamentoNavigation.id;
    }
    
    if (this.equipamento.fabricanteNavigation && this.equipamento.fabricante === 0) {
      this.equipamento.fabricante = this.equipamento.fabricanteNavigation.id;
    }
    
    if (this.equipamento.modeloNavigation && this.equipamento.modelo === 0) {
      this.equipamento.modelo = this.equipamento.modeloNavigation.id;
    }
    
    if (this.equipamento.empresaNavigation && this.equipamento.empresa === null) {
      this.equipamento.empresa = this.equipamento.empresaNavigation.id;
    }
    
    if (this.equipamento.localidadeNavigation && this.equipamento.localidade === null) {
      this.equipamento.localidade = this.equipamento.localidadeNavigation.id;
    }
    
    // Mapear localidade de diferentes fontes
    if (!this.equipamento.localidade) {
      if (this.equipamento.localidadeId) {
        this.equipamento.localidade = this.equipamento.localidadeId;
      } else if (this.equipamento.localizacao) {
        this.equipamento.localidade = this.equipamento.localizacao;
      } else if (this.equipamento.localidadeId) {
        this.equipamento.localidade = this.equipamento.localidadeId;
      }
    }

    // Mapear empresa se necessário
    if (!this.equipamento.empresa && this.equipamento.empresaId) {
      this.equipamento.empresa = this.equipamento.empresaId;
    }

    // Mapear centro de custo se necessário
    if (!this.equipamento.centrocusto && this.equipamento.centrocustoId) {
      this.equipamento.centrocusto = this.equipamento.centrocustoId;
    }

    // Mapear filial se necessário
    if (!this.equipamento.filialId && this.equipamento.filial) {
      this.equipamento.filialId = this.equipamento.filial;
    }

    // Mapear tipo de recurso se necessário
    if (!this.equipamento.tipoequipamento && this.equipamento.tipoEquipamento) {
      this.equipamento.tipoequipamento = this.equipamento.tipoEquipamento;
    }

    // Mapear fabricante se necessário
    if (!this.equipamento.fabricante && this.equipamento.fabricanteId) {
      this.equipamento.fabricante = this.equipamento.fabricanteId;
    }

    // Mapear modelo se necessário
    if (!this.equipamento.modelo && this.equipamento.modeloId) {
      this.equipamento.modelo = this.equipamento.modeloId;
    }

    // Mapear tipo de aquisição se necessário
    if (!this.equipamento.tipoaquisicao && this.equipamento.tipoAquisicao) {
      this.equipamento.tipoaquisicao = this.equipamento.tipoAquisicao;
    }

    // Mapear número de série se necessário
    if (!this.equipamento.numeroserie && this.equipamento.numeroSerie) {
      this.equipamento.numeroserie = this.equipamento.numeroSerie;
    }

    // Mapear patrimônio se necessário
    if (!this.equipamento.patrimonio && this.equipamento.patrimonioId) {
      this.equipamento.patrimonio = this.equipamento.patrimonioId;
    }

    // Mapear data limite de garantia se necessário
    if (!this.equipamento.dtlimitegarantia && this.equipamento.dataLimiteGarantia) {
      this.equipamento.dtlimitegarantia = this.equipamento.dataLimiteGarantia;
    }

    // ✅ CORREÇÃO: Formatar data de garantia para o formato correto do input HTML
    if (this.equipamento.dtlimitegarantia) {
      this.equipamento.dtlimitegarantia = this.formatarDataParaInput(this.equipamento.dtlimitegarantia);
    }

    // Mapear fornecedor se necessário
    if (!this.equipamento.fornecedor && this.equipamento.fornecedorId) {
      this.equipamento.fornecedor = this.equipamento.fornecedorId;
    }

    // Mapear status do recurso se necessário
    if (!this.equipamento.equipamentostatus && this.equipamento.statusEquipamento) {
      this.equipamento.equipamentostatus = this.equipamento.statusEquipamento;
    }

    // Mapear nota fiscal se necessário
    if (!this.equipamento.notafiscal && this.equipamento.notaFiscal) {
      this.equipamento.notafiscal = this.equipamento.notaFiscal;
    }

    // ✅ CORREÇÃO: Definir fornecedor automaticamente baseado na nota fiscal ao carregar
    if (this.equipamento.notafiscal && this.notasfiscais.length > 0) {
      const notaFiscal = this.notasfiscais.find(nf => nf.id === this.equipamento.notafiscal);
      if (notaFiscal && notaFiscal.fornecedorNavigation) {
        this.equipamento.fornecedor = notaFiscal.fornecedorNavigation.id;
      }
    }

    // ✅ CORREÇÃO: Não mostrar "Padrão" como localização
    if (this.equipamento.localidade === 1) {
      this.equipamento.localidade = null;
    }
    
    // Garantir que campos numéricos sejam números
    this.equipamento.localidade = this.equipamento.localidade ? parseInt(this.equipamento.localidade) : null;
    this.equipamento.empresa = this.equipamento.empresa ? parseInt(this.equipamento.empresa) : null;
    this.equipamento.centrocusto = this.equipamento.centrocusto ? parseInt(this.equipamento.centrocusto) : null;
    this.equipamento.filialId = this.equipamento.filialId ? parseInt(this.equipamento.filialId) : null;
    this.equipamento.tipoequipamento = this.equipamento.tipoequipamento ? parseInt(this.equipamento.tipoequipamento) : null;
    this.equipamento.fabricante = this.equipamento.fabricante ? parseInt(this.equipamento.fabricante) : null;
    this.equipamento.modelo = this.equipamento.modelo ? parseInt(this.equipamento.modelo) : null;
    this.equipamento.tipoaquisicao = this.equipamento.tipoaquisicao ? parseInt(this.equipamento.tipoaquisicao) : null;
    this.equipamento.fornecedor = this.equipamento.fornecedor ? parseInt(this.equipamento.fornecedor) : null;
    this.equipamento.equipamentostatus = this.equipamento.equipamentostatus ? parseInt(this.equipamento.equipamentostatus) : null;
    this.equipamento.notafiscal = this.equipamento.notafiscal ? parseInt(this.equipamento.notafiscal) : null;
    
    this.carregarDadosNavegacao();
  }
  
  carregarDadosNavegacao() {
    setTimeout(() => {
      // Carregar tipo de recurso se tiver ID
      if (this.equipamento.tipoequipamento && this.equipamento.tipoequipamento > 0) {
        this.listarFabricantesDoTipoEquipamento();
      }
      
      // Carregar fabricante se tiver ID
      if (this.equipamento.fabricante && this.equipamento.fabricante > 0) {
        this.listarModelosDoFabricante();
      }
      
      // Carregar empresa se tiver ID
      if (this.equipamento.empresa && this.equipamento.empresa > 0) {
        this.listarCentrosCustos();
      }
    }, 100); // Aguardar 100ms para garantir que os dados básicos foram carregados
  }

  sincronizarFormulario() {
    const tratarValor = (valor: any, tipo: 'string' | 'number' | 'boolean' = 'string') => {
      if (valor === null || valor === undefined) {
        return tipo === 'number' ? null : (tipo === 'boolean' ? false : '');
      }
      
      // ✅ CORREÇÃO: Para números, preservar valores válidos (incluindo 0)
      if (tipo === 'number') {
        const valorNumerico = parseInt(valor);
        return isNaN(valorNumerico) ? null : valorNumerico;
      }
      return valor;
    };

    // ✅ CORREÇÃO: Popular o formulário com os dados do recurso
    const valoresFormulario = {
      tipoequipamento: tratarValor(this.equipamento.tipoequipamento, 'number'),
      fabricante: tratarValor(this.equipamento.fabricante, 'number'),
      modelo: tratarValor(this.equipamento.modelo, 'number'),
      notafiscal: tratarValor(this.equipamento.notafiscal, 'number'),
      localidade: tratarValor(this.equipamento.localidade, 'number'),
      tipoaquisicao: tratarValor(this.equipamento.tipoaquisicao, 'number'),
      numeroserie: tratarValor(this.equipamento.numeroserie, 'string'),
      patrimonio: tratarValor(this.equipamento.patrimonio, 'string'),
      dtlimitegarantia: this.formatarDataParaInput(tratarValor(this.equipamento.dtlimitegarantia, 'string')),
      empresa: tratarValor(this.equipamento.empresa, 'number'),
      centrocusto: tratarValor(this.equipamento.centrocusto, 'number'),
      fornecedor: tratarValor(this.equipamento.fornecedor, 'number'),
      filialId: tratarValor(this.equipamento.filialId, 'number'),
      possuibo: tratarValor(this.equipamento.possuibo, 'boolean'),
      descricaobo: tratarValor(this.equipamento.descricaobo, 'string')
    };
    this.form.patchValue(valoresFormulario);
    
    // ✅ DEBUG: Verificar valores específicos após patchValue
    // ✅ DEBUG: Verificar se as listas estão carregadas
    if (this.empresas && this.empresas.length > 0 && valoresFormulario.empresa) {
      const empresaEncontrada = this.empresas.find(e => e.id === valoresFormulario.empresa);
    }
    
    if (this.localidades && this.localidades.length > 0 && valoresFormulario.localidade) {
      const localidadeEncontrada = this.localidades.find(l => l.id === valoresFormulario.localidade);
    }
    const camposPreenchidos = Object.keys(valoresFormulario).filter(key => {
      const valor = valoresFormulario[key];
      return valor !== null && valor !== undefined && valor !== '' && valor !== 0 && valor !== false;
    });
  }

  listarFabricantesDoTipoEquipamento(){
    // ✅ CORREÇÃO: Verificar se temos valores válidos antes de fazer a chamada
    if (!this.equipamento.tipoequipamento || this.equipamento.tipoequipamento === 0) {
      this.fabricantes = [];
      return;
    }
    
    this.util.aguardar(true);
    this.apiCad.listarFabricantesPorTipoRecurso(this.equipamento.tipoequipamento, this.equipamento.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        this.fabricantes = res.data || [];
      } else {
        console.error('❌ [EQUIPAMENTO] Erro ao carregar fabricantes:', res);
        this.fabricantes = [];
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('❌ [EQUIPAMENTO] Erro na API de fabricantes:', error);
      this.fabricantes = [];
    });
  }

  listarModelosDoFabricante(){
    // ✅ CORREÇÃO: Verificar se temos valores válidos antes de fazer a chamada
    if (!this.equipamento.fabricante || this.equipamento.fabricante === 0) {
      this.modelos = [];
      return;
    }
    
    this.util.aguardar(true);
    this.apiCad.listarModelosDoFabricante(this.equipamento.fabricante, this.equipamento.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        this.modelos = res.data || [];
      } else {
        console.error('❌ [EQUIPAMENTO] Erro ao carregar modelos:', res);
        this.modelos = [];
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('❌ [EQUIPAMENTO] Erro na API de modelos:', error);
      this.modelos = [];
    });
  }

  salvar() {
    // ✅ CAMADA DE SEGURANÇA: Validar status antes de salvar (caso usuário acesse via URL direta)
    // Status 8 = Roubado, Status 10 = Descartado
    if (this.equipamento.id > 0 && (this.equipamento.equipamentostatus === 8 || this.equipamento.equipamentostatus === 10)) {
      const statusDescricao = this.equipamento.equipamentostatus === 8 ? 'Roubado' : 'Descartado';
      this.util.exibirMensagemToast(`Não é possível editar equipamentos com status: ${statusDescricao}`, 5000);
      this.route.navigate(['/recursos']);
      return;
    }
    
    if(this.form.valid) {
      // Sincronizar dados do formulário com o objeto recurso
      
      // ✅ CORREÇÃO: Sincronizar TODOS os dados do formulário com o objeto recurso
      const formValues = this.form.value;
      this.equipamento.tipoequipamento = formValues.tipoequipamento;
      this.equipamento.fabricante = formValues.fabricante;
      this.equipamento.modelo = formValues.modelo;
      this.equipamento.notafiscal = formValues.notafiscal;
      this.equipamento.localidade = formValues.localidade;
      this.equipamento.tipoaquisicao = formValues.tipoaquisicao;
      this.equipamento.numeroserie = formValues.numeroserie;
      this.equipamento.patrimonio = formValues.patrimonio;
      this.equipamento.dtlimitegarantia = this.formatarDataParaBackend(formValues.dtlimitegarantia);
      this.equipamento.empresa = formValues.empresa;
      this.equipamento.centrocusto = formValues.centrocusto;
      this.equipamento.fornecedor = formValues.fornecedor;
      this.equipamento.filialId = formValues.filialId;
      this.equipamento.possuibo = formValues.possuibo;
      this.equipamento.descricaobo = formValues.descricaobo;

this.util.aguardar(true);
      this.api.salvarEquipamento(this.equipamento, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          if(res.data.Mensagem != undefined) {
            this.util.exibirMensagemToast(res.data.Mensagem, 5000);
            this.route.navigate(['/recursos']);
          }
          else {
            this.util.exibirMensagemToast('Recurso salvo com sucesso!', 5000);
            this.route.navigate(['/recursos']);
          }
        }
      })
    }
  }

listarCentrosCustos() {
    // ✅ CORREÇÃO: Verificar se temos empresa válida antes de fazer a chamada
    if (!this.equipamento.empresa || this.equipamento.empresa === 0) {
      this.centros = [];
      return;
    }
    
    // ✅ CORREÇÃO: Verificar se as empresas foram carregadas
    if (!this.empresas || this.empresas.length === 0) {
      setTimeout(() => this.listarCentrosCustos(), 200);
      return;
    }
    
    this.util.aguardar(true);
    this.apiCad.listarCentroCustoDaEmpresa(this.equipamento.empresa, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status === 200 || res.status === 204) {
        this.centros = res.data || [];
      } else {
        console.error('❌ [EQUIPAMENTO] Erro ao carregar centros de custo:', res);
        this.centros = [];
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('❌ [EQUIPAMENTO] Erro na API de centros de custo:', error);
      this.centros = [];
    });
  }

  carregarDadosAdicionais() {
    // ✅ CORREÇÃO: Carregar fornecedores com parâmetros corretos
    this.apiCad.listarFornecedores("", this.equipamento.cliente, this.session.token).then(res => {
      if(res.status === 200) {
        this.fornecedores = res.data || [];
      } else {
        console.error('❌ [EQUIPAMENTO] Erro ao carregar fornecedores:', res);
        this.fornecedores = [];
      }
    }).catch(error => {
      console.error('❌ [EQUIPAMENTO] Erro na API de fornecedores:', error);
      this.fornecedores = [];
    });

    // ✅ CORREÇÃO: Carregar filiais com parâmetros corretos
    this.apiCad.listarFiliais("", this.equipamento.cliente, this.session.token).then(res => {
      if(res.status === 200) {
        this.filiais = res.data || [];
      } else {
        console.error('❌ [EQUIPAMENTO] Erro ao carregar filiais:', res);
        this.filiais = [];
      }
    }).catch(error => {
      console.error('❌ [EQUIPAMENTO] Erro na API de filiais:', error);
      this.filiais = [];
    });

  }

  limparFormulario() {
    this.equipamento = {
      equipamentostatus: 3,
      ativo: true,
      possuibo: false,
      cliente: this.session.usuario.cliente,
      usuario: this.session.usuario.id
    };
    this.form.reset();
    this.fabricantes = [];
    this.modelos = [];
    this.centros = [];
    this.fornecedores = [];
    this.filiais = [];
  }

  cancelar(event?: Event) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.route.navigate(['/recursos']);
  }

  onNotaFiscalChange(event: any) {
    const notaFiscalId = event.value;
    if (notaFiscalId) {
      // Buscar a nota fiscal selecionada
      const notaFiscal = this.notasfiscais.find(nf => nf.id === notaFiscalId);
      
      if (notaFiscal && notaFiscal.fornecedorNavigation) {
        // Definir o fornecedor automaticamente baseado na nota fiscal
        this.equipamento.fornecedor = notaFiscal.fornecedorNavigation.id;
        
        // Atualizar o formulário
        this.form.patchValue({
          fornecedor: notaFiscal.fornecedorNavigation.id
        });
      } else {
        console.warn('⚠️ [EQUIPAMENTO] Nota fiscal selecionada não possui fornecedor associado');
        // Limpar fornecedor se a nota fiscal não tiver fornecedor
        this.equipamento.fornecedor = null;
        this.form.patchValue({ fornecedor: null });
      }
    } else {
      // Se nenhuma nota fiscal foi selecionada, limpar o fornecedor
      this.equipamento.fornecedor = null;
      this.form.patchValue({ fornecedor: null });
    }
  }

  formatarDataParaInput(data: any): string {
    if (!data) return '';
    
    try {
      // Se já está no formato correto (yyyy-MM-dd), retorna como está
      if (typeof data === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(data)) {
        return data;
      }
      
      // Se é uma data com hora, extrai apenas a parte da data
      const dataObj = new Date(data);
      if (isNaN(dataObj.getTime())) {
        console.warn('⚠️ [EQUIPAMENTO] Data inválida recebida:', data);
        return '';
      }
      
      // Formatar para yyyy-MM-dd
      const ano = dataObj.getFullYear();
      const mes = String(dataObj.getMonth() + 1).padStart(2, '0');
      const dia = String(dataObj.getDate()).padStart(2, '0');
      
      return `${ano}-${mes}-${dia}`;
    } catch (error) {
      console.error('❌ [EQUIPAMENTO] Erro ao formatar data:', error, 'Data original:', data);
      return '';
    }
  }

  formatarDataParaBackend(data: string): string {
    if (!data) return '';
    
    try {
      // Se está no formato yyyy-MM-dd, adiciona hora para o backend
      if (/^\d{4}-\d{2}-\d{2}$/.test(data)) {
        return `${data}T00:00:00`;
      }
      
      return data;
    } catch (error) {
      console.error('❌ [EQUIPAMENTO] Erro ao formatar data para backend:', error, 'Data original:', data);
      return '';
    }
  }

  onDataGarantiaChange() {
    // Força a atualização do status da garantia quando a data muda
  }

  getStatusGarantia(): { text: string, class: string } | null {
    if (!this.equipamento.dtlimitegarantia) {
      return null;
    }

    try {
      const dataGarantia = new Date(this.equipamento.dtlimitegarantia);
      const hoje = new Date();
      
      // Zerar as horas para comparar apenas as datas
      hoje.setHours(0, 0, 0, 0);
      dataGarantia.setHours(0, 0, 0, 0);
      
      if (isNaN(dataGarantia.getTime())) {
        return null;
      }

      const diferencaMs = dataGarantia.getTime() - hoje.getTime();
      const diferencaDias = Math.ceil(diferencaMs / (1000 * 60 * 60 * 24));

      if (diferencaDias < 0) {
        // Garantia vencida
        const diasVencida = Math.abs(diferencaDias);
        return {
          text: `⚠️ Garantia vencida há ${diasVencida} ${diasVencida === 1 ? 'dia' : 'dias'}`,
          class: 'garantia-vencida'
        };
      } else if (diferencaDias === 0) {
        // Vence hoje
        return {
          text: '⚠️ Garantia vence hoje',
          class: 'garantia-hoje'
        };
      } else if (diferencaDias <= 30) {
        // Vence em breve (30 dias ou menos)
        return {
          text: `⚠️ Garantia vence em ${diferencaDias} ${diferencaDias === 1 ? 'dia' : 'dias'}`,
          class: 'garantia-proximo-vencimento'
        };
      } else {
        // Garantia válida
        return {
          text: `✅ Garantia válida por mais ${diferencaDias} ${diferencaDias === 1 ? 'dia' : 'dias'}`,
          class: 'garantia-valida'
        };
      }
    } catch (error) {
      console.error('❌ [EQUIPAMENTO] Erro ao calcular status da garantia:', error);
      return null;
    }
  }

  // Métodos do Modal de Boletim de Ocorrência
  abrirModalBO() {
    this.boData.descricao = this.equipamento.descricaobo || '';
    this.selectedFiles = [];
    this.showBoModal = true;
  }

  closeBoModal(event: Event) {
    event.stopPropagation();
    this.showBoModal = false;
    this.boData = { descricao: '', anexos: [] };
    this.selectedFiles = [];
  }

  onFileSelected(event: any) {
    const files = event.target.files;
    if (this.selectedFiles.length + files.length > 5) {
      this.util.exibirMensagemToast('Máximo 5 arquivos permitidos', 3000);
      return;
    }
    for (let i = 0; i < files.length; i++) {
      this.selectedFiles.push(files[i]);
    }
  }

  removeFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  salvarBO() {
    if (!this.boData.descricao) {
      this.util.exibirMensagemToast('Descrição é obrigatória', 3000);
      return;
    }

    this.saving = true;
    
    const boData = {
      id: this.equipamento.id,
      possuiBo: 1,
      equipamentoStatusId: 8, // Status para equipamento com problema
      descricaobo: this.boData.descricao,
      usuario: this.session.usuario.id
    };

    this.api.registrarBO(boData, this.session.token).then(res => {
      if (res.status === 200) {
        // Atualizar o recurso local
        this.equipamento.possuibo = true;
        this.equipamento.descricaobo = this.boData.descricao;
        
        this.util.exibirMensagemToast('Boletim de Ocorrência registrado com sucesso!', 3000);
        this.closeBoModal(new Event('click'));
      } else {
        this.util.exibirMensagemToast('Erro ao registrar BO', 3000);
      }
      this.saving = false;
    }).catch(error => {
      console.error('Erro ao salvar BO:', error);
      this.util.exibirMensagemToast('Erro ao registrar BO', 3000);
      this.saving = false;
    });
  }

  // Métodos auxiliares para exibir descrições no modal
  getTipoEquipamentoDescricao(): string {
    const tipo = this.tipos.find(t => t.id === this.equipamento.tipoequipamento);
    return tipo ? tipo.descricao : 'N/A';
  }

  getFabricanteDescricao(): string {
    const fabricante = this.fabricantes.find(f => f.id === this.equipamento.fabricante);
    return fabricante ? fabricante.descricao : 'N/A';
  }

  getModeloDescricao(): string {
    const modelo = this.modelos.find(m => m.id === this.equipamento.modelo);
    return modelo ? modelo.descricao : 'N/A';
  }

}
