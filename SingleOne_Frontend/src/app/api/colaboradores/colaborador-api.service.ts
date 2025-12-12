import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

@Injectable({
  providedIn: 'root'
})
export class ColaboradorApiService extends ConfigApiService{
  public session: any = {};
  listarColaboradores(pesquisa, cliente, pagina, token, tipoFiltro = null) {
    let url = '/colaborador/ListarColaboradores/' + pesquisa + '/' + cliente + '/' + pagina;
    // ✅ CORREÇÃO: Sempre passar tipoFiltro se não for null/undefined/vazio
    if (tipoFiltro && tipoFiltro !== 'null' && tipoFiltro !== 'total') {
      url += '?tipoFiltro=' + encodeURIComponent(tipoFiltro);
    }
    console.log('[API] URL chamada:', url, 'tipoFiltro:', tipoFiltro);
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      console.error('[API] Erro ao listar colaboradores:', err);
      return err;
    })
  }
  obterEstatisticas(cliente, token){
    return this.instance.get('/colaborador/ObterEstatisticas/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  listarColaboradoresAtivos(pesquisa, cliente, token){
    return this.instance.get('/colaborador/ListarColaboradoresAtivos/' + pesquisa + '/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  pesquisarColaboradores(pesquisa, cliente, token){
    return this.instance.get('/colaborador/ListarColaboradores/' + pesquisa + '/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  obterColaboradorPorID(id, token){
    return this.instance.get('/colaborador/ObterColaboradorPorId/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarColaborador(col, token){
    return this.instance.post('/colaborador/SalvarColaborador', col, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirColaborador(id, token){
    return this.instance.delete('/colaborador/ExcluirColaborador/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  termoCompromisso(cliente, colaborador, usuarioLogado, token){
    const url = '/colaborador/TermoCompromisso/'.concat(cliente, '/', colaborador, '/', usuarioLogado, '/false')
    return this.instance.get(url, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      },
      responseType: 'json',
      validateStatus: function (status) {
        // Aceitar status 200-299 como sucesso, mas também processar 400 para capturar mensagem de erro
        return status >= 200 && status < 300;
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API] Erro ao buscar termo de compromisso:', err);
      console.error('[API] Detalhes do erro:', {
        status: err?.response?.status,
        data: err?.response?.data,
        message: err?.message,
        url: url
      });
      
      // Extrair mensagem de erro do backend
      let errorMessage = 'Erro ao gerar termo de compromisso';
      
      if (err?.response?.data) {
        // Se data é uma string (mensagem direta do backend)
        if (typeof err.response.data === 'string') {
          errorMessage = err.response.data;
        }
        // Se data é um objeto, tentar extrair a mensagem
        else if (typeof err.response.data === 'object') {
          errorMessage = err.response.data.message || err.response.data.title || err.response.data.error || JSON.stringify(err.response.data);
        }
      } else if (err?.message) {
        errorMessage = err.message;
      }
      
      // Retornar erro de forma estruturada mantendo a estrutura de resposta
      return {
        status: err?.response?.status || 400,
        data: errorMessage,
        error: true
      };
    })
  }

  termoCompromissoBYOD(cliente, colaborador, usuarioLogado, token){
    const url = '/colaborador/TermoCompromisso/'.concat(cliente, '/', colaborador, '/', usuarioLogado, '/true')
    return this.instance.get(url, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      },
      responseType: 'json',
      validateStatus: function (status) {
        // Aceitar status 200-299 como sucesso, mas também processar 400 para capturar mensagem de erro
        return status >= 200 && status < 300;
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API] Erro ao buscar termo de compromisso BYOD:', err);
      console.error('[API] Detalhes do erro:', {
        status: err?.response?.status,
        data: err?.response?.data,
        message: err?.message,
        url: url
      });
      
      // Extrair mensagem de erro do backend
      let errorMessage = 'Erro ao gerar termo de compromisso BYOD';
      
      if (err?.response?.data) {
        // Se data é uma string (mensagem direta do backend)
        if (typeof err.response.data === 'string') {
          errorMessage = err.response.data;
        }
        // Se data é um objeto, tentar extrair a mensagem
        else if (typeof err.response.data === 'object') {
          errorMessage = err.response.data.message || err.response.data.title || err.response.data.error || JSON.stringify(err.response.data);
        }
      } else if (err?.message) {
        errorMessage = err.message;
      }
      
      // Retornar erro de forma estruturada mantendo a estrutura de resposta
      return {
        status: err?.response?.status || 400,
        data: errorMessage,
        error: true
      };
    })
  }

  termoPorEmail(cliente, colaborador, usuarioLogado, token){
    const url = '/colaborador/TermoPorEmail/'.concat(cliente, '/', colaborador, '/', usuarioLogado, '/false')
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  termoPorEmailBYOD(cliente, colaborador, usuarioLogado, token){
    const url = '/colaborador/TermoPorEmail/'.concat(cliente, '/', colaborador, '/', usuarioLogado, '/true')
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  nadaConsta(colaborador, cliente, token){
    return this.instance.get('/colaborador/NadaConsta/' + colaborador + '/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  termoNadaConsta(colaborador){
    this.session = this.util.getSession('usuario');
    const cliente = this.session.usuario.cliente;
    const usuarioLogado = this.session.usuario.id;
    const parametros = colaborador + '/' + cliente + '/' + usuarioLogado
    return this.instance.get('/colaborador/TermoNadaConsta/' + parametros, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.session.token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  colaboradoresComTermoPorAssinar(pesquisa, cliente, filtro, token) {
    return this.instance.get('/colaborador/ColaboradoresComTermoPorAssinar/' + pesquisa + '/' + cliente + "/" + filtro, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  registrarLocalizacaoAssinatura(dados, token) {
    return this.instance.post('/colaborador/RegistrarLocalizacaoAssinatura', dados, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  //DESCARTE e DESCARTE CARGOS
  listarCargos(pesquisa, cliente, token) {
    return this.instance.get('/colaborador/ListarCargos/' + cliente + '/' + pesquisa, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  listarCargosDescarte(cliente, token) {
    return this.instance.get('/colaborador/ListarCargosDescarte/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarCargoDescarte(cargo, token){
    return this.instance.post('/colaborador/SalvarCargoDescarte', cargo, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirCargoDescarte(id, token){
    return this.instance.delete('/colaborador/ExcluirCargoDescarte/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  programarDesligamento(col, token){
    return this.instance.post('/colaborador/ProgramarDesligamento', col, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  //CARGOS DE CONFIANÇA
  listarCargosUnicos(cliente, token) {
    return this.instance.get('/colaborador/cargosconfianca/ListarUnicos/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  listarCargosConfianca(cliente, token) {
    return this.instance.get('/colaborador/cargosconfianca/Listar/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  salvarCargoConfianca(cargo, token) {
    return this.instance.post('/colaborador/cargosconfianca/Salvar', cargo, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      // Retornar a resposta de erro com status e dados
      if (err.response) {
        return {
          status: err.response.status,
          data: err.response.data,
          error: true
        };
      }
      return err;
    })
  }

  atualizarCargoConfianca(cargo, token) {
    return this.instance.put('/colaborador/cargosconfianca/Atualizar/' + cargo.id, cargo, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      // Retornar a resposta de erro com status e dados
      if (err.response) {
        return {
          status: err.response.status,
          data: err.response.data,
          error: true
        };
      }
      return err;
    })
  }

  excluirCargoConfianca(id, token) {
    return this.instance.delete('/colaborador/cargosconfianca/Excluir/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  verificarCargoConfianca(cargo, cliente, token) {
    return this.instance.get('/colaborador/cargosconfianca/Verificar/' + encodeURIComponent(cargo) + '/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  buscarHistoricoUsuarios(equipamentoId, token) {
    return this.instance.get('/equipamento/HistoricoUsuarios/' + equipamentoId, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
}
