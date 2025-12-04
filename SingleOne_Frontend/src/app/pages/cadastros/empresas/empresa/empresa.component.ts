import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';
import { CnpjValidatorService } from 'src/app/util/cnpj-validator.service';

@Component({
  selector: 'app-empresa',
  templateUrl: './empresa.component.html',
  styleUrls: ['./empresa.component.scss']
})
export class EmpresaComponent implements OnInit {

  private session:any = {};
  public empresa:any = {
    filiais: [],
    localizacoes: []
  };
  public form: FormGroup;
  public cep:any = {};
  public localidadesDisponiveis: any[] = [];

constructor(
    private fb: FormBuilder, 
    private util: UtilService, 
    private api: ConfiguracoesApiService,
    private ar: ActivatedRoute, 
    private route: Router,
    private cnpjValidator: CnpjValidatorService
  ) {
      this.form = this.fb.group({
        razaosocial: ['', Validators.required],
        cnpj: ['', Validators.required],
        localidade_id: ['']
      })
      
      // Sincronizar mudanças do formulário com o objeto empresa
      this.form.valueChanges.subscribe(values => {
        this.empresa.razaosocial = values.razaosocial;
        this.empresa.cnpj = values.cnpj;
        this.empresa.localidade_id = values.localidade_id;
      });

      // Validação em tempo real do CNPJ
      this.form.get('cnpj')?.valueChanges.subscribe(cnpj => {
        if (cnpj && cnpj.length >= 14) {
          const isValid = this.cnpjValidator.isValid(cnpj);
          const cnpjControl = this.form.get('cnpj');
          
          if (!isValid) {
            cnpjControl?.setErrors({ 'cnpjInvalid': true });
          } else {
            cnpjControl?.setErrors(null);
          }
        }
      });
    }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.empresa.cliente = this.session.usuario.cliente;
    
    // Carregar localidades disponíveis
    this.carregarLocalidadesDisponiveis();
    
