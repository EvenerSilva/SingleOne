import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import * as moment from 'moment';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { ContratoApiService } from 'src/app/api/contratos/contrato-api.service';
import { UtilService } from 'src/app/util/util.service';
import { MatDialog } from '@angular/material/dialog';
import { MessageboxComponent } from '../../messagebox/messagebox.component';

@Component({
  selector: 'app-contrato',
  templateUrl: './contrato.component.html',
  styleUrls: ['./contrato.component.scss']
})
export class ContratoComponent implements OnInit {
  
  private session:any = {};
  public contrato:any = {};
  public fornecedores:any = [];
  public listaStatus:any = [];
  public form: FormGroup;
  public contratoId: string | null = null;

  // 🆕 Propriedades para o modal de fornecedor
  public mostrarModalFornecedor = false;
  public novoFornecedor = { nome: '', cnpj: '', telefone: '', email: '' };

  // 📎 Propriedades para anexo
  public arquivoAtual: any = null;
  public arquivoSelecionado: File | null = null;

  // 🆕 Propriedades para prazo do contrato
  public opcoesPrazo = [
    { valor: 12, label: '12 meses' },
    { valor: 24, label: '24 meses' },
    { valor: 36, label: '36 meses' },
    { valor: 48, label: '48 meses' },
    { valor: 60, label: '60 meses' },
    { valor: 'outro', label: 'Outro (data personalizada)' }
  ];
  public prazoSelecionado: number | string = 12;
  public mostrarDataPersonalizada = false;

  constructor(private fb: FormBuilder, 
              private util: UtilService, 
              private api: ContratoApiService,
              private apiConfig: ConfiguracoesApiService,
              private route: Router,
              private activatedRoute: ActivatedRoute,
              private dialog: MatDialog) {
    this.form = this.fb.group({
      fornecedor: ['', Validators.required],
      numero: ['', Validators.required],
      aditivo: [''], // Opcional
      descricao: [''], // Opcional
      dtInicioVigencia: ['', Validators.required],
      dtFinalVigencia: [''],
      valor: [''], // Opcional
      renovavel: [false],
      prazoContrato: [12] // Novo campo para prazo
    });
    this.form.get('dtInicioVigencia')!.valueChanges.subscribe(date => {
      if (date && !this.mostrarDataPersonalizada) {
        this.calcularDataFinal();
      }
    });

    // Listener para mudanças no prazo selecionado
    this.form.get('prazoContrato')!.valueChanges.subscribe(prazo => {
      this.prazoSelecionado = prazo;
      this.mostrarDataPersonalizada = (prazo === 'outro');
      
      if (!this.mostrarDataPersonalizada && this.form.get('dtInicioVigencia')?.value) {
        this.calcularDataFinal();
      }
    });
   }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.contrato.cliente = this.session.usuario.cliente;
    this.contrato.usuarioCriacao = this.session.usuario.id;
    this.util.aguardar(true);

    this.activatedRoute.paramMap.subscribe(params => {
      this.contratoId = params.get('id');
      if (this.contratoId) {
        this.carregarContrato(this.contratoId); // Carrega o contrato se o ID estiver presente
      } else {
        this.util.aguardar(false);
      }
    });
    
