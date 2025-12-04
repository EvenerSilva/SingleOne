import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

@Injectable({
  providedIn: 'root'
})
export class EquipamentoApiService extends ConfigApiService {
  
  public session: any = {};

  listarEquipamentos(pesquisa, contrato, pagina, paginaTamanho, modeloId = null, localidadeId = null){
    if (!pesquisa || pesquisa === '' || pesquisa === null || pesquisa === undefined) {
      pesquisa = "null"; // Backend espera literal "null"
    }
    
    if (contrato == null || contrato == 'null' || contrato == '' || contrato === undefined) {
      contrato = 0;
    } else {
      // Converter para número se for string
      contrato = parseInt(contrato, 10);
      if (isNaN(contrato)) {
        console.warn('⚠️ [DEBUG-API] Contrato inválido, usando 0:', contrato);
        contrato = 0;
      }
    }
    
    // Validar página e tamanho
    pagina = parseInt(pagina, 10) || 1;
    paginaTamanho = parseInt(paginaTamanho, 10) || 10;
    this.session = this.util.getSession('usuario');
    
    if (!this.session || !this.session.usuario || !this.session.usuario.cliente || !this.session.token) {
      console.error('[API-EQUIPAMENTOS] ❌ Sessão inválida ou incompleta');
      return Promise.reject({
        response: {
          status: 401,
          data: { message: 'Sessão inválida ou expirada' }
        }
      });
    }
    
    const cliente = this.session.usuario.cliente;
    
    // ✅ CORREÇÃO: Construir URL corretamente (controller singular) com fallback para legado (plural/minúsculo)
    let urlPrimaria = `/Equipamento/ListarEquipamentos/${pesquisa}/${cliente}/${contrato}/${pagina}/${paginaTamanho}`;
    let urlFallback1 = `/equipamentos/ListarEquipamentos/${pesquisa}/${cliente}/${contrato}/${pagina}/${paginaTamanho}`;
    let urlFallback2 = `/equipamentos/listarEquipamentos/${pesquisa}/${cliente}/${contrato}/${pagina}/${paginaTamanho}`;
    
    // Adicionar parâmetros de query se fornecidos
    if (modeloId || localidadeId) {
      const queryParams = [];
      if (modeloId) queryParams.push(`modeloId=${modeloId}`);
      if (localidadeId) queryParams.push(`localidadeId=${localidadeId}`);
      
      const queryString = queryParams.join('&');
      urlPrimaria += `?${queryString}`;
      urlFallback1 += `?${queryString}`;
      urlFallback2 += `?${queryString}`;
    }
    
    const reqConfig = { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + this.session.token
      },
      timeout: 30000
    } as any;

    return this.instance.get(urlPrimaria, reqConfig)
      .then(res => res)
      .catch(err => {
        if (err && err.response && err.response.status === 404) {
          console.warn('[API-EQUIPAMENTOS] Rota primária não encontrada. Tentando fallback 1:', urlFallback1);
          return this.instance.get(urlFallback1, reqConfig)
            .then(res => res)
            .catch(err2 => {
              if (err2 && err2.response && err2.response.status === 404) {
                console.warn('[API-EQUIPAMENTOS] Fallback 1 não encontrado. Tentando fallback 2:', urlFallback2);
                return this.instance.get(urlFallback2, reqConfig);
              }
              return Promise.reject(err2);
            });
        }
        return Promise.reject(err);
      });
  }

// 📊 MÉTODO PARA RESUMO - SEM LIMITAÇÃO DE PAGINAÇÃO (melhorado)
  listarTodosEquipamentosParaResumo(cliente) {
    if (!cliente || cliente === 0) {
      console.error('[API-EQUIPAMENTOS-RESUMO] ❌ Cliente inválido:', cliente);
      return Promise.reject({
        response: {
          status: 400,
          data: { message: 'Cliente inválido' }
        }
      });
    }

    this.session = this.util.getSession('usuario');
    if (!this.session || !this.session.token) {
      console.error('[API-EQUIPAMENTOS-RESUMO] ❌ Sessão inválida');
      return Promise.reject({
        response: {
          status: 401,
          data: { message: 'Sessão inválida ou expirada' }
        }
      });
    }

    // ✅ BUSCAR TODOS OS 1.700 EQUIPAMENTOS DO BANCO
    return this.listarEquipamentos('', 0, 1, 9999).then(response => {
      const raw = response?.data;
      const items = Array.isArray(raw)
        ? raw
        : (raw?.results || raw?.data || raw?.Data || []);
      const count = Array.isArray(items) ? items.length : 0;
      return {
        status: 200,
        data: items
      };
    }).catch(error => {
      console.error('[API-EQUIPAMENTOS-RESUMO] ❌ Erro ao buscar dados do banco:', error);
      throw error; // Re-throw para que o componente trate o erro
    });
  }

