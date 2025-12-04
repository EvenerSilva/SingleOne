import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { UtilService } from '../../shared/services/util.service';
import axios from 'axios';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ConfigApiService {

  protected instance = axios.create({
    baseURL: environment.apiUrl
  })

  protected instanceCep = axios.create({
    baseURL: "https://viacep.com.br/ws/"
  })

  constructor(private route: Router, protected util: UtilService) {
    this.instance.interceptors.response.use(response => {
        return response;
      }, error => {
        //if (error.response.status == 401) {
        //place your reentry code

        if(error.response && error.response.data == ""){
          this.util.exibirMensagemToast('Sessão expirada. Por favor, entre novamente', 5000);
          this.route.navigate(['/']);
        }
        else if(error.response){
          this.util.exibirMensagemToast('Usuário/senha inválido', 5000);
          this.route.navigate(['/']);
        }
        return Promise.reject(error);
      });
  }
}
