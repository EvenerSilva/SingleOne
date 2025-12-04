import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

@Injectable({
  providedIn: 'root'
})
export class TelefoniaApiService extends ConfigApiService {

  /***************************************************************************************************/
  /******************************************** OPERADORAS *******************************************/
  /***************************************************************************************************/
  listarOperadoras(token){
    return this.instance.get('/telefonia/ListarOperadoras', { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarOperadora(to, token) {
    return this.instance.post('/telefonia/SalvarOperadora', to, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirOperadora(id, token) {
    return this.instance.delete('/telefonia/ExcluirOperadora/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/***************************************************************************************************/
  /******************************************** CONTRATOS ********************************************/
  /***************************************************************************************************/
  listarContratos(pesquisa, operadora, cliente, token){
    // Tratar valores vazios para evitar barras duplas na URL
    const pesquisaParam = pesquisa || 'todos';
    const operadoraParam = operadora || 0;
    const clienteParam = cliente || 0;
    
    return this.instance.get('/telefonia/ListarContratos/' + pesquisaParam + '/' + operadoraParam + '/' + clienteParam, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarContrato(tc, token) {
    return this.instance.post('/telefonia/SalvarContrato', tc, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirContrato(id, token) {
    return this.instance.delete('/telefonia/ExcluirContrato/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  /***************************************************************************************************/
  /******************************************** PLANOS ***********************************************/
  /***************************************************************************************************/
  listarPlanos(pesquisa, contrato, cliente, token){
    // Tratar valores vazios para evitar barras duplas na URL
    const pesquisaParam = pesquisa || 'todos';
    const contratoParam = contrato || 0;
    const clienteParam = cliente || 0;
    
    return this.instance.get('/telefonia/ListarPlanos/' + pesquisaParam + '/' + contratoParam + '/' + clienteParam, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  
  // 🆕 MÉTODO SIMPLES PARA LISTAR TODOS OS PLANOS
  listarTodosPlanos(token){
    return this.instance.get('/telefonia/ListarTodosPlanos', { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarPlano(tp, token) {
    return this.instance.post('/telefonia/SalvarPlano', tp, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirPlano(id, token) {
    return this.instance.delete('/telefonia/ExcluirPlano/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  /***************************************************************************************************/
  /******************************************** LINHAS ***********************************************/
  /***************************************************************************************************/
  listarLinhas(pesquisa, cliente, pagina, token){
    // Tratar valores vazios para evitar barras duplas na URL
    const pesquisaParam = pesquisa || 'todos';
    const clienteParam = cliente || 0;
    const paginaParam = pagina || 1;
    
    return this.instance.get('/telefonia/ListarLinhas/' + pesquisaParam + '/' + clienteParam + '/' + paginaParam, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  // 🆕 NOVOS MÉTODOS PARA FILTROS ESPECÍFICOS
  listarLinhasPorConta(contaId: number, cliente: number, pagina: number, token: string) {
    return this.instance.get(`/telefonia/ListarLinhasPorConta/${contaId}/${cliente}/${pagina}`, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  listarLinhasPorPlano(planoId: number, cliente: number, pagina: number, token: string) {
    return this.instance.get(`/telefonia/ListarLinhasPorPlano/${planoId}/${cliente}/${pagina}`, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  listarLinhasPorTipo(contaId: number, tipo: string, cliente: number, pagina: number, token: string) {
    return this.instance.get(`/telefonia/ListarLinhasPorTipo/${contaId}/${tipo}/${cliente}/${pagina}`, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  listarLinhasPorPlanoETipo(planoId: number, tipo: string, cliente: number, pagina: number, token: string) {
    return this.instance.get(`/telefonia/ListarLinhasPorPlanoETipo/${planoId}/${tipo}/${cliente}/${pagina}`, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }
  listarLinhasDisponiveisParaRequisicao(pesquisa, cliente, token){
    return this.instance.get('/telefonia/LinhasDisponiveisParaRequisicao/' + pesquisa + '/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  // 🆕 NOVO MÉTODO PARA EXPORTAÇÃO COM DADOS COMPLETOS
  listarLinhasParaExportacao(pesquisa, cliente, token){
    const pesquisaParam = pesquisa || 'todos';
    return this.instance.get('/telefonia/ListarLinhasParaExportacao/' + pesquisaParam + '/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarLinha(tl, token) {
    return this.instance.post('/telefonia/SalvarLinha', tl, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  buscarLinhaPorId(id, token) {
    return this.instance.get('/telefonia/BuscarLinhaPorId/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  
  excluirLinha(id, token) {
    return this.instance.delete('/telefonia/ExcluirLinha/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  exportarLinhasParaExcel(cliente, token) {
    return this.instance.get('/telefonia/exportarLinhasParaExcel/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  /***************************************************************************************************/
  /****************************************** CONTADORES *********************************************/
  /***************************************************************************************************/
  contarOperadoras(token) {
    return this.instance.get('/telefonia/ContarOperadoras', { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  contarContratos(token) {
    return this.instance.get('/telefonia/ContarContratos', { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  contarPlanos(token) {
    return this.instance.get('/telefonia/ContarPlanos', { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  contarLinhas(token) {
    return this.instance.get('/telefonia/ContarLinhas', { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
}
