import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { RequisicaoApiService } from 'src/app/api/requisicoes/requisicao-api.service';
import { UtilService } from 'src/app/util/util.service';
import { FormControl } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'app-modal-compartilhar-item',
  templateUrl: './modal-compartilhar-item.component.html',
  styleUrls: ['./modal-compartilhar-item.component.scss']
})
export class ModalCompartilharItemComponent {
  session: any = {};
  itemId: number;
  equipamento: any = {};
  compartilhados: any[] = [];
  opcoes: any[] = [];
  selecionados: any[] = [];
  observacaoNova: string = '';
  private readonly MAX_RESULTADOS: number = 20;
  private paginaSugestoes: number = 1;
  // ✅ CORREÇÃO: Flag para rastrear se houve alterações (adicionar ou encerrar)
  private houveAlteracoes: boolean = false;

  // Busca com autocomplete
  searchCtrl = new FormControl('');

  constructor(
    public dialogRef: MatDialogRef<ModalCompartilharItemComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private util: UtilService,
    private api: RequisicaoApiService,
    private apiCol: ColaboradorApiService
  ) {
    this.session = this.util.getSession('usuario');
    this.equipamento = { ...(data?.eqp || {}), colaboradorContextoId: data?.colaboradorContextoId, colaboradorContextoNome: data?.colaboradorContextoNome };
    this.itemId = data?.itemId || this.getReqItemId(this.equipamento);

    // Debounce da busca
    this.searchCtrl.valueChanges.pipe(debounceTime(300)).subscribe((term: any) => {
      this.buscar(typeof term === 'string' ? term : '');
    });

    this.carregar();
  }

  private getReqItemId(eqp: any): number {
    return (
      eqp?.requisicaoItemId ||
      eqp?.requisicoesItemId ||
      eqp?.requisicaoItem ||
      eqp?.requisicoesitemid ||
      eqp?.requisicoesItemID ||
      eqp?.id || 0
    );
  }

  fechar(): void {
    // ✅ CORREÇÃO: Retornar flag indicando se houve alterações
    this.dialogRef.close(this.houveAlteracoes);
  }

  carregar(): void {
    if (!this.itemId) return;
    this.util.aguardar(true);
    this.api.listarCompartilhadosItem(this.itemId, this.session.token)
      .then((res: any) => {
        this.util.aguardar(false);
        this.compartilhados = res?.data || [];
        // Preencher nomes quando vier só o ID
        this.preencherNomes();
        // Atualizar sugestões considerando os já vinculados
        this.buscar(this.searchCtrl.value || '');
      })
      .catch(() => this.util.aguardar(false));
  }

  private preencherNomes(): void {
    const pendentes = (this.compartilhados || []).filter((v: any) => !v.colaboradorNome && v.colaboradorId);
    if (!pendentes.length) return;
    Promise.all(pendentes.map((v: any) => this.apiCol.obterColaboradorPorID(v.colaboradorId, this.session.token)))
      .then((resps: any[]) => {
        resps.forEach((r: any, idx: number) => {
          const alvo = pendentes[idx];
          const dados = r?.data || r;
          const nome = dados?.nome || dados?.Nome || '';
          if (nome) alvo.colaboradorNome = nome;
        });
      })
      .catch(() => { /* silencioso */ });
  }

  displayCol(c: any): string {
    if (!c) return '';
    return c.nome ? `${c.nome} - ${c.matricula || ''}`.trim() : String(c);
  }

  buscar(term: string): void {
    // ✅ CORREÇÃO: Filtrar apenas compartilhamentos ATIVOS para permitir re-compartilhar após encerramento
    const existentes = new Set(this.compartilhados.filter(x => x.ativo).map(x => x.colaboradorId));
    const jaSelecionados = new Set((this.selecionados || []).map(x => x.id));
    const idsNaoPermitidos = this.getIdsNaoPermitidos();
    const nomesNaoPermitidos = this.getNomesNaoPermitidos();

    // Caso não haja termo (abertura do modal), usar endpoint paginado geral (página 1) e filtrar ativos
    if (!term || term.length < 2) {
      this.paginaSugestoes = 1;
      // Usar apenas endpoint de ATIVOS para evitar listar desligados
      this.apiCol.listarColaboradoresAtivos('null', this.session.usuario.cliente, this.session.token)
        .then((res: any) => {
          const listaBruta = (res?.data || []) as any[];
          const filtrada = listaBruta
            .filter((c: any) => {
              const nome = (c?.nome || c?.Nome || '').toString().trim().toLowerCase();
              return !existentes.has(c.id)
                && !jaSelecionados.has(c.id)
                && !idsNaoPermitidos.has(c.id)
                && (nome ? !nomesNaoPermitidos.has(nome) : true);
            })
            .slice(0, this.MAX_RESULTADOS);
          this.opcoes = filtrada;
        });
      return;
    }

    // Com termo (>=2), usar endpoint de ativos e limitar resultados
    this.apiCol.listarColaboradoresAtivos(term, this.session.usuario.cliente, this.session.token)
      .then((res: any) => {
        const listaBruta = (res?.data || []) as any[];
        const filtrada = listaBruta
          .filter((c: any) => {
            const nome = (c?.nome || c?.Nome || '').toString().trim().toLowerCase();
            return !existentes.has(c.id)
              && !jaSelecionados.has(c.id)
              && !idsNaoPermitidos.has(c.id)
              && (nome ? !nomesNaoPermitidos.has(nome) : true);
          })
          .slice(0, this.MAX_RESULTADOS);
        this.opcoes = filtrada;
      });
  }

