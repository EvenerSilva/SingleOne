import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

// ✅ NOVO: Interface para tipar a resposta da API
interface ApiResponse {
  status: number;
  data: any;
  headers?: any;
  error?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class RequisicaoApiService extends ConfigApiService {

  listarRequisicoes(pesquisa, cliente, pagina, token): Promise<ApiResponse> {
    // ✅ CORREÇÃO: Tratar filtro vazio para evitar barras duplas na URL
    const filtroTratado = pesquisa === '' || pesquisa === null ? 'null' : pesquisa;
    let url;
    if (filtroTratado === 'null') {
      url = `/requisicoes/ListarRequisicoes/null/${cliente}/${pagina}`;
    } else {
      url = `/requisicoes/ListarRequisicoes/${filtroTratado}/${cliente}/${pagina}`;
    }
    return this.instance.get(url, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return {
        status: res.status,
        data: res.data,
        headers: res.headers
      };
    }).catch(err => {
      console.error('[REQUISICAO-API] Erro na requisição:', err);
      console.error('[REQUISICAO-API] Status do erro:', err.response?.status);
      console.error('[REQUISICAO-API] Data do erro:', err.response?.data);
      console.error('[REQUISICAO-API] URL que falhou:', url);
      console.error('[REQUISICAO-API] URL completa que falhou:', this.instance.defaults.baseURL + url);
      
      // ✅ CORREÇÃO: Retornar estrutura de erro padronizada
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { error: 'Erro desconhecido' },
        error: true
      };
    })
  }
  obterRequisicaoPorId(id, token) {
    return this.instance.get('/requisicoes/BuscarRequisicaoPorID/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  listarEquipamentosDaRequisicao(hash, isByod) {
    const url = '/requisicoes/ListarEquipamentosDaRequisicao/'.concat(hash, '/', isByod);
    return this.instance.get(url).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  // ✅ NOVO: Método específico para buscar requisições por equipamento
  listarRequisicoesPorEquipamento(numeroSerie, cliente, token): Promise<ApiResponse> {
    const url = `/requisicoes/ListarRequisicoesPorEquipamento/${numeroSerie}/${cliente}`;
    return this.instance.get(url, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return {
        status: res.status,
        data: res.data,
        headers: res.headers
      };
    }).catch(err => {
      console.error('[REQUISICAO-API] Erro na busca por equipamento:', err);
      console.error('[REQUISICAO-API] Status do erro:', err.response?.status);
      console.error('[REQUISICAO-API] Data do erro:', err.response?.data);
      
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { error: 'Erro desconhecido' },
        error: true
      };
    });
  }
  salvarRequisicao(req, token): Promise<ApiResponse> {
    return this.instance.post('/requisicoes/SalvarRequisicao', req, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return {
        status: res.status,
        data: res.data,
        headers: res.headers
      };
    }).catch(err => {
      console.error('[REQUISICAO-API] Erro ao salvar requisição:', err);
      console.error('[REQUISICAO-API] Status do erro:', err.response?.status);
      console.error('[REQUISICAO-API] Data do erro:', err.response?.data);
      
      // ✅ CORREÇÃO: Retornar estrutura de erro padronizada
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { error: 'Erro desconhecido' },
        error: true
      };
    })
  }
  aceitarTermoResponsabilidade(vm) {
    return this.instance.post('/requisicoes/AceitarTermoResponsabilidade', vm).then(res => {
      return res;
    }).catch(err => {
      console.error('[REQUISICAO-API] ❌ Erro ao aceitar termo:', err);
      console.error('[REQUISICAO-API] Status do erro:', err.response?.status);
      console.error('[REQUISICAO-API] Data do erro:', err.response?.data);
      console.error('[REQUISICAO-API] Mensagem:', err.message);
      
      // ✅ Retornar estrutura padronizada de erro
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { Mensagem: 'Erro de conexão: ' + err.message, Status: '500' },
        error: true
      };
    })
  }

listarEntregasDisponiveis(cliente, token) {
    return this.instance.get('/requisicoes/ListarEntregasDisponiveis/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  obterEntregaPorId(id, token) {
    return this.instance.get('/requisicoes/BuscarEntregaPorID/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  realizarEntrega(req, token) {
    return this.instance.post('/requisicoes/realizarEntrega', req, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  realizarEntregaComCompartilhados(dto, token) {
    return this.instance.post('/requisicoes/RealizarEntregaComCompartilhados', dto, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  listarCompartilhadosItem(requisicaoItemId, token) {
    return this.instance.get('/requisicoes/ListarCompartilhadosItem/' + requisicaoItemId, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => res).catch(err => err);
  }

  adicionarCompartilhadoItem(requisicaoItemId, vinculo, token) {
    return this.instance.post('/requisicoes/AdicionarCompartilhadoItem/' + requisicaoItemId, vinculo, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => res).catch(err => err);
  }

  encerrarCompartilhadoItem(vinculoId, token) {
    return this.instance.patch('/requisicoes/EncerrarCompartilhadoItem/' + vinculoId + '/encerrar', {}, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => res).catch(err => err);
  }

listarDevolucoesDisponivels(pesquisa, cliente, pagina, token) {
    const url = '/requisicoes/ListarDevolucoesDisponiveis/'.concat(pesquisa, '/', cliente, '/', pagina, '/false');
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  listarBYOD(pesquisa, cliente, pagina, token) {
    const url = '/requisicoes/ListarDevolucoesDisponiveis/'.concat(pesquisa, '/', cliente, '/', pagina, '/true');
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  realizarDevolucaoEquipamento(eqp, token) {
    return this.instance.post('/requisicoes/RealizarDevolucaoEquipamento', eqp, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  realizarDevolucoesColaborador(requisicaovm, token) {
    return this.instance.post('/requisicoes/RealizarDevolucoesDoColaborador/false', requisicaovm, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  realizarDevolucoesColaboradorBYOD(requisicaovm, token) {
    return this.instance.post('/requisicoes/RealizarDevolucoesDoColaborador/true', requisicaovm, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  atualizarItemRequisicao(ri, token) {
    return this.instance.post('/requisicoes/AtualizarItemRequisicao', ri, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  adicionarObservacaoEquipamentoVM(equipamento, token) {
    return this.instance.post('/requisicoes/AdicionarObservacaoEquipamentoVM', equipamento, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  adicionarAgendamentoEquipamentoVM(equipamento, token) {
    return this.instance.post('/requisicoes/AdicionarAgendamentoEquipamentoVM', equipamento, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
}
