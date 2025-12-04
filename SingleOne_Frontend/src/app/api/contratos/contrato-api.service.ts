import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

@Injectable({
  providedIn: 'root'
})
export class ContratoApiService extends ConfigApiService {
  private session: any = {};
  public cliente = 0;

  listar() {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    return this.instance.get('/contrato/Listar/' + this.cliente, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + this.session.token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-CONTRATO] Erro na listagem:', err);
      return err;
    })
  }

  listarPorFornecedor(fornecedor) {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    const url = '/contrato/Listar/' + this.cliente + '/' + fornecedor;
    return this.instance.get(url, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + this.session.token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-CONTRATO] Erro na listagem por fornecedor:', err);
      return err;
    })
  }

  listarStatus() {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    return this.instance.get('/contrato/ListarStatus/', {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + this.session.token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  criarNovoContrato(novoContrato) {
    this.session = this.util.getSession('usuario');
    return this.instance.post('/contrato', novoContrato, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.session.token
    }}).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-CONTRATO] Erro da API:', err);
      console.error('[API-CONTRATO] Status do erro:', err.response?.status);
      console.error('[API-CONTRATO] Dados do erro:', err.response?.data);
      return err;
    })
  }

  atualizarContrato(contrato) {
    return this.instance.put('/contrato', contrato, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.session.token
    }}).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  obterContratoPorId(id) {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    const url = '/contrato/Detalhar/'.concat(id);
    return this.instance.get(url, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + this.session.token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    })
  }

  uploadArquivo(contratoId: number, arquivo: File) {
    this.session = this.util.getSession('usuario');
    const formData = new FormData();
    formData.append('arquivo', arquivo);
    return this.instance.post(`/contrato/UploadArquivo/${contratoId}`, formData, {
      headers: {
        'Authorization': 'Bearer ' + this.session.token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-CONTRATO] Erro no upload:', err);
      return err;
    });
  }

  downloadArquivo(contratoId: number) {
    this.session = this.util.getSession('usuario');
    return this.instance.get(`/contrato/DownloadArquivo/${contratoId}`, {
      headers: {
        'Authorization': 'Bearer ' + this.session.token
      },
      responseType: 'blob'
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-CONTRATO] Erro no download:', err);
      return err;
    });
  }

  removerArquivo(contratoId: number) {
    this.session = this.util.getSession('usuario');
    return this.instance.delete(`/contrato/RemoverArquivo/${contratoId}`, {
      headers: {
        'Authorization': 'Bearer ' + this.session.token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-CONTRATO] Erro ao remover:', err);
      return err;
    });
  }
}