    this.apiConfig.listarFornecedores("null", this.session.usuario.cliente, this.session.token)
        .then(res => {
          this.fornecedores = res.data;
          if (!this.fornecedores || this.fornecedores.length === 0) {
            this.criarFornecedorTeste();
          } else {
            if (this.fornecedores.length > 0) {
              this.form.patchValue({ fornecedor: this.fornecedores[0].id });
            }
          }
          
          this.api.listarStatus().then(statusResp => {
            this.listaStatus = statusResp.data;
            this.util.aguardar(false);
          });
    });
  }

  /**
   * Calcula a data final baseada no prazo selecionado
   */
  calcularDataFinal() {
    const dataInicio = this.form.get('dtInicioVigencia')?.value;
    const prazo = this.form.get('prazoContrato')?.value;
    
    if (dataInicio && prazo && prazo !== 'outro') {
      const meses = parseInt(prazo.toString());
      const dataFinal = moment(dataInicio).add(meses, 'months').format('YYYY-MM-DD');
      this.form.get('dtFinalVigencia')!.setValue(dataFinal);
    }
  }

  /**
   * Alterna entre prazo pré-definido e data personalizada
   */
  alternarTipoPrazo() {
    this.mostrarDataPersonalizada = !this.mostrarDataPersonalizada;
    
    if (this.mostrarDataPersonalizada) {
      // Limpar data final para permitir entrada manual
      this.form.get('dtFinalVigencia')!.setValue('');
    } else {
      // Recalcular data final baseada no prazo selecionado
      this.calcularDataFinal();
    }
  }

  carregarContrato(id: string) {
    this.util.aguardar(true);
    this.api.obterContratoPorId(id).then(res => {
      if (res && res.data.isCompleted) {
        this.contrato = res.data.result;
        // Calcular prazo em meses baseado nas datas
        const dataInicio = moment(this.contrato.dtInicioVigencia);
        const dataFinal = moment(this.contrato.dtFinalVigencia);
        const mesesContrato = dataFinal.diff(dataInicio, 'months');
        
        // Verificar se o prazo está nas opções pré-definidas
        const prazoEncontrado = this.opcoesPrazo.find(opcao => 
          opcao.valor === mesesContrato && typeof opcao.valor === 'number'
        );
        const prazoParaForm = prazoEncontrado ? mesesContrato : 'outro';
        this.mostrarDataPersonalizada = !prazoEncontrado;

        // Atualiza os valores do formulário com o contrato carregado
        this.form.patchValue({
          fornecedor: this.contrato.fornecedorId,
          numero: this.contrato.numero,
          aditivo: this.contrato.aditivo,
          descricao: this.contrato.descricao,
          dtInicioVigencia: this.contrato.dtInicioVigencia,
          dtFinalVigencia: this.contrato.dtFinalVigencia,
          valor: this.contrato.valor,
          renovavel: this.contrato.renovavel || false,
          prazoContrato: prazoParaForm
        });
        
        // Carregar informações do arquivo anexado
        this.arquivoAtual = {
          nome: this.contrato.nomeArquivoOriginal,
          arquivo: this.contrato.arquivoContrato,
          dataUpload: this.contrato.dataUploadArquivo,
          temArquivo: this.contrato.temArquivo || !!this.contrato.arquivoContrato
        };
      } else {
        this.util.exibirMensagemToast('Contrato não encontrado.', 5000);
      }
      this.util.aguardar(false);
    }).catch(() => {
      this.util.exibirFalhaComunicacao();
      this.util.aguardar(false);
    });
  }