    this.ar.paramMap.subscribe(param => {
      var parametro = param.get('id');
      if(parametro != null) {
        this.empresa = JSON.parse(atob(parametro));
        this.form.patchValue({
          razaosocial: this.empresa.razaosocial || this.empresa.nome || '',
          cnpj: this.empresa.cnpj || '',
          localidade_id: this.empresa.localidade_id || this.empresa.localidadeId || ''
        });
      }
    })
  }

  salvar() {
    const cnpj = this.form.get('cnpj')?.value;
    if (cnpj && !this.cnpjValidator.isValid(cnpj)) {
      const errorMessage = this.cnpjValidator.getErrorMessage(cnpj);
      this.util.exibirMensagemToast(`CNPJ inválido: ${errorMessage}`, 5000);
      return;
    }
    
    if(this.form.valid) {
      this.util.aguardar(true);
      
      // Preparar dados para salvar usando os valores do formulário
      const formValues = this.form.value;
      const dadosParaSalvar = {
        ...this.empresa,
        nome: formValues.razaosocial,
        cnpj: formValues.cnpj,
        localidade_id: formValues.localidade_id ? parseInt(formValues.localidade_id) : null,
        cliente: this.empresa.cliente
      };
      if (dadosParaSalvar.localidade_id) {
      } else {
      }
      
      this.api.salvarEmpresa(dadosParaSalvar, this.session.token).then(res => {
        this.util.aguardar(false);
        
        // Se chegou aqui, é sucesso (status 200)
        if (res.data && res.data.Mensagem) {
          this.util.exibirMensagemToast(res.data.Mensagem, 5000);
        } else {
          this.util.exibirMensagemToast('Empresa salva com sucesso!', 5000);
        }
        
        this.route.navigate(['/empresas']);
      }).catch(error => {
        console.error('[EMPRESA] ❌ Erro ao salvar:', error);
        console.error('[EMPRESA] ❌ Erro completo:', error.response || error);
        console.error('[EMPRESA] ❌ Status do erro:', error.response?.status);
        console.error('[EMPRESA] ❌ Mensagem do erro:', error.response?.data);
        
        this.util.aguardar(false);
        
        // Verificar se é erro 400 (CNPJ inválido ou duplicado)
        if (error.response?.status === 400) {
          if (error.response.data && error.response.data.Tipo) {
            this.exibirMensagemErroEspecifica(error.response.data);
          } else {
            this.util.exibirMensagemToast('Erro de validação. Verifique os dados informados.', 5000);
          }
        }
        // Verificar se é erro 500 (problema no backend)
        else if (error.response?.status === 500) {
          this.util.exibirMensagemToast('Erro interno do servidor. Verifique o backend.', 5000);
        } else {
          this.util.exibirFalhaComunicacao();
        }
      });
    } else {
      // Verificar se há CNPJ duplicado mesmo com formulário inválido
      if (cnpj) {
        this.verificarCnpjDuplicado(cnpj);
      } else {
        this.util.exibirMensagemToast('Formulário inválido. Verifique os campos obrigatórios.', 5000);
      }
    }
  }

  // Método para carregar localidades disponíveis
  carregarLocalidadesDisponiveis() {
    this.api.listarLocalidades(this.empresa.cliente, this.session.token).then(res => {
      if (res.status === 200 && res.data) {
        // Filtrar apenas localizações ativas e remover duplicatas por ID e descrição
        const localizacoesAtivas = res.data.filter((loc: any) => loc.ativo);
        
        // Remover duplicatas usando Map - primeiro por ID, depois por descrição
        const localizacoesUnicasPorId = new Map();
        localizacoesAtivas.forEach((loc: any) => {
          if (!localizacoesUnicasPorId.has(loc.id)) {
            localizacoesUnicasPorId.set(loc.id, loc);
          }
        });
        
        // Remover duplicatas por descrição (caso haja IDs diferentes com mesma descrição)
        const localizacoesUnicasPorDescricao = new Map();
        Array.from(localizacoesUnicasPorId.values()).forEach((loc: any) => {
          const descricaoNormalizada = (loc.descricao || '').trim().toLowerCase();
          if (!localizacoesUnicasPorDescricao.has(descricaoNormalizada)) {
            localizacoesUnicasPorDescricao.set(descricaoNormalizada, loc);
          }
        });
        
        // Converter Map para Array e ordenar por descrição
        this.localidadesDisponiveis = Array.from(localizacoesUnicasPorDescricao.values())
          .sort((a: any, b: any) => {
            const descA = (a.descricao || '').toLowerCase();
            const descB = (b.descricao || '').toLowerCase();
            return descA.localeCompare(descB);
          });
      } else {
        this.localidadesDisponiveis = [];
      }
    }).catch(err => {
      console.error('[EMPRESA] ❌ Erro ao carregar localidades:', err);
      this.localidadesDisponiveis = [];
    });
  }

