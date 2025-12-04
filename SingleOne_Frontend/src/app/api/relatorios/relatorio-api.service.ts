import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

@Injectable({
  providedIn: 'root'
})
export class RelatorioApiService extends ConfigApiService {

  historicoEquipamentos(id, token) {
    return this.instance.get('/relatorio/HistoricoEquipamento/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  // ✅ NOVO: Buscar histórico por número de série (resolve conflito de IDs)
  historicoEquipamentosPorNumeroSerie(numeroSerie, token) {
    return this.instance.get('/relatorio/HistoricoEquipamentoPorNumeroSerie/' + numeroSerie, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  equipamentoComColaboradores(pesquisa, token) {
    return this.instance.get('/relatorio/EquipamentoComColaboradores/' + pesquisa, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  listarLinhasTelefonicas(pesquisa, cliente, token) {
    return this.instance.get('/relatorio/ListarLinhasTelefonicas/' + pesquisa + '/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  movimentacoesColaboradores(cliente, pagina, relatorio, pesquisa, pageSize, token) {
    return this.instance.get('/relatorio/MovimentacoesColaboradores/' + cliente + '/' + pagina + '/' + relatorio + '/' + pesquisa + '/' + pageSize, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  consultarDetalhesEquipamentos(vw, token) {
    return this.instance.post('/relatorio/ConsultarDetalhesEquipamentos', vw, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  dashboardWeb(cliente, token) {
    return this.instance.get('/relatorio/DashboardWeb/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  laudosComValor(cliente, empresa, centrocusto, token) {
    return this.instance.get('/relatorio/LaudosComValor/' + cliente + '/' + empresa + '/' + centrocusto, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  consultarLogsAcesso(filtros, token) {
    return this.instance.post('/relatorio/ConsultarLogsAcesso', filtros, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  consultarGarantias(filtros, token) {
    return this.instance.post('/relatorio/ConsultarGarantias', filtros, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  consultarSinalizacoesSuspeitas(filtros, token) {
    return this.instance.post('/relatorio/ConsultarSinalizacoesSuspeitas', filtros, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  obterDetalhesSinalizacao(id, token) {
    return this.instance.get('/relatorio/SinalizacaoSuspeita/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  alterarStatusSinalizacao(id, novoStatus, observacoes, token) {
    const payload = {
      sinalizacaoId: id,
      status: novoStatus,
      observacoes: observacoes
    };

    return this.instance.put('/relatorio/AlterarStatusSinalizacao/' + id + '/Status', payload, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  atribuirInvestigador(id, investigadorId, token) {
    const payload = {
      SinalizacaoId: id,
      InvestigadorId: investigadorId  // Capitalização correta para o backend .NET
    };

    return this.instance.put('/relatorio/AtribuirInvestigadorSinalizacao/' + id + '/Investigador', payload, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  obterMotivosSuspeita(token) {
    return this.instance.get('/relatorio/MotivosSuspeita', { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  obterUsuariosInvestigadores(token) {
    return this.instance.get('/relatorio/UsuariosInvestigadores', { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  consultarColaboradoresSemRecursos(filtros, token) {
    return this.instance.post('/relatorio/ConsultarColaboradoresSemRecursos', filtros, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  obterEmpresasComColaboradores(clienteId, token) {
    return this.instance.get('/relatorio/FiltrosColaboradores/Empresas/' + clienteId, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  obterLocalidadesComColaboradores(clienteId, token) {
    return this.instance.get('/relatorio/FiltrosColaboradores/Localidades/' + clienteId, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  obterCentrosCustoComColaboradores(clienteId, token) {
    return this.instance.get('/relatorio/FiltrosColaboradores/CentrosCusto/' + clienteId, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  /**
   * 🗺️ Obter Mapa de Recursos com drilldown hierárquico
   */
  obterMapaRecursos(filtros, token) {
    return this.instance.post('/relatorio/ObterMapaRecursos', filtros, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
}