salvarContrato() {
    this.form.markAllAsTouched();
    
    if(this.form.valid){
      // ✅ CORREÇÃO: Verificar se há fornecedores antes de salvar
      if (!this.fornecedores || this.fornecedores.length === 0) {
        this.criarFornecedorTeste().then(() => {
          // Após criar o fornecedor, tentar salvar novamente
          setTimeout(() => this.salvarContrato(), 2000);
        });
        return;
      }

      this.util.aguardar(true);
      
      // Preparar dados do formulário
      const formValues = this.form.value;
      const session = this.util.getSession('usuario');
      
      // Estrutura completa com todos os campos obrigatórios
      const dadosParaSalvar = {
        cliente: session.usuario.cliente,
        fornecedorId: parseInt(formValues.fornecedor),
        numero: parseInt(formValues.numero),
        aditivo: formValues.aditivo ? parseInt(formValues.aditivo) : 0,
        descricao: formValues.descricao || '',
        dtInicioVigencia: formValues.dtInicioVigencia,
        dtFinalVigencia: formValues.dtFinalVigencia || (formValues.dtInicioVigencia ? moment(formValues.dtInicioVigencia).add(1, 'year').format('YYYY-MM-DD') : null),
        valor: parseFloat(formValues.valor) || 0,
        renovavel: Boolean(formValues.renovavel),
        usuarioCriacao: session.usuario.id
      };
      if (this.contratoId == undefined || this.contratoId == null) {
        this.api.criarNovoContrato(dadosParaSalvar).then(res => {
          this.util.aguardar(false);
          
          if(res.status !== undefined && res.status !== null && res.status === 200){
            // Verificar se o contrato foi realmente criado
            if(res.data && res.data.result && Object.keys(res.data.result).length > 0) {
              this.util.exibirMensagemToast('Contrato salvo com sucesso.', 5000);
              this.route.navigate(['/contratos']);
            } else {
              console.warn('[CONTRATO] Resposta 200 mas sem dados do contrato criado');
              console.warn('[CONTRATO] Estrutura da resposta:', res);
              console.warn('[CONTRATO] Backend adicionou a entidade, mas não retornou dados');
              console.warn('[CONTRATO] Problema no backend: status 5 indica erro na transação');
              console.warn('[CONTRATO] Backend adicionou a entidade ao contexto, mas não retornou dados');
              console.warn('[CONTRATO] Possível problema: transação não commitada ou erro na validação');
              console.warn('[CONTRATO] Backend adicionou a entidade ao contexto, mas não retornou dados');
              console.warn('[CONTRATO] Verificar se a transação está sendo commitada no backend');
              this.util.exibirMensagemToast('Contrato processado pelo backend, mas há problema na transação. Verifique a listagem.', 8000);
              this.route.navigate(['/contratos']);
            }
          }
          else if(res.response && (res.response.status == 400 || res.response.status == 404 || res.response.status == 409 || res.response.status == 422)) {
            const errorMessage = res.response.data?.message || res.response.data?.error || 'Erro ao salvar contrato';
            console.error('[CONTRATO] Erro específico:', errorMessage);
            this.util.exibirMensagemToast(errorMessage, 10000);
          }
          else  {
            console.error('[CONTRATO] Resposta inesperada:', res);
            this.util.exibirFalhaComunicacao();
          }
        }).catch(error => {
          console.error('[CONTRATO] Erro ao salvar contrato:', error);
          console.error('[CONTRATO] Erro response:', error.response);
          console.error('[CONTRATO] Erro data:', error.response?.data);
          this.util.aguardar(false);
          
          if (error.response?.data?.message) {
            this.util.exibirMensagemToast(error.response.data.message, 10000);
          } else {
            this.util.exibirFalhaComunicacao();
          }
        });
      }
      else {
        // ✅ CORREÇÃO: Incluir o ID do contrato nos dados para atualização
        const dadosParaAtualizar = {
          ...dadosParaSalvar,
          id: parseInt(this.contratoId!)
        };
        this.api.atualizarContrato(dadosParaAtualizar).then(res => {
          this.util.aguardar(false);
          if(res.status !== undefined && res.status !== null && res.status === 200){
            this.util.exibirMensagemToast('Contrato salvo com sucesso.', 5000);
            this.route.navigate(['/contratos']);          
          }
          else if(res.response && (res.response.status == 400 || res.response.status == 404 || res.response.status == 409 || res.response.status == 422)) {
            this.util.exibirMensagemToast(res.response.data.message, 10000);
          }
          else  {
            this.util.exibirFalhaComunicacao();
          }
        }).catch(error => {
          console.error('[CONTRATO] Erro ao atualizar contrato:', error);
          this.util.aguardar(false);
          this.util.exibirFalhaComunicacao();
        });
      }
    } else {
      this.util.exibirMensagemToast('Por favor, preencha todos os campos obrigatórios.', 5000);
    }
  }

  /**
   * Limpa o formulário
   */
  limparFormulario() {
    this.form.reset();
    this.contrato = {};
    this.contrato.cliente = this.session.usuario.cliente;
    this.contrato.usuarioCriacao = this.session.usuario.id;
    
    // Resetar configurações de prazo
    this.prazoSelecionado = 12;
    this.mostrarDataPersonalizada = false;
    this.form.patchValue({ prazoContrato: 12 });
  }

  /**
   * Abre o modal para novo fornecedor
   */
  abrirModalNovoFornecedor() {
    this.mostrarModalFornecedor = true;
    this.novoFornecedor = { nome: '', cnpj: '', telefone: '', email: '' };
  }

  /**
   * Fecha o modal de novo fornecedor
   */
  fecharModalFornecedor() {
    this.mostrarModalFornecedor = false;
  }

  /**
   * Salva um novo fornecedor
   */
  async salvarNovoFornecedor() {
    if (this.novoFornecedor.nome.trim() && this.novoFornecedor.cnpj.trim()) {
      
      // ✅ VALIDAÇÃO: Verificar se CNPJ já existe na lista local
      const cnpjExistente = this.fornecedores.find(f => 
        f.cnpj && f.cnpj.replace(/\D/g, '') === this.novoFornecedor.cnpj.replace(/\D/g, '')
      );
      if (cnpjExistente) {
        this.util.exibirMensagemToast('Este CNPJ já está cadastrado no sistema.', 8000);
        return;
      }
      
      try {
        this.util.aguardar(true);
        
        const fornecedor = {
          nome: this.novoFornecedor.nome.trim(),
          cnpj: this.novoFornecedor.cnpj.trim(),
          telefone: this.novoFornecedor.telefone.trim() || null,
          email: this.novoFornecedor.email.trim() || null,
          ativo: true,
          cliente: this.session.usuario.cliente
        };
        
        // Usar a API de fornecedores correta
        const response = await this.apiConfig.salvarFornecedor(fornecedor, this.session.token);
        
        if (response && response.data) {
          this.apiConfig.listarFornecedores("null", this.session.usuario.cliente, this.session.token)
            .then(res => {
              if (res && res.data) {
                this.fornecedores = res.data;
                this.form.patchValue({ fornecedor: response.data.id });
                
                this.fecharModalFornecedor();
                this.util.exibirMensagemToast('Fornecedor cadastrado com sucesso!', 3000);
              } else {
                console.error('[CONTRATO] ❌ Erro ao recarregar lista de fornecedores');
                this.util.exibirMensagemToast('Fornecedor salvo, mas erro ao atualizar lista.', 5000);
              }
            })
            .catch(error => {
              console.error('[CONTRATO] ❌ Erro ao recarregar fornecedores:', error);
              // Mesmo com erro, adicionar à lista local como fallback
              this.fornecedores.push(response.data);
              this.form.patchValue({ fornecedor: response.data.id });
              this.fecharModalFornecedor();
              this.util.exibirMensagemToast('Fornecedor cadastrado com sucesso!', 3000);
            });
        }
      } catch (error) {
        console.error('[CONTRATO] Erro ao salvar fornecedor:', error);
        console.error('[CONTRATO] Detalhes do erro:', error.response?.data);
        console.error('[CONTRATO] Status do erro:', error.response?.status);
        
        // Tratar erros específicos do backend
        if (error.response?.data?.message) {
          const errorMessage = error.response.data.message;
          
          // Verificar se é erro de CNPJ duplicado
          if (errorMessage.toLowerCase().includes('cnpj') && 
              (errorMessage.toLowerCase().includes('já existe') || 
               errorMessage.toLowerCase().includes('duplicado') ||
               errorMessage.toLowerCase().includes('cadastrado'))) {
            this.util.exibirMensagemToast('Este CNPJ já está cadastrado no sistema.', 8000);
          } else if (errorMessage.toLowerCase().includes('nome') && 
                     errorMessage.toLowerCase().includes('já existe')) {
            this.util.exibirMensagemToast('Já existe um fornecedor com este nome.', 8000);
          } else {
            // Outros erros específicos do backend
            this.util.exibirMensagemToast(errorMessage, 8000);
          }
        } else if (error.response?.status === 400) {
          this.util.exibirMensagemToast('Dados inválidos. Verifique os campos preenchidos.', 8000);
        } else if (error.response?.status === 409) {
          this.util.exibirMensagemToast('Conflito: Este fornecedor já existe no sistema.', 8000);
        } else if (error.response?.status === 422) {
          this.util.exibirMensagemToast('Erro de validação. Verifique os dados informados.', 8000);
        } else {
          // Erro genérico de comunicação
          this.util.exibirFalhaComunicacao();
        }
        
        // Limpar o modal em caso de erro para permitir nova tentativa
        this.novoFornecedor = { nome: '', cnpj: '', telefone: '', email: '' };
      } finally {
        this.util.aguardar(false);
      }
    }
  }

  // ✅ CORREÇÃO: Método para criar fornecedor de teste
  async criarFornecedorTeste() {
    try {
      const fornecedorTeste = {
        nome: 'Fornecedor Teste',
        cnpj: '12345678000199',
        email: 'teste@fornecedor.com',
        telefone: '(11) 99999-9999',
        ativo: true,
        cliente: this.session.usuario.cliente
      };
      const response = await this.apiConfig.salvarFornecedor(fornecedorTeste, this.session.token);
      
      if (response && response.data) {
        this.util.exibirMensagemToast('Fornecedor de teste criado automaticamente.', 5000);
        
        // Recarrega a lista de fornecedores
        this.apiConfig.listarFornecedores("null", this.session.usuario.cliente, this.session.token)
          .then(res => {
            if (res && res.data) {
              this.fornecedores = res.data;
              if (this.fornecedores.length > 0) {
                this.form.patchValue({ fornecedor: this.fornecedores[0].id });
              }
            } else {
              console.error('[CONTRATO] ❌ Erro ao recarregar lista de fornecedores');
            }
          })
          .catch(error => {
            console.error('[CONTRATO] ❌ Erro ao recarregar fornecedores:', error);
          });
      } else {
        this.util.exibirMensagemToast('Erro ao criar fornecedor de teste.', 5000);
      }
    } catch (error) {
      console.error('[CONTRATO] ❌ Erro ao criar fornecedor de teste:', error);
      console.error('[CONTRATO] Detalhes do erro:', error.response?.data);
      
      // Tratar erros específicos
      if (error.response?.data?.message) {
        const errorMessage = error.response.data.message;
        if (errorMessage.toLowerCase().includes('cnpj') && 
            errorMessage.toLowerCase().includes('já existe')) {
          this.util.exibirMensagemToast('Fornecedor de teste já existe. Usando fornecedor existente.', 5000);
        } else {
          this.util.exibirMensagemToast(`Erro ao criar fornecedor de teste: ${errorMessage}`, 8000);
        }
      } else {
        this.util.exibirMensagemToast('Erro ao criar fornecedor de teste.', 5000);
      }
    }
  }

  /**
   * Cancela o cadastro/edição do contrato e volta para a lista
   */
  cancelarContrato() {
    // Verificar se há alterações não salvas
    if (this.form.dirty) {
      // Usar modal padronizado do sistema
      const dialogRef = this.dialog.open(MessageboxComponent, {
        data: {
          mensagem: `
            <div style="text-align: center;">
              <i class="cil-warning" style="font-size: 48px; color: #f59e0b; margin-bottom: 16px;"></i>
              <h3 style="margin: 0 0 12px 0; color: #080039; font-size: 1.25rem;">Alterações Não Salvas</h3>
              <p style="margin: 0; color: #64748b; font-size: 0.95rem; line-height: 1.6;">
                Você tem alterações não salvas no formulário.<br>
                Deseja realmente cancelar e perder essas alterações?
              </p>
            </div>
          `,
          exibeCancelar: true
        },
        width: '450px',
        disableClose: false
      });

      dialogRef.afterClosed().subscribe(resultado => {
        if (resultado === true) {
          // Usuário confirmou o cancelamento
          this.route.navigate(['/contratos']);
        }
      });
    } else {
      // Não há alterações, voltar direto
      this.route.navigate(['/contratos']);
    }
  }

  /**
   * Seleciona arquivo para upload
   */
  selecionarArquivo(event: any) {
    const arquivo: File = event.target.files[0];
    
    if (!arquivo) {
      return;
    }

    // Validar tamanho (10MB)
    if (arquivo.size > 10 * 1024 * 1024) {
      this.util.exibirMensagemToast('Arquivo muito grande! Tamanho máximo: 10MB', 5000);
      event.target.value = '';
      return;
    }

    // Validar extensão
    const extensoesPermitidas = ['.pdf', '.doc', '.docx'];
    const nomeArquivo = arquivo.name.toLowerCase();
    const extensaoValida = extensoesPermitidas.some(ext => nomeArquivo.endsWith(ext));
    
    if (!extensaoValida) {
      this.util.exibirMensagemToast('Tipo de arquivo não permitido! Use apenas PDF, DOC ou DOCX', 5000);
      event.target.value = '';
      return;
    }

    this.arquivoSelecionado = arquivo;
  }

  /**
   * Faz upload do arquivo selecionado
   */
  uploadArquivo() {
    if (!this.contratoId) {
      this.util.exibirMensagemToast('Salve o contrato antes de anexar arquivos', 5000);
      return;
    }

    if (!this.arquivoSelecionado) {
      this.util.exibirMensagemToast('Selecione um arquivo primeiro', 5000);
      return;
    }

    this.util.aguardar(true);
    this.api.uploadArquivo(parseInt(this.contratoId), this.arquivoSelecionado).then(res => {
      this.util.aguardar(false);
      
      if (res.status === 200) {
        this.util.exibirMensagemToast('Arquivo anexado com sucesso!', 3000);
        this.arquivoSelecionado = null;
        // Recarregar o contrato para atualizar as informações do arquivo
        this.carregarContrato(this.contratoId!);
      } else {
        console.error('Erro ao fazer upload:', res);
        this.util.exibirMensagemToast(res.data?.message || 'Erro ao anexar arquivo', 5000);
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('Erro no upload:', err);
      this.util.exibirMensagemToast('Erro ao anexar arquivo', 5000);
    });
  }

  /**
   * Faz download do arquivo anexado
   */
  downloadArquivo() {
    if (!this.contratoId || !this.arquivoAtual?.temArquivo) {
      return;
    }
    this.util.aguardar(true);

    this.api.downloadArquivo(parseInt(this.contratoId)).then(res => {
      this.util.aguardar(false);
      
      if (res.status === 200) {
        // Criar link temporário para download
        const blob = new Blob([res.data], { type: res.headers['content-type'] });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = this.arquivoAtual.nome;
        link.click();
        window.URL.revokeObjectURL(url);
        
        this.util.exibirMensagemToast('Download concluído!', 3000);
      } else {
        console.error('Erro ao baixar arquivo:', res);
        this.util.exibirMensagemToast('Erro ao baixar arquivo', 5000);
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('Erro no download:', err);
      this.util.exibirMensagemToast('Erro ao baixar arquivo', 5000);
    });
  }

  /**
   * Remove o arquivo anexado
   */
  removerArquivo() {
    if (!this.contratoId || !this.arquivoAtual?.temArquivo) {
      return;
    }

    // Abrir modal de confirmação
    const dialogRef = this.dialog.open(MessageboxComponent, {
      data: {
        mensagem: '<strong>Deseja realmente remover o arquivo anexado?</strong><br><br>Esta ação não poderá ser desfeita e o arquivo será permanentemente excluído do sistema.',
        exibeCancelar: true
      },
      width: '420px',
      disableClose: false
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado === true) {
        this.util.aguardar(true);

        this.api.removerArquivo(parseInt(this.contratoId!)).then(res => {
          this.util.aguardar(false);
          
          if (res.status === 200) {
            this.util.exibirMensagemToast('Arquivo removido com sucesso!', 3000);
            // Limpar informações do arquivo
            this.arquivoAtual = null;
            // Recarregar o contrato
            this.carregarContrato(this.contratoId!);
          } else {
            console.error('Erro ao remover arquivo:', res);
            this.util.exibirMensagemToast(res.data?.message || 'Erro ao remover arquivo', 5000);
          }
        }).catch(err => {
          this.util.aguardar(false);
          console.error('Erro ao remover:', err);
          this.util.exibirMensagemToast('Erro ao remover arquivo', 5000);
        });
      }
    });
  }

  /**
   * Cancela o arquivo selecionado
   */
  cancelarArquivoSelecionado() {
    this.arquivoSelecionado = null;
  }

}
