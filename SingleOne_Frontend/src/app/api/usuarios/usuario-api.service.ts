import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

@Injectable({
  providedIn: 'root'
})
export class UsuarioApiService extends ConfigApiService {

  public Salvar(usuario, token){
    var senha = usuario.senha;
    if(senha != null) {
      usuario.senha = btoa(usuario.senha);
    }
    return this.instance.post('usuario', usuario, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      usuario.senha = senha;
      return res;
    }).catch(err => {
      usuario.senha = senha;
      throw err; // Re-throw o erro para ser capturado pelo componente
    })
  }

  public entrar(usuario) {
    var senha = usuario.Senha;
    usuario.Senha = btoa(usuario.Senha);
    
    return this.instance.post('usuario/login', usuario).then(res => {
      usuario.Senha = senha;
      return res;
    }).catch(err => {
      usuario.Senha = senha;
      throw err;
    })
  }

  public recuperarPalavraChave(email) {
    return this.instance.get('usuario/RecuperarPalavraChave/' + email).then(res => {
      return res;
    }).catch(err => {
      throw err; // Re-throw o erro para ser capturado pelo componente
    })
  }

  public recuperarSenha(usr) {
    return this.instance.post('usuario/RecuperarSenha', usr).then(res => {
      return res;
    }).catch(err => {
      throw err; // Re-throw o erro para ser capturado pelo componente
    })
  }

  // listarUsuarios() {
  //   return this.instance.get('/usuario').then(res => {
  //     return res;
  //   }).catch(err => {
  //     return err;
  //   })
  // }

  buscarPorId(id, token) {
    return this.instance.get('usuario/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      throw err; // Re-throw o erro para ser capturado pelo componente
    }) 
  }

  listarUsuarios(pesquisa, cliente, token) {
    return this.instance.get('usuario/ListarUsuarios/' + pesquisa + '/' + cliente, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      throw err;
    })
  }

  excluir(id, token) {
    return this.instance.delete('usuario/ExcluirUsuario/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      throw err; // Re-throw o erro para ser capturado pelo componente
    })
  }

  desativarUsuario(id, token) {
    // ✅ NOTA: Usando o endpoint de exclusão existente para desativar usuário
    // No backend, este endpoint deve ser modificado para marcar como inativo ao invés de excluir
    return this.instance.delete('usuario/ExcluirUsuario/' + id, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      throw err; // Re-throw o erro para ser capturado pelo componente
    })
  }

  enableTwoFactor(userId: number, token: string) {
    return this.instance.post('usuario/EnableTwoFactor', { userId }, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      throw err; // Re-throw o erro para ser capturado pelo componente
    })
  }

  disableTwoFactor(userId: number, token: string) {
    return this.instance.post('usuario/DisableTwoFactor', { UserId: userId }, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + token
    }}).then(res => {
      return res;
    }).catch(err => {
      throw err;
    })
  }

  sendTwoFactorCode(request: { UserId: number, Email: string }) {
    return this.instance.post('usuario/SendTwoFactorCode', request).then(res => {
      return res.data;
    }).catch(err => {
      throw err;
    });
  }

  verifyTwoFactorCode(request: { UserId: number, Code: string, VerificationType: string }) {
    return this.instance.post('usuario/VerifyTwoFactor', request).then(res => {
      return res.data;
    }).catch(err => {
      throw err;
    });
  }
}
