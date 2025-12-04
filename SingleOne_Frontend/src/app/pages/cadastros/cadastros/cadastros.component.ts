import { Component, OnInit } from '@angular/core';
import { UtilService } from 'src/app/util/util.service';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-cadastros',
  templateUrl: './cadastros.component.html',
  styleUrls: ['./cadastros.component.scss']
})
export class CadastrosComponent implements OnInit {

  public session: any = {};
  public recursosResumo: any = null;
  public telefoniaResumo: any = null;

  constructor(
    private util: UtilService,
    private equipamentoApi: EquipamentoApiService,
    private telefoniaApi: TelefoniaApiService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.carregarResumoRecursos();
    this.carregarResumoTelefonia();
  }

  // 📊 CARREGAR RESUMO DOS RECURSOS
  async carregarResumoRecursos() {
    try {
      this.util.aguardar(true);
      const response = await this.equipamentoApi.listarTodosEquipamentosParaResumo(this.session.usuario.cliente);
      if (response && response.status === 200 && response.data) {
        const equipamentos = response.data;
        const normalizar = (v: any) => (v ?? '').toString().toLowerCase().trim();
        const obterStatusNome = (e: any) => normalizar(
          e?.equipamentostatus ?? e?.Equipamentostatus ?? e?.EquipamentoStatus ?? e?.status ?? e?.Status ?? e?.situacao ?? e?.Situacao
        );
        const obterStatusId = (e: any) => {
          const idRaw = e?.equipamentostatusid ?? e?.EquipamentoStatusId ?? e?.statusId ?? e?.StatusId ?? e?.situacaoId ?? e?.SituacaoId;
          const idNum = parseInt(idRaw, 10);
          return isNaN(idNum) ? undefined : idNum;
        };
        const isStatus = (e: any, nomes: string[], ids?: number[]) => {
          const nome = obterStatusNome(e);
          const id = obterStatusId(e);
          const nomeMatch = nomes.some(n => nome === n);
          const idMatch = Array.isArray(ids) && ids.length > 0 ? ids.includes(id as number) : false;
          return nomeMatch || idMatch;
        };

        const total = equipamentos.length;
        const emEstoque = equipamentos.filter(e => isStatus(e, ['em estoque', 'estoque'], [3])).length;
        const entregue = equipamentos.filter(e => isStatus(e, ['entregue'], [4])).length;
        const novo = equipamentos.filter(e => isStatus(e, ['novo', 'em lançamento', 'em lancamento'], [6])).length;
        const emTransito = equipamentos.filter(e => isStatus(e, ['requisitado', 'devolvido'], [2, 5])).length;

        this.recursosResumo = { total, emEstoque, entregue, novo, emTransito };
      } else {
        console.warn('[CADASTROS] Resposta inválida ou sem dados:', response);
        // Inicializar com valores padrão
        this.recursosResumo = { total: 0, emEstoque: 0, entregue: 0, novo: 0, emTransito: 0 };
      }
    } catch (error) {
      console.error('[CADASTROS] Erro ao carregar resumo de recursos:', error);
      // Inicializar com valores padrão em caso de erro
      this.recursosResumo = { total: 0, emEstoque: 0, entregue: 0, novo: 0, emTransito: 0 };
    } finally {
      this.util.aguardar(false);
    }
  }

  // 📞 CARREGAR RESUMO DA TELEFONIA
  async carregarResumoTelefonia() {
    try {
      this.util.aguardar(true);

      const [operadorasRes, contratosRes, planosRes, linhasRes] = await Promise.all([
        this.telefoniaApi.contarOperadoras(this.session.token),
        this.telefoniaApi.contarContratos(this.session.token),
        this.telefoniaApi.contarPlanos(this.session.token),
        this.telefoniaApi.contarLinhas(this.session.token)
      ]);

      this.telefoniaResumo = {
        operadoras: operadorasRes?.data || 0,
        contratos: contratosRes?.data || 0,
        planos: planosRes?.data || 0,
        linhas: linhasRes?.data || 0
      };
    } catch (error) {
      console.error('Erro ao carregar resumo de telefonia:', error);
      this.telefoniaResumo = {
        operadoras: 0,
        contratos: 0,
        planos: 0,
        linhas: 0
      };
    } finally {
      this.util.aguardar(false);
    }
  }

  // 🆕 NOVO RECURSO
  novoRecurso() {
    this.router.navigate(['/recurso']);
  }

  // 📥 EXPORTAR RECURSOS
  async exportarRecursos() {
    try {
      this.util.aguardar(true);
      // Aqui você pode implementar a lógica de exportação
      // Por enquanto, vou redirecionar para a tela de recursos
      this.router.navigate(['/recursos']);
    } catch (error) {
      console.error('Erro ao exportar recursos:', error);
    } finally {
      this.util.aguardar(false);
    }
  }

  // 🚀 NAVEGAÇÃO PARA CARDS
  navegarParaClientes() {
    this.router.navigate(['/clientes']);
  }

  navegarParaEmpresaWizard() {
    this.router.navigate(['/empresa-wizard']);
  }

  navegarParaEmpresas() {
    this.router.navigate(['/empresas']);
  }

  navegarParaFiliais() {
    this.router.navigate(['/filiais']);
  }

  navegarParaLocalidades() {
    this.router.navigate(['/localidades']);
  }

  navegarParaCentrosCustos() {
    this.router.navigate(['/centros-custos']);
  }

  navegarParaRecursos() {
    this.router.navigate(['/recursos']);
  }

  navegarParaTelecom() {
    this.router.navigate(['/telecom']);
  }

  navegarParaEquipamentosWizard() {
    this.router.navigate(['/equipamentos-wizard']);
  }

  navegarParaTiposRecursos() {
    this.router.navigate(['/tipos-recursos']);
  }

  navegarParaFabricantes() {
    this.router.navigate(['/fabricantes']);
  }

  navegarParaModelos() {
    this.router.navigate(['/modelos']);
  }

  navegarParaCategorias() {
    this.router.navigate(['/categorias']);
  }

  navegarParaFornecedores() {
    this.router.navigate(['/fornecedores']);
  }

  navegarParaContratos() {
    this.router.navigate(['/contratos']);
  }

  navegarParaNotasFiscais() {
    this.router.navigate(['/notas-fiscais']);
  }

  // 📞 GERENCIAR OPERADORAS
  gerenciarOperadoras() {
    this.router.navigate(['/operadoras']);
  }

  // 📞 GERENCIAR PLANOS
  gerenciarPlanos() {
    this.router.navigate(['/planos']);
  }

  // 📞 GERENCIAR LINHAS
  gerenciarLinhas() {
    this.router.navigate(['/linhas']);
  }

  // 📞 GERENCIAR CONTRATOS DE TELEFONIA
  gerenciarContratosTelefonia() {
    this.router.navigate(['/contratos-telefonia']);
  }

  // 📦 NAVEGAÇÃO PARA ESTOQUE MÍNIMO
  navegarParaEstoqueMinimo() {
    this.router.navigate(['/estoque-minimo']);
  }

}
