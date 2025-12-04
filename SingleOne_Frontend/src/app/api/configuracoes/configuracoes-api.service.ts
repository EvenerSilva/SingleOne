import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

@Injectable({
  providedIn: 'root'
})
export class ConfiguracoesApiService extends ConfigApiService {
  public session: any = {};

  /**********************************************************************************************/
  /***************************************** CLIENTES *******************************************/
  /**********************************************************************************************/
  listarClientes(pesquisa, token) {
    return this.instance.get('/configuracoes/listarClientes/' + pesquisa, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarCliente(cli, token) {
    return this.instance.post('/configuracoes/salvarCliente', cli, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      throw err;  // ✅ CORRIGIDO: Rejeita a Promise para que o componente trate no .catch()
    });
  }
  excluirCliente(id, token) {
    return this.instance.delete('/configuracoes/excluirCliente/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/**********************************************************************************************/
  /***************************************** EMPRESAS *******************************************/
  /**********************************************************************************************/
  listarEmpresas(pesquisa, cliente, token){
    const pesquisaTratada = pesquisa || 'null';
    const url = '/empresas/listarEmpresas/' + pesquisaTratada + '/' + cliente;
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  buscarEmpresaPorId(id, token) {
    return this.instance.get('/empresas/buscarEmpresaPeloID/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarEmpresa(empresa, token) {
    return this.instance.post('/empresas/salvarEmpresa', empresa, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      throw err;  // ✅ CORRIGIDO: Rejeita a Promise para que o componente trate no .catch()
    })
  }
  excluirEmpresa(id, token) {
    return this.instance.delete('/empresas/excluirEmpresa/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/**********************************************************************************************/
  /***************************************** CENTRO CUSTOS **************************************/
  /**********************************************************************************************/
  listarCentroCusto(pesquisa, cliente, token){
    // ✅ CORREÇÃO: Backend espera a string literal 'null' para não filtrar
    let pesquisaParam = 'null';
    if (pesquisa !== null && pesquisa !== undefined) {
      const valor = typeof pesquisa === 'string' ? pesquisa.trim() : pesquisa;
      if (valor !== '' && valor !== 'null') {
        pesquisaParam = valor;
      }
    }
    const clienteParam = cliente !== null && cliente !== undefined ? cliente : 0;
    
    // ✅ CORREÇÃO: Construir URL corretamente para evitar barras duplas
    const url = `/configuracoes/ListarCentroCustos/${pesquisaParam}/${clienteParam}`;
    
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  listarCentroCustoDaEmpresa(idEmpresa, token){
    return this.instance.get('/configuracoes/ListarCentrosDaEmpresa/' + idEmpresa, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  buscarCentroPorId(id, token) {
    return this.instance.get('/configuracoes/BuscarCentroPorID/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarCentroCusto(cc, token) {
    return this.instance.post('/configuracoes/SalvarCentroCusto', cc, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirCentroCusto(id, token) {
    return this.instance.delete('/configuracoes/ExcluirCentroCusto/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/**********************************************************************************************/
  /***************************************** FORNECEDORES ***************************************/
  /**********************************************************************************************/
  listarFornecedores(pesquisa, cliente, token){
    // ✅ CORREÇÃO: Tratar valores null e vazios adequadamente
    let pesquisaParam = 'null';
    if (pesquisa !== null && pesquisa !== undefined && pesquisa !== 'null' && pesquisa.trim() !== '') {
      pesquisaParam = pesquisa;
    }
    const clienteParam = cliente !== null && cliente !== undefined ? cliente : 0;
    
    // ✅ CORREÇÃO: Construir URL corretamente para evitar barras duplas
    const url = `/configuracoes/ListarFornecedores/${pesquisaParam}/${clienteParam}`;
    
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarFornecedor(fnc, token) {
    return this.instance.post('/configuracoes/SalvarFornecedor', fnc, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      throw err;  // ✅ CORRIGIDO: Rejeita a Promise para que o componente trate no .catch()
    });
  }
  excluirFornecedor(id, token) {
    return this.instance.delete('/configuracoes/ExcluirFornecedor/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/**********************************************************************************************/
  /***************************************** TIPOS DE RECURSOS **********************************/
  /**********************************************************************************************/
  listarTiposRecursos(pesquisa, cliente, token){
    // ✅ CORREÇÃO: Backend espera a string literal 'null' para não filtrar
    let pesquisaParam = 'null';
    if (pesquisa !== null && pesquisa !== undefined) {
      const valor = typeof pesquisa === 'string' ? pesquisa.trim() : pesquisa;
      if (valor !== '' && valor !== 'null') {
        pesquisaParam = valor;
      }
    }
    const clienteParam = cliente !== null && cliente !== undefined ? cliente : 0;
    
    // ✅ CORREÇÃO: Construir URL corretamente para evitar barras duplas
    const url = `/configuracoes/ListarTiposRecursos/${pesquisaParam}/${clienteParam}`;
    
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarTipoRecurso(tr, token) {
    return this.instance.post('/configuracoes/SalvarTipoRecurso', tr, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirTipoRecurso(id, cliente, token) {
    return this.instance.delete('/configuracoes/ExcluirTipoRecurso/' + id + "/" + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  listarTiposAquisicao(token){
    return this.instance.get('/configuracoes/ListarTiposAquisicao/', { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/**********************************************************************************************/
  /***************************************** FABRICANTES ****************************************/
  /**********************************************************************************************/
  listarFabricantes(pesquisa, cliente, token){
    // ✅ CORREÇÃO: Backend espera a string literal 'null' para não filtrar
    let pesquisaParam = 'null';
    if (pesquisa !== null && pesquisa !== undefined) {
      const valor = typeof pesquisa === 'string' ? pesquisa.trim() : pesquisa;
      if (valor !== '' && valor !== 'null') {
        pesquisaParam = valor;
      }
    }
    const clienteParam = cliente !== null && cliente !== undefined ? cliente : 0;
    
    // ✅ CORREÇÃO: Construir URL corretamente para evitar barras duplas
    const url = `/configuracoes/ListarFabricantes/${pesquisaParam}/${clienteParam}`;
    
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  listarFabricantesPorTipoRecurso(tipo, cliente, token){
    // ✅ CORREÇÃO: Tratar valores null adequadamente
    const tipoParam = tipo !== null && tipo !== undefined ? tipo : 0;
    const clienteParam = cliente !== null && cliente !== undefined ? cliente : 0;
    
    return this.instance.get('/configuracoes/ListarFabricantesPorTipoRecurso/' + tipoParam + '/' + clienteParam, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarFabricante(tr, token) {
    return this.instance.post('/configuracoes/SalvarFabricante', tr, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirFabricante(id, token) {
    return this.instance.delete('/configuracoes/ExcluirFabricante/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/**********************************************************************************************/
  /***************************************** MODELOS ********************************************/
  /**********************************************************************************************/
  listarModelos(pesquisa, cliente, token){
    // ✅ CORREÇÃO: Backend espera a string literal 'null' para não filtrar
    let pesquisaParam = 'null';
    if (pesquisa !== null && pesquisa !== undefined) {
      const valor = typeof pesquisa === 'string' ? pesquisa.trim() : pesquisa;
      if (valor !== '' && valor !== 'null') {
        pesquisaParam = valor;
      }
    }
    const clienteParam = cliente !== null && cliente !== undefined ? cliente : 0;
    
    // ✅ CORREÇÃO: Construir URL corretamente para evitar barras duplas
    const url = `/configuracoes/ListarModelos/${pesquisaParam}/${clienteParam}`;
    
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  listarModelosDoFabricante(fabricante, cliente, token){
    // ✅ CORREÇÃO: Tratar valores null adequadamente
    const fabricanteParam = fabricante !== null && fabricante !== undefined ? fabricante : 0;
    const clienteParam = cliente !== null && cliente !== undefined ? cliente : 0;
    
    return this.instance.get('/configuracoes/ListarModelosDoFabricante/' + fabricanteParam + '/' + clienteParam, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarModelo(tr, token) {
    return this.instance.post('/configuracoes/SalvarModelo', tr, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirModelo(id, token) {
    return this.instance.delete('/configuracoes/ExcluirModelo/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/**********************************************************************************************/
  /***************************************** NOTAS FISCAIS **************************************/
  /**********************************************************************************************/
  listarNotasFiscais(pesquisa, cliente, token){
    // ✅ CORREÇÃO: Tratar valores null e vazios adequadamente
    let pesquisaParam = 'null';
    if (pesquisa !== null && pesquisa !== undefined && pesquisa !== 'null' && pesquisa.trim() !== '') {
      pesquisaParam = pesquisa;
    }
    const clienteParam = cliente !== null && cliente !== undefined ? cliente : 0;
    
    // ✅ CORREÇÃO: Construir URL corretamente para evitar barras duplas
    const url = `/configuracoes/ListarNotasFiscais/${pesquisaParam}/${clienteParam}`;
    
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  buscarNotaFiscalPorId(id, token){
    return this.instance.get('/configuracoes/BuscarNotaPorId/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  visualizarNotaFiscal(id){
    this.session = this.util.getSession('usuario');
    return this.instance.get('/configuracoes/VisualizarNotaFiscal/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.session.token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

salvarNotaFiscal(nf, token) {
    return this.instance.post('/configuracoes/SalvarNotaFiscal', nf, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  adicionarItemNotaFiscal(nfi, token) {
    return this.instance.post('/configuracoes/AdicionarItemNota', nfi, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirNotaFiscal(id, token) {
    return this.instance.delete('/configuracoes/ExcluirNotaFiscal/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirItemNotaFiscal(id, token) {
    return this.instance.delete('/configuracoes/ExcluirItemNota/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  liberarParaEstoque(nfvm, token) {
    return this.instance.post('/configuracoes/LiberarParaEstoque', nfvm, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/**********************************************************************************************/
  /***************************************** LAUDOS *********************************************/
  /**********************************************************************************************/
  listarLaudos(pesquisa, cliente, token){
    return this.instance.get('/configuracoes/ListarLaudos/' + pesquisa + '/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  buscarLaudoPorId(id, token){
    return this.instance.get('/configuracoes/BuscarLaudoPorId/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarLaudo(laudo, token) {
    return this.instance.post('/configuracoes/SalvarLaudo', laudo, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  encerrarLaudo(laudo, token) {
    return this.instance.post('/configuracoes/EncerrarLaudo', laudo, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  gerarLaudoEmPDF(id, token, templateId = null) {
    let url = '/configuracoes/GerarLaudoEmPDF/' + id;
    if (templateId) {
      url += '?templateId=' + templateId;
    }
    
    return this.instance.get(url, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
      // Removido responseType: 'blob' para deixar o Axios detectar automaticamente
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/**********************************************************************************************/
  /***************************************** TEMPLATES *****************************************/
  /**********************************************************************************************/
  listarTiposDeTemplates(token) {
    return this.instance.get('/configuracoes/ListarTiposDeTemplates', { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  listarTemplates(cliente, token) {
    return this.instance.get('/configuracoes/ListarTemplates/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  
  listarTemplatesPorTipo(cliente, tipo, token) {
    return this.instance.get('/configuracoes/ListarTemplatesPorTipo/' + cliente + '/' + tipo, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  obterTemplatePorId(id, token) {
    return this.instance.get('/configuracoes/ObterTemplatePorID/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  visualizarTemplate(tmp, token) {
    return this.instance.post('/configuracoes/VisualizarTemplate', tmp, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarTemplate(tmp, token) {
    return this.instance.post('/configuracoes/SalvarTemplate', tmp, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  excluirTemplate(id, token) {
    return this.instance.delete('/configuracoes/ExcluirTemplate/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

/**********************************************************************************************/
  /***************************************** PARAMETROS *****************************************/
  /**********************************************************************************************/
  obterParametros(cliente, token) {
    return this.instance.get('/configuracoes/BuscarParametros/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
  salvarParametro(param, token) {
    return this.instance.post('/configuracoes/SalvarParametros', param, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  testarConexaoSMTP(parametro, token) {
    return this.instance.post('/configuracoes/testarConexaoSMTP', parametro, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  /**********************************************************************************************/
  /***************************************** LOGOS **********************************************/
  /**********************************************************************************************/
  uploadLogoCliente(formData: FormData, token: string) {
    return this.instance.post('/configuracoes/UploadLogoCliente', formData, { 
      headers: {
        'Authorization': 'Bearer ' + token
        // Não incluir Content-Type para FormData - o navegador define automaticamente
      },
      timeout: 30000 // 30 segundos de timeout
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API] Erro no upload:', err);
      return err;
    })
  }

  buscarLogoCliente() {
    return this.instance.get('/configuracoes/BuscarLogoCliente', { 
      headers: {
        'Content-Type': 'application/json'
      },
      timeout: 10000 // 10 segundos de timeout
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API] Erro ao buscar logo:', err);
      return err;
    })
  }

  /**********************************************************************************************/
  /************************************** LAUDO EVIDÊNCIAS *************************************/
  /**********************************************************************************************/
  listarEvidenciasLaudo(laudoId: number) {
    return this.instance.get(`/configuracoes/ListarEvidenciasLaudo/${laudoId}`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + this.getToken()
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  uploadEvidenciaLaudo(formData: FormData): Promise<any> {
    return this.instance.post('/configuracoes/UploadEvidenciaLaudo', formData, {
      headers: {
        'Authorization': 'Bearer ' + this.getToken()
        // Não incluir Content-Type para FormData
      },
      timeout: 60000 // 60 segundos para upload de imagens
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API] Erro no upload de evidência:', err);
      // Retornar uma resposta estruturada em caso de erro
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { mensagem: 'Erro interno no upload' },
        error: true
      };
    })
  }

  excluirEvidenciaLaudo(evidenciaId: number) {
    return this.instance.delete(`/configuracoes/ExcluirEvidenciaLaudo/${evidenciaId}`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + this.getToken()
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  reordenarEvidenciasLaudo(request: { laudoId: number, ordemEvidencias: number[] }) {
    return this.instance.post('/configuracoes/ReordenarEvidenciasLaudo', request, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + this.getToken()
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  downloadEvidenciaLaudo(evidenciaId: number) {
    return this.instance.get(`/configuracoes/DownloadEvidenciaLaudo/${evidenciaId}`, {
      headers: {
        'Authorization': 'Bearer ' + this.getToken()
      },
      responseType: 'blob' // Para download de arquivos
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  private getToken(): string {
    // Assumindo que o token está armazenado na sessão
    const session = JSON.parse(localStorage.getItem('usuario') || '{}');
    return session.token || '';
  }

  /**********************************************************************************************/
  /***************************************** LOCALIDADES ****************************************/
  /**********************************************************************************************/
  listarLocalidades(cliente, token) {
    return this.instance.get('/configuracoes/ListarLocalidades/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  /// <summary>
  /// ✅ NOVO: Retorna apenas as localidades de uma empresa específica
  /// Implementa cascata: Empresa → Localidades da empresa
  /// </summary>
  listarLocalidadesDaEmpresa(empresaId, token) {
    return this.instance.get('/configuracoes/LocalidadesDaEmpresa/' + empresaId, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  salvarLocalidade(local, token) {
    return this.instance.post('/configuracoes/SalvarLocalidade', local, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  excluirLocalidade(id, token) {
    return this.instance.delete('/configuracoes/ExcluirLocalidade/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  /**********************************************************************************************/
  /***************************************** FILIAIS ********************************************/
  /**********************************************************************************************/
  listarFiliais(pesquisa, cliente, token){
    // Tratar string vazia e valores null adequadamente
    let pesquisaParam = 'null';
    if (pesquisa !== null && pesquisa !== undefined && pesquisa !== 'null') {
      const valor = typeof pesquisa === 'string' ? pesquisa.trim() : pesquisa;
      if (valor !== '') {
        pesquisaParam = valor;
      }
    }
    
    const url = '/configuracoes/ListarFiliais/' + pesquisaParam + '/' + cliente;
    
    return this.instance.get(url, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  /// <summary>
  /// ✅ NOVO: Retorna apenas as filiais de uma empresa específica
  /// Implementa cascata: Empresa → Filiais da empresa
  /// </summary>
  listarFiliaisDaEmpresa(empresaId, token) {
    return this.instance.get('/configuracoes/FiliaisDaEmpresa/' + empresaId, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  /// <summary>
  /// ✅ NOVO: Retorna apenas as filiais de uma empresa E localidade específicas
  /// Implementa cascata: Localidade → Filiais da localidade
  /// </summary>
  listarFiliaisPorLocalidade(empresaId, localidadeId, token) {
    return this.instance.get('/configuracoes/FiliaisPorLocalidade/' + empresaId + '/' + localidadeId, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  buscarFilialPorId(id, token) {
    return this.instance.get('/configuracoes/BuscarFilialPeloID/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  // Alias para compatibilidade
  obterFilialPorId(id, token) {
    return this.buscarFilialPorId(id, token);
  }

  salvarFilial(filial, token) {
    return this.instance.post('/configuracoes/SalvarFilial', filial, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  excluirFilial(id, token) {
    return this.instance.delete('/configuracoes/ExcluirFilial/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }
}
