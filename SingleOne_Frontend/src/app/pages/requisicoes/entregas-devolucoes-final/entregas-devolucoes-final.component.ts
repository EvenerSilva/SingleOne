import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { RequisicaoApiService } from 'src/app/api/requisicoes/requisicao-api.service';
import { UsuarioApiService } from 'src/app/api/usuarios/usuario-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-entregas-devolucoes-final',
  templateUrl: './entregas-devolucoes-final.component.html',
  styleUrls: ['./entregas-devolucoes-final.component.scss']
})
export class EntregasDevolucoesFinalComponent implements OnInit {

  private session:any = {};
  public requisicao:any = {
    requisicoesitens: [],
    requisicaostatus: 1, //Ativa
    colaboradorfinalid: null
  };
  public item:any = [];
  public colunas = ['recurso', 'observacao', 'devprogramada', 'compartilhado', 'coresponsaveis'];
  public mapaOpcoesCoResp: { [itemId: number]: any[] } = {};
  public colaboradores:any = [];
  public form: FormGroup;
  public buscaColaborador = new FormControl();
  public buscaCoResp = new FormControl();
  public resultado: Observable<any>;
  public dataSource: MatTableDataSource<any>;
  private colabNameCache: Map<number, string> = new Map();
  public compareColaborador = (a: any, b: any): boolean => {
    if (a === b) { return true; }
    if (!a || !b) { return false; }
    const aid = typeof a === 'object' ? (a.id ?? a.colaboradorId ?? a.ColaboradorId) : a;
    const bid = typeof b === 'object' ? (b.id ?? b.colaboradorId ?? b.ColaboradorId) : b;
    return aid === bid;
  };