listarEquipamentosDisponiveis(pesquisa, cliente, token) {
    // Tenta primeiro rota atual (controller singular /Equipamento), com fallback para legado em minúsculo/plural
    const headers = { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }};
    const urlPrimaria = '/Equipamento/ListarEquipamentosDisponiveis/' + pesquisa + '/' + cliente;
    const urlFallback = '/equipamentos/listarEquipamentosDisponiveis/' + pesquisa + '/' + cliente;
    return this.instance.get(urlPrimaria, headers).then(res => res).catch(err => {
      if (err && err.response && err.response.status === 404) {
        return this.instance.get(urlFallback, headers).then(res => res);
      }
      return err;
    });
  }
  listarEquipamentoDisponivelParaLaudos(pesquisa, cliente, token){
    const headers = { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }};
    const urlPrimaria = '/Equipamento/ListarEquipamentoDisponivelParaLaudos/' + pesquisa + '/' + cliente;
    const urlFallback = '/equipamentos/listarEquipamentoDisponivelParaLaudos/' + pesquisa + '/' + cliente;
    return this.instance.get(urlPrimaria, headers).then(res => res).catch(err => {
      if (err && err.response && err.response.status === 404) {
        return this.instance.get(urlFallback, headers).then(res => res);
      }
      return err;
    });
  }
  listarEquipamentosDisponiveisParaEstoque(cliente, token) {
    const headers = { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }};
    const urlPrimaria = '/Equipamento/ListarEquipamentosDisponiveisParaEstoque/' + cliente;
    const urlFallback = '/equipamentos/listarEquipamentosDisponiveisParaEstoque/' + cliente;
    return this.instance.get(urlPrimaria, headers).then(res => res).catch(err => {
      if (err && err.response && err.response.status === 404) {
        return this.instance.get(urlFallback, headers).then(res => res);
      }
      return err;
    });
  }
  listarStatusEquipamentos(token) {
    const headers = { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }};
    
    const urlPrimaria = '/Equipamento/ListarStatusEquipamentos';
    const urlFallback = '/equipamentos/ListarStatusEquipamentos';
    
    return this.instance.get(urlPrimaria, headers).then(res => res).catch(err => {
      if (err && err.response && err.response.status === 404) {
        return this.instance.get(urlFallback, headers).then(res => res);
      }
      return Promise.reject(err);
    });
  }
  listarAnexosDoEquipamento(idEquipamento, token) {
    return this.instance.get('/equipamentos/listarAnexosDoEquipamento/' + idEquipamento, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  listarEquipamentosDoTermoDeEntrega(cliente, colaborador, token) {
    return this.instance.get('/equipamentos/EquipamentosDoTermoDeEntrega/' + cliente + '/' + colaborador, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  exportarParaExcel(cliente, token) {
    return this.instance.get('/equipamentos/exportarParaExcel/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

buscarEquipamentoPorId(id, token){
    const headers = { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }};

    const urlPrimaria = '/Equipamento/BuscarEquipamentoPorId/' + id;
    const urlFallback = '/equipamentos/BuscarEquipamentoPorId/' + id;

    return this.instance.get(urlPrimaria, headers).then(res => res).catch(err => {
      console.error('❌ [API-EQUIPAMENTO] Erro na API (primária):', err);
      if (err && err.response && err.response.status === 404) {
        return this.instance.get(urlFallback, headers).then(res => res);
      }
      return Promise.reject(err);
    })
  }
  visualizarRecurso(id, token){
    return this.instance.get('/equipamentos/VisualizarRecurso/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  liberarEquipamentoParaEstoque(usuario, equipamento, token) {
    const headers = { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }};
    // Tenta primeiro no controller singular (padrão atual)
    const urlPrimaria = '/Equipamento/LiberarEquipamentoParaEstoque/' + usuario + '/' + equipamento;
    // Fallback para rota antiga em minúsculo/plural
    const urlFallback = '/equipamentos/liberarEquipamentoParaEstoque/' + usuario + '/' + equipamento;

    return this.instance.get(urlPrimaria, headers)
      .then(res => res)
      .catch(err => {
        if (err && err.response && err.response.status === 404) {
          // Tenta rota antiga
          return this.instance.get(urlFallback, headers).then(res => res).catch(e => Promise.reject(e));
        }
        return Promise.reject(err);
      });
  }

salvarEquipamento(eqp, token){
    const headers = { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }};
    const urlPrimaria = '/Equipamento/SalvarEquipamento';
    const urlFallback = '/equipamentos/SalvarEquipamento';
    return this.instance.post(urlPrimaria, eqp, headers).then(res => res).catch(err => {
      if (err && err.response && err.response.status === 404) {
        return this.instance.post(urlFallback, eqp, headers).then(res => res);
      }
      return Promise.reject(err);
    })
  }
  incluirAnexo(anx, token){
    return this.instance.post('/equipamentos/incluirAnexo', anx, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  registrarBO(eqp, token) {
    return this.instance.post('/equipamentos/registrarBO', eqp, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

excluirEquipamento(id, token) {
    return this.instance.delete('/equipamentos/excluirEquipamento/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirAnexo(id, token) {
    return this.instance.delete('/equipamentos/excluirAnexo/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

//Descarte
  listarEquipamentosDisponiveisParaDescarte(cliente, pesquisa, token) {
    return this.instance.get('/Equipamento/ListarEquipamentosDisponiveisParaDescarte/' + cliente + '/' + pesquisa, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      console.error('❌ [API-DESCARTE] Erro:', err);
      console.error('❌ [API-DESCARTE] Erro detalhado:', err.response);
      return err;
    })
  }
  realizarDescarte(descartes, token) {
    return this.instance.post('/Equipamento/RealizarDescarte', descartes, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

//Reativação
  reativarEquipamento(id, token) {
    return this.instance.get('/equipamentos/ReativarEquipamento/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
}
