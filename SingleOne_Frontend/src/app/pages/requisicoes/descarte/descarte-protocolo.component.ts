import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ProtocoloDescarteApiService } from 'src/app/api/protocolo-descarte/protocolo-descarte-api.service';
import { UtilService } from 'src/app/util/util.service';
import { ModalProtocoloComponent } from './modal-protocolo/modal-protocolo.component';
import { ModalEvidenciasComponent } from './modal-evidencias/modal-evidencias.component';
import { 
  ProtocoloDescarte, 
  ProtocoloDescarteItem, 
  EquipamentoDisponivel,
  EstatisticasProtocolo,
  StatusProtocoloEnum,
  TipoDescarteEnum,
  MetodoSanitizacaoEnum,
  METODOS_SANITIZACAO_LABELS
} from 'src/app/models/protocolo-descarte.model';

@Component({
  selector: 'app-descarte-protocolo',
  templateUrl: './descarte-protocolo.component.html',
  styleUrls: ['./descarte-protocolo.component.scss']
})
export class DescarteProtocoloComponent implements OnInit {

  public session: any = {};
  public metodosSanitizacao = Object.values(MetodoSanitizacaoEnum);
  public metodosSanitizacaoLabels = METODOS_SANITIZACAO_LABELS;
  
  // Sistema de Protocolo
  public protocolos: ProtocoloDescarte[] = [];
  public protocoloAtual: ProtocoloDescarte | null = null;
  public equipamentosDisponiveis: EquipamentoDisponivel[] = [];
  public estatisticas: EstatisticasProtocolo | null = null;

  constructor(
    private util: UtilService,
    private protocoloApi: ProtocoloDescarteApiService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.carregarProtocolos();
  }

  // ==================== SISTEMA DE PROTOCOLO ====================

  carregarProtocolos(): void {
    if (!this.session?.usuario?.cliente) return;
    
    this.util.aguardar(true);
    this.protocoloApi.listarProtocolos(this.session.usuario.cliente).subscribe({
      next: (protocolos) => {
        this.protocolos = protocolos;
        this.util.aguardar(false);
      },
      error: (error) => {
        this.util.aguardar(false);
        console.error('Erro ao carregar protocolos:', error);
        this.util.exibirMensagemToast('Erro ao carregar protocolos', 5000);
      }
    });
  }

  criarNovoProtocolo(): void {
    if (!this.session?.usuario?.cliente) return;

    const dialogRef = this.dialog.open(ModalProtocoloComponent, {
      width: '700px',
      maxWidth: '90vw',
      data: { clienteId: this.session.usuario.cliente },
      disableClose: false,
      panelClass: 'custom-modal-panel'
    });

    dialogRef.afterClosed().subscribe(protocolo => {
      if (protocolo) {
        this.carregarProtocolos();
        this.selecionarProtocolo(protocolo);
      }
    });
  }

  selecionarProtocolo(protocolo: ProtocoloDescarte): void {
    if (!protocolo.id) return;
    
    // Carregar detalhes completos do protocolo (com itens)
    this.protocoloApi.obterProtocolo(protocolo.id).subscribe({
      next: (protocoloCompleto) => {
        this.protocoloAtual = protocoloCompleto;
        if (protocoloCompleto.itens && protocoloCompleto.itens.length > 0) {
          protocoloCompleto.itens.forEach((item, index) => {
          });
        }
        
        this.carregarEstatisticas(protocolo.id!);
        this.carregarEquipamentosDisponiveis();
      },
      error: (error) => {
        console.error('Erro ao carregar protocolo:', error);
        this.util.exibirMensagemToast('Erro ao carregar detalhes do protocolo', 5000);
      }
    });
  }

  carregarEstatisticas(protocoloId: number): void {
    if (!this.session?.token) return;

    this.protocoloApi.obterEstatisticas(protocoloId, this.session.token).subscribe({
      next: (stats) => {
        this.estatisticas = stats;
      },
      error: (error) => {
        console.error('Erro ao carregar estatísticas:', error);
      }
    });
  }