  constructor(private fb: FormBuilder, private util: UtilService, private api: RequisicaoApiService,
    private apiEqp: EquipamentoApiService, private apiCol: ColaboradorApiService, private ar: ActivatedRoute, private route: Router) {
      this.form = this.fb.group({
        recurso: [''],
        tecnico: [''],
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.requisicao.cliente = this.session.usuario.cliente;

    this.resultado = this.buscaColaborador.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();

    this.util.aguardar(true);
    // this.apiCol.listarColaboradoresAtivos("null", this.requisicao.cliente, this.session.token).then(res => {
    //   this.colaboradores = res.data;
      this.util.aguardar(false);

      this.ar.paramMap.subscribe(param => {
        var parametro = param.get('id');
        if(parametro != null) {
          // this.equipamento = JSON.parse(atob(parametro));
          this.requisicao.id = parametro;
          this.buscarPorId();
        }
      })
    // })
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.apiCol.listarColaboradoresAtivos(valor, this.session.usuario.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          const ativos = (res.data || []).filter((c: any) => (c?.ativo === true || c?.Ativo === true || c?.status === 'ativo' || c?.Status === 'Ativo' || typeof c?.ativo === 'undefined'));
          this.colaboradores = ativos;
        }
      })
    }
  }

  buscarPorId(){
    this.util.aguardar(true);
    // this.api.obterEntregaPorId(this.requisicao.id, this.session.token).then(res => {
    //   this.util.aguardar(false);
    //   this.requisicao = res.data;
    //   this.dataSource = new MatTableDataSource(this.requisicao.requisicoesitens);
    // })
    this.api.obterRequisicaoPorId(this.requisicao.id, this.session.token).then(res => {
      this.util.aguardar(false);
      this.requisicao = res.data;
      
      // ✅ CORREÇÃO: Evitar duplicação de recursos
      const todosItens = [];
      
      // ✅ CORREÇÃO: Usar apenas equipamentosRequisicao que já contém todos os equipamentos
      // com informações completas (nome, S/N, etc.)
      if (this.requisicao.equipamentosRequisicao && this.requisicao.equipamentosRequisicao.length > 0) {
        todosItens.push(...this.requisicao.equipamentosRequisicao);
      }
      
      // ✅ CORREÇÃO: Adicionar apenas linhas telefônicas dos requisicaoItens (não equipamentos)
      if (this.requisicao.requisicaoItens && this.requisicao.requisicaoItens.length > 0) {
        const linhasTelefonicas = this.requisicao.requisicaoItens.filter(item => 
          item.linhatelefonica && item.linhatelefonica > 0 && !item.equipamento
        );
        todosItens.push(...linhasTelefonicas);
      }
      
      // Fallback: usar dados da requisição original apenas se não houver equipamentosRequisicao
      if (todosItens.length === 0 && this.requisicao.requisicao && this.requisicao.requisicao.requisicoesitens) {
        todosItens.push(...this.requisicao.requisicao.requisicoesitens);
      }
      this.dataSource = new MatTableDataSource(todosItens);
      // Carregar co-responsáveis ativos por item
      this.carregarCompartilhadosAtivosParaItens();
      // Atualizar colunas conforme capacidade de compartilhamento
      this.atualizarColunas();
    })
  }

  // ✅ NOVO: Método para verificar se há recursos para entrega
  temRecursosParaEntrega(): boolean {
    // Verificar se há equipamentos
    const temEquipamentos = this.requisicao?.equipamentosRequisicao && this.requisicao.equipamentosRequisicao.length > 0;
    
    // Verificar se há itens de requisição (equipamentos + linhas telefônicas)
    const temItensRequisicao = this.requisicao?.requisicaoItens && this.requisicao.requisicaoItens.length > 0;
    
    // Verificar se há dados no dataSource (fallback)
    const temDataSource = this.dataSource && this.dataSource.data && this.dataSource.data.length > 0;
    return temEquipamentos || temItensRequisicao || temDataSource;
  }

  private isLinha(row: any): boolean {
    return !!(row?.linhatelefonica || row?.linhaid);
  }

  private getItemId(row: any): number {
    return row?.id || row?.requisicaoItemId || row?.requisicoesItemId || 0;
  }

  // Detecta BYOD a partir de múltiplas variações de campos
  public isByod(row: any): boolean {
    if (!row) return false;
    if (row.isByod === true || row.byod === true || row.Byod === true) return true;
    const nomes = [row.tipoAquisicaoNome, row.TipoAquisicaoNome, row.tipoaquisicaoNome, row.tipoAquisicaoDescricao, row.tipoaquisicaoDescricao]
      .filter((v: any) => v !== undefined && v !== null)
      .map((v: any) => String(v).toLowerCase());
    if (nomes.some((t: string) => t.includes('particular') || t.includes('byod') || t.includes('pessoal'))) return true;
    const ids = [row.tipoAquisicao, row.tipoaquisicao, row.TipoAquisicao, row.tipoAquisicaoId, row.TipoAquisicaoId]
      .filter((v: any) => v !== undefined && v !== null)
      .map((v: any) => parseInt(String(v), 10));
    if (ids.some((n: number) => n === 2)) return true; // 2 = Particular (BYOD)
    return false;
  }

  private carregarCompartilhadosAtivosParaItens(): void {
    if (!this.dataSource?.data?.length) return;
    const itens = this.dataSource.data;
    itens.forEach((row: any) => {
      const itemId = this.getItemId(row);
      if (!itemId || this.isLinha(row)) return;
      this.api.listarCompartilhadosItem(itemId, this.session.token)
        .then((res: any) => {
          const ativos = (res?.data || []).filter((v: any) => v?.ativo === true);
          row.compartilhadosAtivos = ativos;
          if (!ativos.length) {
            row.compartilhadosAtivosNomes = '';
            // this.cdr.markForCheck(); // cdr is not defined in this context
            return;
          }
          // Resolver nomes com cache
          const promessas = ativos.map((v: any) => {
            if (this.colabNameCache.has(v.colaboradorId)) {
              v.colaboradorNome = this.colabNameCache.get(v.colaboradorId);
              return Promise.resolve(null);
            }
            return this.apiCol.obterColaboradorPorID(v.colaboradorId, this.session.token)
              .then((r: any) => {
                const nome = r?.data?.nome || r?.data?.Nome || '';
                if (nome) {
                  this.colabNameCache.set(v.colaboradorId, nome);
                  v.colaboradorNome = nome;
                }
              })
              .catch(() => null);
          });
          Promise.all(promessas).then(() => {
            row.compartilhadosAtivosNomes = ativos.map((v: any) => v.colaboradorNome || v.colaboradorId).join(', ');
            // this.cdr.markForCheck(); // cdr is not defined in this context
          });
        })
        .catch(() => { /* silencioso */ });
    });
  }

  private podeCompartilhar(row: any): boolean {
    return !this.isLinha(row) && this.isByod(row) !== true;
  }

  private atualizarColunas(): void {
    const dados = this.dataSource?.data || [];
    const existeCompartilhavel = Array.isArray(dados) && dados.some((r: any) => this.podeCompartilhar(r));
    this.colunas = existeCompartilhavel
      ? ['recurso', 'observacao', 'devprogramada', 'compartilhado', 'coresponsaveis']
      : ['recurso', 'observacao', 'devprogramada'];
  }

  buscarCoResponsaveis(itemId: number, termo: string): void {
    if (termo == null) { termo = ''; }
    // Permitir abrir sem termo para manter selecionados visíveis; buscar apenas quando houver termo
    if (termo.length < 2) {
      // Mantém os já selecionados visíveis; não recarrega lista
      return;
    }
    this.apiCol.listarColaboradoresAtivos(termo, this.session.usuario.cliente, this.session.token).then(res => {
      if (res.status === 200) {
        const selecionado: number = this.requisicao.colaboradorfinalid || 0;
        let lista = (res.data || [])
          .filter((c: any) => (c?.ativo === true || c?.Ativo === true || c?.status === 'ativo' || c?.Status === 'Ativo' || typeof c?.ativo === 'undefined'))
          .filter((c: any) => c.id !== selecionado);
        // Garantir que os já selecionados permaneçam na lista de opções
        const rowRef = (this.dataSource?.data || []).find(r => this.getItemId(r) === itemId) || {};
        const selecionados: any[] = Array.isArray(rowRef.coResponsaveisSelecionados) ? rowRef.coResponsaveisSelecionados : [];
        const existentes = new Set(lista.map((c: any) => c.id));
        const manterSelecionados = selecionados.filter((s: any) => !existentes.has(s.id));
        this.mapaOpcoesCoResp[itemId] = [...manterSelecionados, ...lista];
      }
    }).catch(() => {});
  }

  prepararCoResp(itemId: number): void {
    const rowRef = (this.dataSource?.data || []).find(r => this.getItemId(r) === itemId) || {};
    const selecionados: any[] = Array.isArray(rowRef.coResponsaveisSelecionados) ? rowRef.coResponsaveisSelecionados : [];
    const listaAtual: any[] = Array.isArray(this.mapaOpcoesCoResp[itemId]) ? this.mapaOpcoesCoResp[itemId] : [];
    const idsAtuais = new Set(listaAtual.map((c: any) => c.id));
    const toAdd = selecionados.filter((s: any) => !idsAtuais.has(s.id));
    this.mapaOpcoesCoResp[itemId] = [...toAdd, ...listaAtual];
  }

  temDataDevolucao(row: any): boolean {
    if (!row) return false;
    const data = row.dtprogramadaretorno;
    if (data === null || data === undefined || data === '') return false;
    // Se for uma string vazia ou apenas espaços
    if (typeof data === 'string' && data.trim() === '') return false;
    // Se for uma data válida
    if (data instanceof Date) return true;
    // Se for uma string de data válida
    if (typeof data === 'string' && data.length > 0) {
      const date = new Date(data);
      return !isNaN(date.getTime());
    }
    return false;
  }

  limparDataDevolucao(row: any): void {
    row.dtprogramadaretorno = null;
    // Forçar detecção de mudanças se necessário
    if (this.dataSource) {
      this.dataSource.data = [...this.dataSource.data];
    }
  }

  salvar() {
    const colaboradorUnico = this.requisicao.colaboradorfinalid != null ? this.requisicao.colaboradorfinalid : null;
    if (colaboradorUnico == null) {
      this.util.exibirMensagemToast("Você deve informar para qual colaborador a entrega será realizada.", false);
      return;
    }

    const montarItensParaProcessar = (): any[] => {
      const itensParaProcessar: any[] = [];
      if (this.requisicao.equipamentosRequisicao && this.requisicao.equipamentosRequisicao.length > 0) {
        itensParaProcessar.push(...this.requisicao.equipamentosRequisicao);
      }
      if (this.requisicao.requisicaoItens && this.requisicao.requisicaoItens.length > 0) {
        const linhasTelefonicas = this.requisicao.requisicaoItens.filter((item: any) => item.linhatelefonica && item.linhatelefonica > 0 && !item.equipamento);
        itensParaProcessar.push(...linhasTelefonicas);
      }
      if (itensParaProcessar.length === 0 && this.requisicao.requisicao && this.requisicao.requisicao.requisicoesitens) {
        itensParaProcessar.push(...this.requisicao.requisicao.requisicoesitens);
      }
      return itensParaProcessar;
    };

    const itensParaProcessar = montarItensParaProcessar();

    const montarReq = (colabId: number) => {
      const req:any = {
        id: this.requisicao.requisicao.id,
        cliente: this.requisicao.requisicao.cliente,
        usuariorequisicao: this.requisicao.requisicao.usuariorequisicaoid,
        tecnicoresponsavel: this.requisicao.requisicao.tecnicoresponsavelid,
        requisicaostatus: this.requisicao.requisicao.requisicaostatusid,
        colaboradorfinal: colabId,
        dtsolicitacao: this.requisicao.requisicao.dtsolicitacao,
        assinaturaeletronica: false,
        hashrequisicao: this.requisicao.requisicao.hashrequisicao,
        requisicoesitens: [] as any[]
      };

      itensParaProcessar.forEach((x: any) => {
        // Converter data de devolução para formato ISO sem timezone (se for Date)
        let dtprogramadaretorno = x.dtprogramadaretorno;
        if (dtprogramadaretorno instanceof Date) {
          const date = dtprogramadaretorno as Date;
          const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
          dtprogramadaretorno = local.toISOString().replace('Z', '');
        }

        const item:any = {
          id: x.id,
          requisicao: x.requisicao || x.requisicaoId,
          equipamento: x.equipamentoid || x.equipamento,
          linhatelefonica: x.linhaid || x.linhatelefonica,
          usuarioentrega: this.session.usuario.id,
          observacaoentrega: x.observacaoentrega,
          dtprogramadaretorno: dtprogramadaretorno
        };

        // Anexar co-responsáveis apenas para itens não-linha quando marcado como compartilhado
        if (!this.isLinha(x) && this.isByod(x) !== true && x.compartilhado === true && Array.isArray(x.coResponsaveisSelecionados) && x.coResponsaveisSelecionados.length > 0) {
          item.coResponsaveis = x.coResponsaveisSelecionados.map((col: any) => ({
            colaboradorId: col.id,
            tipoAcesso: 'usuario_compartilhado',
            dataFim: null,
            observacao: x.observacaoCompartilhamento || x.observacaoentrega || null
          }));
        }

        req.requisicoesitens.push(item);
      });

      return req;
    };

    this.util.aguardar(true);
    this.api.realizarEntregaComCompartilhados(montarReq(colaboradorUnico), this.session.token)
      .then((res: any) => {
        this.util.aguardar(false);
        if (res?.status != 200 && res?.status != "200") {
          this.util.exibirFalhaComunicacao();
          return;
        }
        this.util.exibirMensagemToast("Entrega realizada com sucesso!", 5000);
        this.route.navigate(['/movimentacoes/requisicoes']);
      })
      .catch(() => {
        this.util.aguardar(false);
        this.util.exibirFalhaComunicacao();
      });
  }

}