// Método para formatar a exibição da localidade
  formatarLocalidade(localidade: any): string {
    let resultado = localidade.descricao || '';
    
    // Adicionar cidade apenas se for válida (não for número ou coordenada)
    if (localidade.cidade && 
        localidade.cidade.trim() !== '' && 
        !this.isNumeroOuCoordenada(localidade.cidade)) {
      resultado += ` - ${localidade.cidade}`;
    }
    
    // Adicionar estado apenas se for válido (não for número ou coordenada)
    if (localidade.estado && 
        localidade.estado.trim() !== '' && 
        !this.isNumeroOuCoordenada(localidade.estado)) {
      resultado += resultado.includes(' - ') ? `, ${localidade.estado}` : ` - ${localidade.estado}`;
    }
    
    return resultado;
  }

  // Método para verificar se um valor é número ou coordenada
  private isNumeroOuCoordenada(valor: string): boolean {
    if (!valor) return false;
    
    // Verificar se é apenas números, vírgulas e pontos
    const numeroRegex = /^[\d.,]+$/;
    if (numeroRegex.test(valor)) {
      return true;
    }
    
    // Verificar se é coordenada (formato 90,9 ou 90.9)
    const coordenadaRegex = /^\d+[,.]\d+$/;
    if (coordenadaRegex.test(valor)) {
      return true;
    }
    
    return false;
  }

  // Método para obter nome da localidade por ID
  getLocalidadeNome(localidadeId: number): string {
    const localidade = this.localidadesDisponiveis.find(loc => loc.id === localidadeId);
    if (localidade) {
      return this.formatarLocalidade(localidade);
    }
    return 'Localidade não encontrada';
  }

  // Método para tratar mudança de localidade
  onLocalidadeChange(event: any) {
    const localidadeId = event.target.value;
    if (localidadeId && localidadeId !== '') {
      const localidade = this.localidadesDisponiveis.find(loc => loc.id == localidadeId);
      if (localidade) {
        this.empresa.localidade_id = parseInt(localidadeId);
      } else {
      }
    } else {
      // Se nenhuma localidade selecionada, limpar o campo
      this.empresa.localidade_id = null;
    }
  }

  // Método para limpar todo o formulário
  limparFormulario() {
    this.empresa = {
      cliente: this.session.usuario.cliente
    };
    this.form.reset();
  }

  /**
   * Obtém a mensagem de erro para o campo CNPJ
   */
  getCnpjErrorMessage(): string {
    const cnpj = this.form.get('cnpj')?.value;
    if (cnpj) {
      return this.cnpjValidator.getErrorMessage(cnpj) || 'CNPJ inválido';
    }
    return 'CNPJ é obrigatório';
  }

  /**
   * Verifica se o campo CNPJ tem erro de required
   */
  hasCnpjRequiredError(): boolean {
    const cnpjControl = this.form.get('cnpj');
    return cnpjControl?.errors?.['required'] && cnpjControl?.touched || false;
  }

  /**
   * Verifica se o campo CNPJ tem erro de CNPJ inválido
   */
  hasCnpjInvalidError(): boolean {
    const cnpjControl = this.form.get('cnpj');
    return cnpjControl?.errors?.['cnpjInvalid'] && cnpjControl?.touched || false;
  }

  /**
   * Verifica se o campo CNPJ tem algum erro
   */
  hasCnpjError(): boolean {
    return this.hasCnpjRequiredError() || this.hasCnpjInvalidError();
  }

  /**
   * Verifica se o CNPJ está duplicado chamando a API
   */
  verificarCnpjDuplicado(cnpj: string): void {
    if (!cnpj || !this.empresa.cliente) return;
    const dadosTemp = {
      id: 0, // Nova empresa
      cnpj: cnpj,
      cliente: this.empresa.cliente,
      nome: 'Verificação temporária'
    };
    this.api.salvarEmpresa(dadosTemp, this.session.token).then(res => {
      // Se chegou aqui, não é duplicado (status 200)
    }).catch(error => {
      if (error.response?.status === 400) {
        if (error.response.data?.Tipo === 'CNPJ_DUPLICADO') {
          this.exibirMensagemErroEspecifica(error.response.data);
        } else {
          if (error.response.data && error.response.data.Mensagem) {
            this.util.exibirMensagemToast(error.response.data.Mensagem, 8000);
          }
        }
      } else {
        this.util.exibirFalhaComunicacao();
      }
    });
  }

  /**
   * Exibe mensagem de erro específica baseada no tipo de erro
   */
  exibirMensagemErroEspecifica(data: any): void {
    if (!data || !data.Tipo) {
      this.util.exibirFalhaComunicacao();
      return;
    }

    let mensagem = data.Mensagem || 'Erro desconhecido';
    let duracao = 10000; // 10 segundos para mensagens de erro

    // Log detalhado para debug
    switch (data.Tipo) {
      case 'CNPJ_INVALIDO':
        mensagem = `❌ ${mensagem}`;
        if (data.Sugestao) {
          mensagem += `\n\n💡 ${data.Sugestao}`;
        }
        if (data.CNPJ_Informado) {
          mensagem += `\n\n📝 CNPJ informado: ${data.CNPJ_Informado}`;
        }
        break;
      
      case 'CNPJ_DUPLICADO':
        mensagem = `⚠️ ${mensagem}`;
        if (data.Sugestao) {
          mensagem += `\n\n💡 ${data.Sugestao}`;
        }
        break;
      
      default:
        mensagem = `❌ ${mensagem}`;
        break;
    }

    // Exibir mensagem principal
    this.util.exibirMensagemToast(mensagem, duracao);
    
    // Se houver sugestão, exibir separadamente para melhor visibilidade
    if (data.Sugestao && data.Tipo !== 'CNPJ_INVALIDO') {
      setTimeout(() => {
        this.util.exibirMensagemToast(`💡 ${data.Sugestao}`, 8000);
      }, 2000);
    }
  }
}
