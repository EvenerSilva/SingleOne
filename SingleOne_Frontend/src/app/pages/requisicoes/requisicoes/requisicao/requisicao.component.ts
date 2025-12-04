import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { RequisicaoApiService } from 'src/app/api/requisicoes/requisicao-api.service';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UsuarioApiService } from 'src/app/api/usuarios/usuario-api.service';
import { UtilService } from 'src/app/util/util.service';
import { Requisicao, RequisicaoItem } from 'src/app/models/requisicao.interface';

@Component({
  selector: 'app-requisicao',
  templateUrl: './requisicao.component.html',
  styleUrls: ['./requisicao.component.scss']
})
export class RequisicaoComponent implements OnInit {

  private session:any = {};
  public requisicao: Requisicao = {
    id: 0,
    cliente: 0,
    usuariorequisicao: 0,
    tecnicoresponsavel: null, // ✅ CORREÇÃO: Inicializar como null para validação
    requisicaostatus: 1, //Ativa
    colaboradorfinal: undefined,
    dtsolicitacao: new Date(),
    dtprocessamento: undefined,
    assinaturaeletronica: false,
    dtassinaturaeletronica: undefined,
    dtenviotermo: undefined,
    hashrequisicao: undefined,
    migrateid: undefined,
    requisicoesitens: [],
    tiporecurso: 0
  };
  public item:any = [];
  public colunas = ['descricao', 'acao'];
  public equipamentos:any = [];
  public tecnicos:any = [];
  public linhas:any = [];
  public form: FormGroup;
  public recursos = new FormControl();
  public frmLinhas = new FormControl();
  public resultadoEqps: Observable<any>;
  public resultadoLinhas: Observable<any>;
  public dataSource: MatTableDataSource<any>;
  private paramsEquipamentoSelecionado: any = null;