  private isAtivo(c: any): boolean {
    if (!c) return false;
    // Valores diretos booleanos
    if (typeof c.ativo === 'boolean') return c.ativo === true;
    if (typeof c.Ativo === 'boolean') return c.Ativo === true;
    // Numéricos comuns
    const numericos = [c.ativo, c.Ativo];
    if (numericos.some(v => v === 1 || v === '1')) return true;
    // Textos comuns (normalizados)
    const textos = [c.status, c.Status, c.situacao, c.Situacao, c.situacaoFuncionario, c.SituacaoFuncionario]
      .filter(v => v !== undefined && v !== null)
      .map(v => String(v).trim().toLowerCase());
    if (textos.some(t => ['ativo','a','s','em atividade','act','active'].includes(t))) return true;
    if (textos.some(t => ['inativo','desligado','demitido','inactive','inact','inatividade','deslig'].includes(t))) return false;
    // Sem sinalização explícita, tratar como inativo para não vazar desligados
    return false;
  }

  private getIdsNaoPermitidos(): Set<number> {
    const ids = new Set<number>();
    const add = (v: any) => {
      const n = this.toNum(v);
      if (n) ids.add(n);
    };
    const e: any = this.equipamento || {};
    // IDs diretos do equipamento
    add(e.colaboradorId); add(e.ColaboradorId); add(e.colaboradorid);
    add(e.colaborador?.id); add(e.colaborador?.Id);
    add(e.colaboradorFinalId); add(e.colaboradorfinalid); add(e.colaboradorFinal?.id);
    add(e.responsavelId); add(e.ResponsavelId);
    add(e.responsavelProvisorioId); add(e.ResponsavelProvisorioId);
    add(e.tecnicoresponsavelid); add(e.tecnicoResponsavelId); add(e.TecnicoResponsavelId);
    add(e.destinatarioId); add(e.DestinatarioId); add(e.destinatario?.id);
    // IDs vindos do contexto do colaborador (linha da lista)
    add(e.colaboradorContextoId);
    // IDs vindos de objetos relacionados
    const r: any = e.requisicao || e.Requisicao || {};
    add(r.colaboradorfinalid); add(r.colaboradorFinalId); add(r.colaboradorFinal?.id);
    add(r.colaborador?.id); add(r.ColaboradorId);
    add(r.tecnicoresponsavelid); add(r.tecnicoResponsavelId); add(r.TecnicoResponsavelId);
    // Fallback: se houver outro objeto aninhado "requisicao"
    const rr: any = r.requisicao || r.Requisicao || {};
    add(rr.colaboradorfinalid); add(rr.colaboradorFinalId); add(rr.colaboradorFinal?.id);
    add(rr.colaborador?.id); add(rr.ColaboradorId);
    return ids;
  }

  private toNum(v: any): number | null {
    if (v === null || v === undefined) return null;
    const n = parseInt(String(v), 10);
    return isNaN(n) ? null : n;
  }

  private getNomesNaoPermitidos(): Set<string> {
    const nomes = new Set<string>();
    const add = (v: any) => {
      if (!v) return;
      const s = String(v).trim().toLowerCase();
      if (s) nomes.add(s);
    };
    const e: any = this.equipamento || {};
    // Nomes diretos no item
    add(e.colaboradorNome); add(e.ColaboradorNome); add(e.colaborador);
    add(e.responsavelNome); add(e.ResponsavelNome); add(e.responsavelProvisorio);
    add(e.tecnicoResponsavel); add(e.TecnicoResponsavel);
    add(e.usuarioEntregaNome); add(e.UsuarioEntregaNome); add(e.usuarioentregaNome); add(e.UsuarioentregaNome);
    // Nome do contexto (linha do colaborador no grid)
    add(e.colaboradorContextoNome);
    // Nomes vindos da requisição relacionada
    const r: any = e.requisicao || e.Requisicao || {};
    add(r.colaborador); add(r.Colaborador);
    add(r.colaboradorfinal); add(r.ColaboradorFinal);
    add(r.tecnicoResponsavel); add(r.TecnicoResponsavel);
    // Fallback aninhado
    const rr: any = r.requisicao || r.Requisicao || {};
    add(rr.colaborador); add(rr.colaboradorfinal);
    return nomes;
  }

