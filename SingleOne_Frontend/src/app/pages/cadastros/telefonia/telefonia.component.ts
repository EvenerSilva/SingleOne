import { Component, OnInit } from '@angular/core';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UtilService } from 'src/app/util/util.service';
import {saveAs as importedSaveAs} from "file-saver";

@Component({
  selector: 'app-telefonia',
  templateUrl: './telefonia.component.html',
  styleUrls: ['./telefonia.component.scss']
})
export class TelefoniaComponent implements OnInit {

  private session:any = {};
  constructor(private util: UtilService, private api: TelefoniaApiService) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
  }

  exportarExcel() {
    // this.api.exportarParaExcel(this.token);
    this.util.aguardar(true);
    this.api.exportarLinhasParaExcel(this.session.usuario.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200) {
        this.util.exibirMensagemToast('Falha de comunicação com o serviço', 5000);
      }
      else {
        const arr = this._base64ToArrayBuffer(res.data);
        const blob = new Blob([arr], { type: 'application/octet-stream' });

        importedSaveAs(blob, 'Recursos telecom.xlsx');
      }
    })
  }

  private _base64ToArrayBuffer(base64) {
    var binary_string = window.atob(base64);
    var len = binary_string.length;
    var bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
        bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes.buffer;
  }

}
