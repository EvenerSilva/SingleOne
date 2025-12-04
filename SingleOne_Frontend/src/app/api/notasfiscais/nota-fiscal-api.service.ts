import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

@Injectable({
  providedIn: 'root'
})
export class NotaFiscalApiService extends ConfigApiService {
  private session: any = {};

  uploadArquivo(notaFiscalId: number, arquivo: File) {
    this.session = this.util.getSession('usuario');
    const formData = new FormData();
    formData.append('arquivo', arquivo);
    return this.instance.post(`/notafiscal/UploadArquivo/${notaFiscalId}`, formData, {
      headers: {
        'Authorization': 'Bearer ' + this.session.token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-NOTAFISCAL] Erro no upload:', err);
      return err;
    });
  }

  downloadArquivo(notaFiscalId: number) {
    this.session = this.util.getSession('usuario');
    return this.instance.get(`/notafiscal/DownloadArquivo/${notaFiscalId}`, {
      headers: {
        'Authorization': 'Bearer ' + this.session.token
      },
      responseType: 'blob'
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-NOTAFISCAL] Erro no download:', err);
      return err;
    });
  }

  removerArquivo(notaFiscalId: number) {
    this.session = this.util.getSession('usuario');
    return this.instance.delete(`/notafiscal/RemoverArquivo/${notaFiscalId}`, {
      headers: {
        'Authorization': 'Bearer ' + this.session.token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-NOTAFISCAL] Erro ao remover:', err);
      return err;
    });
  }
}

