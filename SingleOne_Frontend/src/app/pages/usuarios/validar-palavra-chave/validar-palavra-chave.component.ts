import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UsuarioApiService } from 'src/app/api/usuarios/usuario-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-validar-palavra-chave',
  templateUrl: './validar-palavra-chave.component.html',
  styleUrls: ['./validar-palavra-chave.component.scss']
})
export class ValidarPalavraChaveComponent implements OnInit {

  public usuario: any = {};
  public form: FormGroup;
  public enviando = false;

  constructor(
    private fb: FormBuilder, 
    private util: UtilService, 
    private api: UsuarioApiService, 
    private route: Router, 
    private ar: ActivatedRoute
  ) { 
    this.form = this.fb.group({
      usuario: ['', [Validators.required, Validators.email]],
      token: ['', Validators.required]
    })
  }

  ngOnInit(): void {
    this.ar.paramMap.subscribe(param => {
      const parametro = param.get('token');
      if (parametro != null) {
        this.usuario.palavracriptografada = parametro;
        this.form.patchValue({ token: parametro });
      }
    })
  }

  async salvar() {
    if (this.form.valid) {
      try {
        this.enviando = true;
        this.util.aguardar(true);
        const resultado = await this.api.recuperarSenha(this.usuario);
        this.util.aguardar(false);
        this.enviando = false;
        
        if (resultado.status != 200) {
          this.util.exibirMensagemToast('❌ Falha de comunicação com o serviço. Tente novamente.', 8000);
        } else {
          this.util.exibirMensagemToast(
            '✅ Requisição enviada com sucesso! Em breve você receberá um e-mail com uma nova senha de acesso.', 
            8000
          );
          // Aguardar um pouco antes de redirecionar
          setTimeout(() => {
            this.route.navigate(['/login']);
          }, 2000);
        }
        
      } catch (erro) {
        console.error('[VALIDAR-TOKEN] Erro na validação:', erro);
        
        this.util.aguardar(false);
        this.enviando = false;
        
        let mensagemErro = "❌ Erro ao validar token.";
        
        if (erro.response) {
          if (erro.response.status === 400) {
            mensagemErro = "❌ Dados inválidos. Verifique o e-mail e tente novamente.";
          } else if (erro.response.status === 404) {
            mensagemErro = "❌ Usuário não encontrado com os dados informados.";
          } else if (erro.response.status === 500) {
            mensagemErro = "❌ Erro interno do servidor. Tente novamente.";
          } else if (erro.response.data && erro.response.data.Mensagem) {
            mensagemErro = `❌ ${erro.response.data.Mensagem}`;
          }
        } else if (erro.message) {
          mensagemErro = `❌ ${erro.message}`;
        }
        
        this.util.exibirMensagemToast(mensagemErro, 8000);
      }
    } else {
      this.util.exibirMensagemToast("❌ Por favor, preencha todos os campos corretamente.", 5000);
    }
  }
}