  carregarEquipamentosDisponiveis(): void {
    if (!this.session?.usuario?.cliente || !this.session?.token) return;

    this.protocoloApi.listarEquipamentosDisponiveis(
      this.session.usuario.cliente, 
      '', 
      this.session.token
    ).subscribe({
      next: (equipamentos) => {
        this.equipamentosDisponiveis = equipamentos;
      },
      error: (error) => {
        console.error('Erro ao carregar equipamentos disponíveis:', error);
      }
    });
  }

  adicionarEquipamentoAoProtocolo(equipamento: EquipamentoDisponivel): void {
    if (!this.protocoloAtual?.id || !this.session?.token) return;

    this.util.aguardar(true);
    this.protocoloApi.adicionarEquipamento(
      this.protocoloAtual.id, 
      equipamento.id, 
      this.session.token
    ).subscribe({
      next: (item) => {
        this.util.aguardar(false);
        this.util.exibirMensagemToast('Equipamento adicionado ao protocolo', 3000);
        // Recarregar protocolo completo para atualizar a lista de itens
        this.recarregarProtocoloAtual();
      },
      error: (error) => {
        this.util.aguardar(false);
        console.error('Erro ao adicionar equipamento:', error);
        let mensagemErro = 'Erro ao adicionar equipamento';
        if (error.error?.mensagem) {
          mensagemErro = error.error.mensagem;
        }
        this.util.exibirMensagemToast(mensagemErro, 5000);
      }
    });
  }

  removerEquipamentoDoProtocolo(item: ProtocoloDescarteItem | EquipamentoDisponivel): void {
    if (!this.protocoloAtual?.id || !this.session?.token) return;

    // Se recebeu um ProtocoloDescarteItem, pegar o ID do equipamento
    const equipamentoId = 'equipamento' in item ? item.equipamento : item.id;

    this.util.exibirMensagemPopUp(
      'Tem certeza que deseja remover este equipamento do protocolo?', 
      true
    ).then(confirmado => {
      if (confirmado) {
        this.util.aguardar(true);
        this.protocoloApi.removerEquipamento(
          this.protocoloAtual!.id!, 
          equipamentoId, 
          this.session.token
        ).subscribe({
          next: () => {
            this.util.aguardar(false);
            this.util.exibirMensagemToast('Equipamento removido do protocolo', 3000);
            // Recarregar protocolo completo para atualizar a lista de itens
            this.recarregarProtocoloAtual();
          },
          error: (error) => {
            this.util.aguardar(false);
            console.error('Erro ao remover equipamento:', error);
            let mensagemErro = 'Erro ao remover equipamento';
            if (error.error?.mensagem) {
              mensagemErro = error.error.mensagem;
            }
            this.util.exibirMensagemToast(mensagemErro, 5000);
          }
        });
      }
    });
  }

  atualizarProcesso(item: ProtocoloDescarteItem, processo: string, valor: boolean): void {
    if (!item.id || !this.session?.token) return;
    const metodoTemp = item.metodoSanitizacao;
    const ferramentaTemp = item.ferramentaUtilizada;
    const observacoesTemp = item.observacoesSanitizacao;
    
    this.protocoloApi.atualizarProcessoItem(item.id, processo, valor, this.session.token).subscribe({
      next: () => {
        this.util.exibirMensagemToast(`Processo ${processo} atualizado`, 2000);
        
        // Recarregar protocolo mantendo valores temporários
        this.recarregarProtocoloAtual();
        
        // Aguardar um tick do Angular e restaurar valores temporários se necessário
        setTimeout(() => {
          if (this.protocoloAtual?.itens) {
            const itemAtualizado = this.protocoloAtual.itens.find(i => i.id === item.id);
            if (itemAtualizado) {
              if (!itemAtualizado.metodoSanitizacao && metodoTemp) {
                itemAtualizado.metodoSanitizacao = metodoTemp;
              }
              if (!itemAtualizado.ferramentaUtilizada && ferramentaTemp) {
                itemAtualizado.ferramentaUtilizada = ferramentaTemp;
              }
              if (!itemAtualizado.observacoesSanitizacao && observacoesTemp) {
                itemAtualizado.observacoesSanitizacao = observacoesTemp;
              }
            }
          }
        }, 100);
      },
      error: (error) => {
        console.error(`❌ Erro ao atualizar processo ${processo}:`, error);
        // Reverter o valor no frontend
        switch (processo) {
          case 'sanitizacao':
            item.processoSanitizacao = !valor;
            break;
          case 'descaracterizacao':
            item.processoDescaracterizacao = !valor;
            break;
          case 'perfuracao':
            item.processoPerfuracaoDisco = !valor;
            break;
        }
        this.util.exibirMensagemToast('Erro ao atualizar processo', 5000);
      }
    });
  }

