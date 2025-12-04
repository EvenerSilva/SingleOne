import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UsuarioApiService } from 'src/app/api/usuarios/usuario-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-esqueci-senha',
  templateUrl: './esqueci-senha.component.html',
  styleUrls: ['./esqueci-senha.component.scss']
})
export class EsqueciSenhaComponent implements OnInit {

  public usuario: any = {};
  public form: FormGroup;
  public enviando = false;

  constructor(private fb: FormBuilder, private util: UtilService, private api: UsuarioApiService) {
    this.form = this.fb.group({
      usuario: ['', [Validators.required, Validators.email]]
    })
  }

  ngOnInit(): void {
  }

  async entrar() {
    if (this.form.valid) {
      try {
        this.enviando = true;
        this.util.aguardar(true);
        const resultado = await this.api.recuperarPalavraChave(this.usuario.email);
        this.util.aguardar(false);
        this.enviando = false;
        
        this.util.exibirMensagemToast(
          "✅ E-mail de recuperação enviado com sucesso! Verifique sua caixa de entrada.", 
          8000
        );
        // Limpar formulário após sucesso
        this.form.reset();
        this.usuario = {};
        
      } catch (erro) {
        console.error('[ESQUECI-SENHA] Erro na recuperação:', erro);
        
        this.util.aguardar(false);
        this.enviando = false;
        
        let mensagemErro = "❌ Erro ao enviar e-mail de recuperação.";
        
        if (erro.response) {
          // Erro da API
          if (erro.response.status === 404) {
            mensagemErro = "❌ E-mail não encontrado no sistema.";
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
      this.util.exibirMensagemToast("❌ Por favor, preencha um e-mail válido.", 5000);
    }
  }
}