  selecionar(c: any): void {
    if (!c) return;
    if (!this.isAtivo(c)) {
      this.util.exibirMensagemToast('Colaborador desligado não pode ser co-responsável.', 4000);
      return;
    }
    const exists = (this.selecionados || []).some(x => x.id === c.id);
    if (!exists) this.selecionados = [...(this.selecionados || []), c];
    this.searchCtrl.setValue('');
    this.buscar('');
  }

  remover(c: any): void {
    this.selecionados = (this.selecionados || []).filter(x => x.id !== c.id);
    this.buscar(this.searchCtrl.value || '');
  }

  limparSelecao(): void {
    this.selecionados = [];
    this.buscar(this.searchCtrl.value || '');
  }

  adicionar(): void {
    if (!this.selecionados?.length || !this.itemId) return;
    const validos = (this.selecionados || []).filter((c: any) => this.isAtivo(c));
    if (validos.length === 0) {
      this.util.exibirMensagemToast('Selecione ao menos um co-responsável ativo.', 4000);
      return;
    }
    this.util.aguardar(true);
    Promise.all(validos.map((c: any) => this.api.adicionarCompartilhadoItem(this.itemId, {
      colaboradorId: c.id,
      tipoAcesso: 'usuario_compartilhado',
      observacao: this.observacaoNova || ''
    }, this.session.token)))
    .then(() => {
      this.util.aguardar(false);
      this.util.exibirMensagemToast('Co-responsável(is) adicionado(s) com sucesso!', 4000);
      this.selecionados = [];
      this.observacaoNova = '';
      // ✅ CORREÇÃO: Marcar que houve alterações e fechar
      this.houveAlteracoes = true;
      this.dialogRef.close(true);
    })
    .catch(() => {
      this.util.aguardar(false);
      this.util.exibirMensagemToast('Falha ao adicionar co-responsável(is).', 4000);
    });
  }

  encerrar(v: any): void {
    if (!v?.id) return;
    this.util.aguardar(true);
    this.api.encerrarCompartilhadoItem(v.id, this.session.token)
      .then(() => {
        this.util.aguardar(false);
        this.util.exibirMensagemToast('Compartilhamento encerrado com sucesso!', 4000);
        // ✅ CORREÇÃO: Marcar que houve alterações
        this.houveAlteracoes = true;
        // Recarregar a lista dentro do modal
        this.carregar();
      })
      .catch(() => {
        this.util.aguardar(false);
        this.util.exibirMensagemToast('Falha ao encerrar compartilhamento.', 4000);
      });
  }

  temCompartilhadosEncerrados(): boolean {
    return (this.compartilhados || []).some((v: any) => !v.ativo);
  }

  // ✅ MÉTODOS PARA LINHA TELEFÔNICA
  isLinhaItem(): boolean {
    const nome = (this.equipamento?.equipamento || this.equipamento?.nome || '').toString().toLowerCase();
    return nome.includes('linha');
  }

  getNomeLinha(): string {
    const nome = this.equipamento?.equipamento || this.equipamento?.nome || 'Linha Telefônica';
    // Se já tem o número no nome, retornar como está
    if (nome.match(/\d{10,11}/)) {
      return nome;
    }
    // Senão, tentar adicionar o número do patrimônio
    const numero = this.equipamento?.patrimonio || this.equipamento?.numero;
    if (numero) {
      return `Linha telefônica ${numero}`;
    }
    return nome;
  }

  getOperadoraPlano(): string {
    // Verificar múltiplas variações de campos
    const operadora = (
      this.equipamento?.operadora ||
      this.equipamento?.Operadora ||
      this.equipamento?.operadoraNome ||
      this.equipamento?.OperadoraNome ||
      ''
    ).toString().trim();

    const plano = (
      this.equipamento?.plano ||
      this.equipamento?.Plano ||
      this.equipamento?.planoNome ||
      this.equipamento?.PlanoNome ||
      this.equipamento?.tipoPlano ||
      this.equipamento?.TipoPlano ||
      ''
    ).toString().trim();

    // Tentar buscar em fabricante/modelo como fallback (padrão usado em outros lugares)
    const fabricante = (this.equipamento?.fabricante || '').toString().trim();
    const modelo = (this.equipamento?.modelo || '').toString().trim();

    // Se tem operadora e plano explícitos
    if (operadora && plano) {
      return `${operadora} - ${plano}`;
    }

    // Se tem só operadora
    if (operadora) {
      return operadora;
    }

    // Se tem só plano
    if (plano) {
      return plano;
    }

    // Fallback: usar fabricante e modelo (padrão usado na lista)
    if (fabricante && modelo) {
      return `${fabricante} - ${modelo}`;
    }

    if (fabricante) {
      return fabricante;
    }

    if (modelo) {
      return modelo;
    }

    return '';
  }
}