  abrirModalEvidencias(item: ProtocoloDescarteItem): void {
    if (!item.equipamentoNavigation) {
      this.util.exibirMensagemToast('Dados do equipamento não disponíveis', 3000);
      return;
    }

    const dialogRef = this.dialog.open(ModalEvidenciasComponent, {
      width: '900px',
      maxHeight: '90vh',
      data: {
        equipamento: {
          id: item.equipamento,
          tipoequipamento: 'Equipamento',
          fabricante: item.equipamentoNavigation.Fabricante || '',
          numeroserie: item.equipamentoNavigation.Numeroserie || '',
          patrimonio: item.equipamentoNavigation.Patrimonio || ''
        },
        protocoloId: item.protocoloId,
        itemId: item.id
      },
      panelClass: 'custom-modal-panel'
    });

    dialogRef.afterClosed().subscribe((totalEvidencias) => {
      if (totalEvidencias !== undefined) {
        if (item.id) {
          item.quantidadeEvidencias = totalEvidencias;
          
          // Se tinha evidências obrigatórias e agora tem pelo menos 1, marcar como executado
          if (item.evidenciasObrigatorias && totalEvidencias > 0) {
            this.atualizarEvidenciasExecutadas(item);
          }
        }
        
        // Recarregar protocolo para atualizar estatísticas
        this.recarregarProtocoloAtual();
      }
    });
  }

  atualizarEvidenciasExecutadas(item: ProtocoloDescarteItem): void {
    if (!item.id || !this.session?.token) return;
    this.protocoloApi.atualizarProcessoItem(item.id, 'evidencias', true, this.session.token).subscribe({
      next: () => {
        item.evidenciasExecutadas = true;
        this.recarregarProtocoloAtual();
      },
      error: (error) => {
        console.error(`❌ Erro ao atualizar evidências:`, error);
      }
    });
  }

  recarregarProtocoloAtual(): void {
    if (!this.protocoloAtual?.id || !this.session?.token) return;

    this.protocoloApi.obterProtocolo(this.protocoloAtual.id).subscribe({
      next: (protocolo) => {
        this.protocoloAtual = protocolo;
        this.carregarEstatisticas(protocolo.id!);
        this.carregarEquipamentosDisponiveis();
      },
      error: (error) => {
        console.error('Erro ao recarregar protocolo:', error);
      }
    });
  }

  atualizarMetodoSanitizacao(item: ProtocoloDescarteItem): void {
    if (!item.id || !this.session?.token) return;
    this.atualizarCampoItem(item.id, 'metodoSanitizacao', item.metodoSanitizacao);
  }

  atualizarFerramentaUtilizada(item: ProtocoloDescarteItem): void {
    if (!item.id || !this.session?.token) return;
    this.atualizarCampoItem(item.id, 'ferramentaUtilizada', item.ferramentaUtilizada);
  }

  atualizarObservacoesSanitizacao(item: ProtocoloDescarteItem): void {
    if (!item.id || !this.session?.token) return;
    this.atualizarCampoItem(item.id, 'observacoesSanitizacao', item.observacoesSanitizacao);
  }

  private atualizarCampoItem(itemId: number, campo: string, valor: any): void {
    if (!this.session?.token) return;

    this.protocoloApi.atualizarCampoItem(itemId, campo, valor, this.session.token).subscribe({
      next: () => {
      },
      error: (error) => {
        console.error(`❌ Erro ao atualizar ${campo}:`, error);
        this.util.exibirMensagemToast(`Erro ao atualizar ${campo}`, 3000);
      }
    });
  }