  constructor(private fb: FormBuilder, private util: UtilService, private api: RequisicaoApiService,
    private apiEqp: EquipamentoApiService, private apiTel: TelefoniaApiService, private apiUsu: UsuarioApiService, private ar: ActivatedRoute, private route: Router) {
      this.form = this.fb.group({
        tipo: [''],
        recurso: [''],
        tecnico: ['', Validators.required], // ✅ CORREÇÃO: Campo obrigatório
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    if (!this.session || !this.session.usuario) {
      console.error('[REQUISICAO] Sessão inválida, redirecionando para login');
      this.route.navigate(['/']);
      return;
    }
    
    this.requisicao.cliente = this.session.usuario.cliente;
    this.requisicao.usuariorequisicao = this.session.usuario.id;
    if (!this.requisicao.cliente || this.requisicao.cliente <= 0) {
      console.error('[REQUISICAO] Cliente inválido:', this.requisicao.cliente);
      this.util.exibirMensagemToast('Cliente inválido. Por favor, faça login novamente.', 5000);
      this.route.navigate(['/']);
      return;
    }
    
    // ✅ CORREÇÃO CRÍTICA: Garantir que novas requisições sempre tenham ID = 0
    this.requisicao.id = 0; // Forçar ID = 0 para garantir INSERT
    this.form.patchValue({
      tipo: 0,
      tecnico: null
    });
    
    // ✅ CORREÇÃO: Definir tipo de recurso padrão
    this.requisicao.tiporecurso = 0;
    
    // ✅ NOVO: Capturar parâmetros de query para pré-preenchimento
    this.ar.queryParams.subscribe(params => {
      if (params['equipamentoId'] && params['numeroSerie']) {
        this.paramsEquipamentoSelecionado = params;
      }
    });
    
    // ✅ CORREÇÃO: Carregar técnicos para seleção de responsável provisório
    /*BUsca de equipamentos*/
    this.resultadoEqps = (this.recursos?.valueChanges || new Observable()).pipe(
      debounceTime(1000),
      tap(value => this.buscarEquipamentos(value))
    );
    this.resultadoEqps.subscribe?.();

    /*Busca de linhas*/
    this.resultadoLinhas = (this.frmLinhas?.valueChanges || new Observable()).pipe(
      debounceTime(1000),
      tap(value => this.buscarLinhas(value))
    );
    this.resultadoLinhas.subscribe?.();

this.util.aguardar(true);
    
          // ✅ CORREÇÃO: Carregar equipamentos e responsáveis provisórios em paralelo
      Promise.all([
        this.apiEqp.listarEquipamentosDisponiveis("null", this.requisicao.cliente, this.session.token),
        this.apiUsu.listarUsuarios("null", this.requisicao.cliente, this.session.token)
      ]).then(([equipamentosRes, usuariosRes]) => {
        // Normaliza respostas para arrays
        const eqData = (equipamentosRes && (equipamentosRes.data?.results || equipamentosRes.data)) || [];
        const usData = (usuariosRes && (usuariosRes.data?.results || usuariosRes.data)) || [];
        this.equipamentos = Array.isArray(eqData) ? eqData : [];
        this.tecnicos = Array.isArray(usData) ? usData : [];
        this.util.aguardar(false);
      if (this.paramsEquipamentoSelecionado) {
        this.preencherEquipamentoSelecionado(this.paramsEquipamentoSelecionado);
        this.paramsEquipamentoSelecionado = null; // Limpar após processar
      }

      this.ar.paramMap.subscribe(param => {
        const parametro = param.get('id');
        // Guard: quando a rota é "nova-requisicao" (sem :id), não buscar por ID
        if (parametro != null && parametro !== '0') {
          this.requisicao.id = parseInt(parametro, 10);
          if (!isNaN(this.requisicao.id) && this.requisicao.id > 0) {
            this.buscarPorId();
          } else {
            // Forçar fluxo de nova requisição
            this.requisicao.id = 0;
          }
        }
      });
    }).catch(error => {
      this.util.aguardar(false);
      console.error('[REQUISICAO] Erro ao carregar dados iniciais:', error);
    });
  }

  // ✅ NOVO: Método para selecionar tipo de recurso
  selecionarTipoRecurso(tipo: number) {
    this.requisicao.tiporecurso = tipo;
    
    // Atualizar o formulário
    this.form.patchValue({ tipo: tipo });
    
    // Limpar seleções anteriores
    this.item.equipamento = null;
    this.item.linhatelefonica = null;
    
    // Carregar recursos para o tipo selecionado
    this.carregarRecursos();
  }

  carregarRecursos(){
    this.item.equipamento = null;
    this.item.linhatelefonica = null;
    
    // ✅ CORREÇÃO: Só recarregar se necessário
    if(this.requisicao.tiporecurso == 0) { //Equipamentos
      if (this.equipamentos.length === 0) {
        this.util.aguardar(true);
        this.apiEqp.listarEquipamentosDisponiveis("null", this.requisicao.cliente, this.session.token).then(res => {
          this.util.aguardar(false);
          this.equipamentos = res.data;
          this.filtrarEquipamentosDisponiveis(); // ✅ CORREÇÃO: Filtrar equipamentos já adicionados
        }).catch(error => {
          this.util.aguardar(false);
          console.error('[REQUISICAO] Erro ao carregar equipamentos:', error);
        });
      } else {
        this.filtrarEquipamentosDisponiveis(); // ✅ CORREÇÃO: Filtrar equipamentos já adicionados
      }
    }
    else {
      if (this.linhas.length === 0) {
        this.util.aguardar(true);
        this.apiTel.listarLinhasDisponiveisParaRequisicao("null", this.requisicao.cliente, this.session.token).then(res => {
          this.util.aguardar(false);
          this.linhas = res.data;
          this.filtrarLinhasDisponiveis(); // ✅ CORREÇÃO: Filtrar linhas já adicionadas
        }).catch(error => {
          this.util.aguardar(false);
          console.error('[REQUISICAO] Erro ao carregar linhas:', error);
        });
      } else {
        this.filtrarLinhasDisponiveis(); // ✅ CORREÇÃO: Filtrar linhas já adicionadas
      }
    }
  }

  // ✅ CORREÇÃO: Método para filtrar equipamentos já adicionados
  filtrarEquipamentosDisponiveis() {
    if (!this.equipamentos || this.equipamentos.length === 0) {
      return;
    }
    
    // Filtrar equipamentos que NÃO estão na lista de itens da requisição
    const equipamentosDisponiveis = this.equipamentos.filter(equip => {
      const jaAdicionado = this.requisicao.requisicoesitens.some(item => 
        item.equipamento === equip.id
      );
      if (jaAdicionado) {
      }
      
      return !jaAdicionado;
    });
    this.equipamentos = equipamentosDisponiveis;
  }

  // ✅ CORREÇÃO: Método para filtrar linhas já adicionadas
  filtrarLinhasDisponiveis() {
    if (!this.linhas || this.linhas.length === 0) return;
    
    // Filtrar linhas que NÃO estão na lista de itens da requisição
    const linhasDisponiveis = this.linhas.filter(linha => {
      return !this.requisicao.requisicoesitens.some(item => 
        item.linhatelefonica === linha.id
      );
    });
    this.linhas = linhasDisponiveis;
  }

  buscarEquipamentos(valor) {
    if (valor && valor !== '' && valor !== null && valor !== undefined) {
      this.util.aguardar(true);
      this.apiEqp.listarEquipamentosDisponiveis(valor, this.requisicao.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.equipamentos = res.data;
          this.filtrarEquipamentosDisponiveis();
        }
      }).catch(error => {
        this.util.aguardar(false);
        console.error('[REQUISICAO] Erro ao buscar equipamentos:', error);
      });
    } else {
    }
  }
  buscarLinhas(valor){
    if (valor && valor !== '' && valor !== null && valor !== undefined) {
      this.util.aguardar(true);
      this.apiTel.listarLinhasDisponiveisParaRequisicao(valor, this.requisicao.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.linhas = res.data;
          this.filtrarLinhasDisponiveis();
        }
      }).catch(error => {
        this.util.aguardar(false);
        console.error('[REQUISICAO] Erro ao buscar linhas:', error);
      });
    } else {
    }
  }

  buscarPorId(){
    this.util.aguardar(true);
    this.api.obterRequisicaoPorId(this.requisicao.id, this.session.token).then(res => {
      this.util.aguardar(false);
      this.requisicao = res.data;
    })
  }

  adicionar(){
    var mensagem = "";
    if(this.requisicao.tiporecurso == 0) {
      if(this.item.equipamento == undefined || this.item.equipamento == null) {
        mensagem += "\n - Selecionar um equipamento;";
      }
    }
    else {
      if(this.item.linhatelefonica == undefined || this.item.linhatelefonica == null) {
        mensagem += "\n - Selecionar uma linha telefônica;";
      }
    }
    // ✅ CORREÇÃO: Validar também se o valor é 0 (inválido)
    if(this.requisicao.tecnicoresponsavel == undefined || this.requisicao.tecnicoresponsavel == null || this.requisicao.tecnicoresponsavel === 0) {
      mensagem += "\n - Selecionar um responsável provisório;"
    }
    
    if(mensagem != "") {
      this.util.exibirMensagemToast("Antes de adicionar, você deve: " + mensagem, 5000);
      return;
    }
    
    try {
      if(this.requisicao.tiporecurso == 0) {
        // ✅ CORREÇÃO: Adicionar equipamento
        const equipamentoSelecionado = this.equipamentos.find(x => x.id === this.item.equipamento);
        if (!equipamentoSelecionado) {
          alert("Equipamento não encontrado!");
          return;
        }
        
        var item: RequisicaoItem = {
          id: 0,
          equipamento: this.item.equipamento,
          linhatelefonica: undefined,
          usuarioentrega: undefined,
          usuariodevolucao: undefined,
          dtentrega: undefined,
          dtdevolucao: undefined,
          observacaoentrega: undefined,
          dtprogramadaretorno: undefined,
          equipamentoNavigation: equipamentoSelecionado // ✅ CORREÇÃO: Adicionar equipamentoNavigation
        };
        
        const equipamentoExistente = this.requisicao.requisicoesitens.find(
          (requisicaoItem) => requisicaoItem.equipamento === item.equipamento
        );
        if(!equipamentoExistente){
          this.requisicao.requisicoesitens.push(item);
        }
        else{
          alert("Equipamento já adicionado.");
          return;
        }
      }
      else {
        // ✅ CORREÇÃO: Adicionar linha telefônica
        const linhatelefonica = this.linhas.find(x => x.id === this.item.linhatelefonica);
        if (!linhatelefonica) {
          alert("Linha telefônica não encontrada!");
          return;
        }
        
        var linha: RequisicaoItem = {
          id: 0,
          equipamento: null, // ✅ CORREÇÃO: Usar null para linhas telefônicas
          linhatelefonica: linhatelefonica.id,
          usuarioentrega: undefined,
          usuariodevolucao: undefined,
          dtentrega: undefined,
          dtdevolucao: undefined,
          observacaoentrega: undefined,
          dtprogramadaretorno: undefined,
          linhaTelefonicaNavigation: linhatelefonica // ✅ CORREÇÃO: Adicionar linhaTelefonicaNavigation
        };
        
        const linhaExistente = this.requisicao.requisicoesitens.find(
          (requisicaoItem) => requisicaoItem.linhatelefonica === linha.linhatelefonica
        );
        if(!linhaExistente){
          this.requisicao.requisicoesitens.push(linha);
        }
        else{
          alert("Linha telefônica já adicionada.");
          return;
        }
      }
      
      // ✅ CORREÇÃO: Atualizar data source e limpar item
      this.dataSource = new MatTableDataSource(this.requisicao.requisicoesitens);
      
      // ✅ CORREÇÃO: Filtrar recursos disponíveis após adicionar
      if (this.requisicao.tiporecurso == 0) {
        this.filtrarEquipamentosDisponiveis();
      } else {
        this.filtrarLinhasDisponiveis();
      }
      
      // ✅ CORREÇÃO: Limpar FormControls sem disparar valueChanges
      this.recursos.setValue('', { emitEvent: false });
      this.frmLinhas.setValue('', { emitEvent: false });
      this.item = {};
    } catch (error) {
      console.error('[REQUISICAO] Erro ao adicionar item:', error);
      alert("Erro ao adicionar item. Tente novamente.");
    }
  }
  excluir(row) {
    // ✅ CORREÇÃO: Salvar referência do item removido para restaurar à lista
    const itemRemovido = row;
    
    this.requisicao.requisicoesitens = this.requisicao.requisicoesitens.filter(x => {
      // ✅ CORREÇÃO: Verificar tanto equipamento quanto linha telefônica
      if (row.equipamento) {
        return x.equipamento !== row.equipamento;
      } else if (row.linhatelefonica) {
        return x.linhatelefonica !== row.linhatelefonica;
      }
      return true;
    });

    this.dataSource = new MatTableDataSource(this.requisicao.requisicoesitens);
    
    // ✅ CORREÇÃO: Restaurar recurso removido à lista de disponíveis
    if (itemRemovido.equipamento) {
      this.restaurarEquipamento(itemRemovido.equipamento);
    } else if (itemRemovido.linhatelefonica) {
      this.restaurarLinha(itemRemovido.linhatelefonica);
    }
  }

  // ✅ CORREÇÃO: Método para restaurar equipamento removido
  restaurarEquipamento(equipamentoId: number) {
    // Buscar o equipamento removido na lista original
    this.apiEqp.listarEquipamentosDisponiveis("null", this.requisicao.cliente, this.session.token).then(res => {
      const equipamentoRemovido = res.data.find(equip => equip.id === equipamentoId);
      if (equipamentoRemovido) {
        this.equipamentos.push(equipamentoRemovido);
      }
    });
  }

  // ✅ CORREÇÃO: Método para restaurar linha removida
  restaurarLinha(linhaId: number) {
    // Buscar a linha removida na lista original
    this.apiTel.listarLinhasDisponiveisParaRequisicao("null", this.requisicao.cliente, this.session.token).then(res => {
      const linhaRemovida = res.data.find(linha => linha.id === linhaId);
      if (linhaRemovida) {
        this.linhas.push(linhaRemovida);
      }
    });
  }

  limparFormulario() {
    this.requisicao = {
      id: 0,
      cliente: this.requisicao.cliente,
      usuariorequisicao: this.requisicao.usuariorequisicao,
      tiporecurso: 0,
      tecnicoresponsavel: null,
      colaboradorfinal: null,
      dtsolicitacao: new Date(),
      dtprocessamento: null,
      requisicaostatus: 1,
      assinaturaeletronica: false,
      dtassinaturaeletronica: null,
      dtenviotermo: null,
      hashrequisicao: null,
      migrateid: null,
      requisicoesitens: []
    };
    
    // Limpar item atual
    this.item = {};
    
    // Limpar formulário
    this.form.reset();
    this.form.patchValue({ 
      tipo: 0, 
      tecnico: null 
    });
    
    // Limpar FormControls
    this.recursos.setValue('', { emitEvent: false });
    this.frmLinhas.setValue('', { emitEvent: false });
    
    // Limpar data source
    this.dataSource = new MatTableDataSource([]);
    
    // Recarregar dados iniciais
    this.carregarRecursos();
  }

  salvar() {
    // ✅ CORREÇÃO: Validar responsável provisório antes de salvar
    if(this.requisicao.tecnicoresponsavel == undefined || this.requisicao.tecnicoresponsavel == null || this.requisicao.tecnicoresponsavel === 0) {
      this.util.exibirMensagemToast("Selecione um responsável provisório antes de salvar a requisição.", 5000);
      return;
    }
    
    if(!this.requisicao.requisicoesitens || this.requisicao.requisicoesitens.length === 0) {
      this.util.exibirMensagemToast("Adicione pelo menos um recurso antes de salvar a requisição.", 5000);
      return;
    }
    
    if(this.form.valid) {
      // ✅ CORREÇÃO CRÍTICA: Garantir que novas requisições sempre tenham ID = 0
      if(this.requisicao.id == undefined || this.requisicao.id == null || this.requisicao.id == 0){
        this.requisicao.id = 0; // Forçar ID = 0 para garantir INSERT
      }
      
      // ✅ CORREÇÃO: Limpar dados antes de enviar para evitar erro 400
      const dadosLimpos = {
        id: this.requisicao.id,
        cliente: this.requisicao.cliente,
        usuariorequisicao: this.requisicao.usuariorequisicao,
        tecnicoresponsavel: this.requisicao.tecnicoresponsavel,
        requisicaostatus: this.requisicao.requisicaostatus,
        colaboradorfinal: this.requisicao.colaboradorfinal || null,
        dtsolicitacao: this.requisicao.dtsolicitacao || new Date(),
        dtprocessamento: this.requisicao.dtprocessamento || null,
        assinaturaeletronica: this.requisicao.assinaturaeletronica || false,
        dtassinaturaeletronica: this.requisicao.dtassinaturaeletronica || null,
        dtenviotermo: this.requisicao.dtenviotermo || null,
        hashrequisicao: this.requisicao.hashrequisicao || null,
        migrateid: this.requisicao.migrateid || null,
        requisicoesitens: this.requisicao.requisicoesitens.map(item => ({
          id: item.id || 0,
          equipamento: item.equipamento,
          linhatelefonica: item.linhatelefonica || null,
          usuarioentrega: item.usuarioentrega || null,
          usuariodevolucao: item.usuariodevolucao || null,
          dtentrega: item.dtentrega || null,
          dtdevolucao: item.dtdevolucao || null,
          observacaoentrega: item.observacaoentrega || null,
          dtprogramadaretorno: item.dtprogramadaretorno || null,
          equipamentoNavigation: item.equipamentoNavigation || null,
          linhaTelefonicaNavigation: item.linhaTelefonicaNavigation || null
        }))
      };
      this.util.aguardar(true);
      this.api.salvarRequisicao(dadosLimpos, this.session.token).then(res => {
        this.util.aguardar(false);
        
        if(res.status == 200) {
          const retorno: any = res.data;
          if(retorno.Status == "200") {
            this.util.exibirMensagemToast("Requisição salva com sucesso!", 3000);
            this.route.navigate(['/movimentacoes/requisicoes']);
          } else {
            this.util.exibirMensagemToast(retorno.Mensagem || "Erro ao salvar requisição", 3000);
          }
        } else {
          this.util.exibirFalhaComunicacao();
        }
      }).catch(err => {
        this.util.aguardar(false);
        console.error('[REQUISICAO] Erro ao salvar:', err);
        this.util.exibirFalhaComunicacao();
      });
    }
  }

  // ✅ NOVO: Método para pré-preencher equipamento selecionado
  private preencherEquipamentoSelecionado(params: any): void {
    try {
      // Criar objeto do equipamento com os dados recebidos
      const equipamentoSelecionado = {
        id: parseInt(params['equipamentoId']),
        numeroserie: params['numeroSerie'],
        tipoequipamento: params['tipoEquipamento'] || '',
        fabricante: params['fabricante'] || '',
        modelo: params['modelo'] || '',
        patrimonio: params['patrimonio'] || ''
      };
      this.requisicao.requisicoesitens.push({
        id: 0,
        equipamento: equipamentoSelecionado.id,
        linhatelefonica: null,
        equipamentoNavigation: equipamentoSelecionado,
        linhaTelefonicaNavigation: null
      });
      
      // Atualizar a tabela de recursos
      this.dataSource = new MatTableDataSource(this.requisicao.requisicoesitens);
      
      // Definir tipo de recurso como equipamento (0)
      this.requisicao.tiporecurso = 0;
      this.form.patchValue({
        tipo: 0
      });
      this.filtrarEquipamentosDisponiveis();
      
      // Exibir mensagem de sucesso
      this.util.exibirMensagemToast(
        `Equipamento ${equipamentoSelecionado.tipoequipamento} ${equipamentoSelecionado.fabricante} ${equipamentoSelecionado.modelo} (S/N: ${equipamentoSelecionado.numeroserie}) adicionado à requisição!`, 
        5000
      );
    } catch (error) {
      console.error('[REQUISICAO] Erro ao pré-preencher equipamento:', error);
      this.util.exibirMensagemToast('Erro ao pré-preencher equipamento. Tente novamente.', 3000);
    }
  }

}
