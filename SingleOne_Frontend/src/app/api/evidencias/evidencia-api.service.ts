import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

@Injectable({
  providedIn: 'root'
})
export class EvidenciaApiService extends ConfigApiService {

  /**
   * Upload de evidência de descarte
   */
  uploadEvidencia(equipamento: number, tipoProcesso: string, descricao: string, arquivo: File, token: string) {
    const formData = new FormData();
    formData.append('equipamento', equipamento.toString());
    formData.append('tipoProcesso', tipoProcesso);
    formData.append('descricao', descricao || '');
    formData.append('arquivo', arquivo);

    return this.instance.post('/DescarteEvidencia/UploadEvidencia', formData, {
      headers: {
        'Authorization': 'Bearer ' + token
        // Não definir Content-Type - o browser define automaticamente para multipart/form-data
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('❌ [API-EVIDENCIA] Erro no upload:', err);
      return err;
    });
  }

  /**
   * Listar evidências de um equipamento
   */
  listarEvidencias(equipamento: number, token: string) {
    return this.instance.get(`/DescarteEvidencia/ListarEvidencias/${equipamento}`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('❌ [API-EVIDENCIA] Erro ao listar:', err);
      return err;
    });
  }

  /**
   * Download de evidência
   */
  downloadEvidencia(id: number, token: string) {
    return this.instance.get(`/DescarteEvidencia/DownloadEvidencia/${id}`, {
      headers: {
        'Authorization': 'Bearer ' + token
      },
      responseType: 'blob' // Importante para download de arquivos
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('❌ [API-EVIDENCIA] Erro no download:', err);
      return err;
    });
  }

  /**
   * Excluir evidência
   */
  excluirEvidencia(id: number, token: string) {
    return this.instance.delete(`/DescarteEvidencia/ExcluirEvidencia/${id}`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('❌ [API-EVIDENCIA] Erro ao excluir:', err);
      return err;
    });
  }

  /**
   * Contar evidências por tipo de processo
   */
  contarEvidenciasPorTipo(equipamento: number, token: string) {
    return this.instance.get(`/DescarteEvidencia/ContarEvidenciasPorTipo/${equipamento}`, {
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
}