  finalizarProtocolo(): void {
    if (!this.protocoloAtual?.id || !this.session?.token) return;

    this.util.exibirMensagemPopUp(
      'Tem certeza que deseja finalizar este protocolo? Esta ação não poderá ser desfeita.',
      true
    ).then(confirmado => {
      if (confirmado) {
        this.util.aguardar(true);
        this.protocoloApi.finalizarProtocolo(this.protocoloAtual!.id!, this.session.token).subscribe({
          next: (protocolo) => {
            this.util.aguardar(false);
            this.protocoloAtual = protocolo;
            this.carregarProtocolos();
            this.util.exibirMensagemToast('Protocolo finalizado com sucesso!', 5000);
          },
          error: (error) => {
            this.util.aguardar(false);
            console.error('Erro ao finalizar protocolo:', error);
            this.util.exibirMensagemToast('Erro ao finalizar protocolo', 5000);
          }
        });
      }
    });
  }

  gerarDocumentoDescarte(protocolo: ProtocoloDescarte, event: Event): void {
    event.stopPropagation(); // Evita selecionar o protocolo ao clicar no botão
    
    if (!protocolo.id || !this.session?.token) return;
    this.util.aguardar(true);

    this.protocoloApi.gerarDocumentoDescarte(protocolo.id, this.session.token).subscribe({
      next: (blob) => {
        this.util.aguardar(false);
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `DESCARTE_${protocolo.protocolo}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        
        this.util.exibirMensagemToast('Documento gerado e baixado com sucesso!', 5000);
      },
      error: (error) => {
        this.util.aguardar(false);
        console.error('❌ Erro ao gerar documento:', error);
        this.util.exibirMensagemToast('Erro ao gerar documento de descarte', 5000);
      }
    });
  }

  // ==================== MÉTODOS AUXILIARES ====================

  // Filtros para PROTOCOLOS
  getProtocolosEmAndamento(): ProtocoloDescarte[] {
    return this.protocolos.filter(p => p.status === 'EM_ANDAMENTO');
  }

  getProtocolosConcluidos(): ProtocoloDescarte[] {
    return this.protocolos.filter(p => p.status === 'CONCLUIDO');
  }

  // Filtros para ITENS dentro de um protocolo
  getItensEmProcesso(): ProtocoloDescarteItem[] {
    if (!this.protocoloAtual?.itens) return [];
    return this.protocoloAtual.itens.filter(item => 
      item.statusItem === 'PENDENTE' || item.statusItem === 'EM_PROCESSO'
    );
  }

  getItensConcluidos(): ProtocoloDescarteItem[] {
    if (!this.protocoloAtual?.itens) return [];
    return this.protocoloAtual.itens.filter(item => 
      item.statusItem === 'CONCLUIDO'
    );
  }

  temAlgumProcessoMarcado(item: ProtocoloDescarteItem): boolean {
    return item.processoSanitizacao || 
           item.processoDescaracterizacao || 
           item.processoPerfuracaoDisco || 
           item.evidenciasExecutadas;
  }

  obterDescricaoTipoDescarte(tipo: string): string {
    switch (tipo) {
      case TipoDescarteEnum.DOACAO: return 'Doação';
      case TipoDescarteEnum.VENDA: return 'Venda';
      case TipoDescarteEnum.DEVOLUCAO: return 'Devolução';
      case TipoDescarteEnum.LOGISTICA_REVERSA: return 'Logística Reversa';
      case TipoDescarteEnum.DESCARTE_FINAL: return 'Descarte Geral (destruição)';
      default: return tipo;
    }
  }

  obterDescricaoStatus(status: string): string {
    switch (status) {
      case StatusProtocoloEnum.EM_ANDAMENTO: return 'Em Andamento';
      case StatusProtocoloEnum.CONCLUIDO: return 'Concluído';
      case StatusProtocoloEnum.CANCELADO: return 'Cancelado';
      default: return status;
    }
  }

  obterClasseStatus(status: string): string {
    switch (status) {
      case StatusProtocoloEnum.EM_ANDAMENTO: return 'status-em-andamento';
      case StatusProtocoloEnum.CONCLUIDO: return 'status-concluido';
      case StatusProtocoloEnum.CANCELADO: return 'status-cancelado';
      default: return 'status-indefinido';
    }
  }
}
