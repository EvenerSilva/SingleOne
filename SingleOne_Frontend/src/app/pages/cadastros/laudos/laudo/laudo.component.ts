import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { UsuarioApiService } from 'src/app/api/usuarios/usuario-api.service';
import { ConfirmacaoComponent } from 'src/app/pages/confirmacao/confirmacao.component';
import { ModalSerialPatrimonioComponent } from 'src/app/pages/equipamentos/modal-serial-patrimonio/modal-serial-patrimonio.component';
import { UtilService } from 'src/app/shared/services/util.service';

// Interface para resposta da API de upload
interface UploadResponse {
  status: number;
  data: any;
  error?: boolean;
  mensagem?: string;
}

@Component({
  selector: 'app-laudo',
  templateUrl: './laudo.component.html',
  styleUrls: ['./laudo.component.scss']
})
export class LaudoComponent implements OnInit {

  // 🆕 PROPRIEDADES PARA O NOVO PADRÃO
  @Input() laudo: any = {};
  @Input() modo: 'criar' | 'editar' = 'criar';
  @Output() laudoSalvo = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  private session:any = {};
  public recursos:any = [];
  public tecnicos:any = [];
  public form: FormGroup;
  public titulo = '';
  public recursosControl = new FormControl();
  public resultado: Observable<any>;
  public empresas:any = [];
  public centros:any = [];
  public mostrarDropdown: boolean = false;
  public recursoSelecionadoTexto: string = '';
  public equipamentoSelecionado: any = null;
  private ignorarProximaBusca: boolean = false;

  // 📸 PROPRIEDADES PARA EVIDÊNCIAS
  public evidencias: any[] = [];
  public dragOver: boolean = false;
  public uploadProgress: number = 0;
  private dragIndex: number = -1;

  constructor(private fb: FormBuilder, private util: UtilService, private api: EquipamentoApiService,
    private apiUsu: UsuarioApiService, private apiCad: ConfiguracoesApiService, private ar: ActivatedRoute, private route: Router,
    private dialog: MatDialog) {
      this.form = this.fb.group({
        recurso: ['', Validators.required],
        tecnico: ['', Validators.required],
        descricao: ['', [Validators.required, Validators.maxLength(1000)]],
        // empresa: ['', Validators.required],
        // centrocusto: ['', Validators.required]
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    if (!this.session || !this.session.usuario) {
      console.error('[LAUDO] Sessão inválida ou usuário não encontrado');
      this.util.exibirMensagemToast('Erro: Sessão inválida. Faça login novamente.', 5000);
      return;
    }
    if (!this.laudo || Object.keys(this.laudo).length === 0) {
      this.laudo = {};
    } else {
    }
    
    this.laudo.cliente = this.session.usuario.cliente;
    this.laudo.usuario = this.session.usuario.id;
    this.resultado = this.recursosControl.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();

    // Carregar técnicos
    this.carregarTecnicos();
    
            // Definir título baseado no modo
        if (this.modo === 'editar') {
          this.titulo = 'Editar Sinistro';
          // Carregar evidências existentes se for modo de edição
          this.carregarEvidenciasExistentes();
        } else {
          this.titulo = 'Novo Sinistro';
        }
  }

  // 🆕 MÉTODO PARA CARREGAR TÉCNICOS
  carregarTecnicos() {
    this.carregando = true;
    
    this.apiUsu.listarUsuarios("null", this.laudo.cliente, this.session.token).then(res => {
      this.carregando = false;
      if (res.status === 200 && res.data) {
        this.tecnicos = res.data;
        if (this.tecnicos.length > 0) {
        }
      } else {
        console.error('[LAUDO] Erro ao carregar técnicos:', res);
        this.tecnicos = [];
      }
    }).catch(error => {
      this.carregando = false;
      console.error('[LAUDO] Erro na API técnicos:', error);
      this.tecnicos = [];
    });
  }

  buscar(valor) {
    // Se devemos ignorar esta busca (equipamento já selecionado)
    if (this.ignorarProximaBusca) {
      this.ignorarProximaBusca = false;
      return;
    }

    // Se o valor é igual ao texto do equipamento selecionado, não buscar
    if (this.equipamentoSelecionado && valor === this.recursoSelecionadoTexto) {
      return;
    }

    if (valor && valor.trim() !== '') {
      this.carregando = true;
      this.api.listarEquipamentoDisponivelParaLaudos(valor, this.laudo.cliente, this.session.token).then(res => {
        this.carregando = false;
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          // Filtrar apenas equipamentos com status válido para laudo (2=Devolvido, 7=Requisitado)
          this.recursos = (res.data || []).filter((recurso: any) => 
            recurso.equipamentostatusid === 2 || recurso.equipamentostatusid === 7
          );
          this.mostrarDropdown = this.recursos.length > 0;
          
          if (this.recursos.length === 0 && !this.equipamentoSelecionado) {
            this.util.exibirMensagemToast('Nenhum equipamento disponível para laudo encontrado.', 3000);
          }
        }
      }).catch(error => {
        this.carregando = false;
        console.error('[LAUDO] Erro ao buscar recursos:', error);
        this.recursos = [];
        this.mostrarDropdown = false;
      });
    } else {
      // Se o campo está vazio, limpar seleção
      this.equipamentoSelecionado = null;
      this.form.get('recurso')?.setValue('');
      this.laudo.equipamento = null;
      this.recursos = [];
      this.mostrarDropdown = false;
    }
  }

  selecionarRecurso(recurso: any) {
    // Validar se o equipamento tem status válido para laudo
    if (recurso.equipamentostatusid !== 2 && recurso.equipamentostatusid !== 7) {
      this.util.exibirMensagemToast('Este equipamento não pode receber laudo. Status inválido.', 5000);
      return;
    }
    
    // Salvar o equipamento selecionado
    this.equipamentoSelecionado = recurso;
    
    // Definir o valor no formulário
    this.form.get('recurso')?.setValue(recurso.id);
    this.laudo.equipamento = recurso.id;
    
    // Mostrar o texto selecionado no campo
    this.recursoSelecionadoTexto = `${recurso.tipoequipamento} ${recurso.fabricante} ${recurso.modelo}`;
    
    // Marcar para ignorar a próxima busca (causada pelo setValue)
    this.ignorarProximaBusca = true;
    this.recursosControl.setValue(this.recursoSelecionadoTexto);
    
    // Fechar dropdown
    this.mostrarDropdown = false;
  }

  onRecursoFocus() {
    if (this.recursos.length > 0) {
      this.mostrarDropdown = true;
    }
  }

  onRecursoBlur() {
    // Pequeno delay para permitir o click no item
    setTimeout(() => {
      this.mostrarDropdown = false;
    }, 200);
  }

salvar() {
    if(this.form.valid) {
      // Verificar se há recursos selecionados e se têm empresa e centro de custo
      const recursoId = this.form.get('recurso')?.value;
      if(recursoId && this.equipamentoSelecionado) {
        const recursoSelecionado = this.equipamentoSelecionado;
        
        // Validar se o equipamento tem status válido para laudo
        if (recursoSelecionado.equipamentostatusid !== 2 && recursoSelecionado.equipamentostatusid !== 7) {
          this.util.exibirMensagemToast('Este equipamento não pode receber laudo. Status inválido.', 5000);
          return;
        }
        
        if(recursoSelecionado && (recursoSelecionado.empresaid == null || recursoSelecionado.centrocustoid == null)) {
          const modalSerialPatrimonio = this.dialog.open(ModalSerialPatrimonioComponent, {
            width: '500px',
            data: {
              recurso: recursoSelecionado
            }
          });

          modalSerialPatrimonio.afterClosed().subscribe(r => {
            if(r == false) {
              this.util.exibirMensagemPopUp('Para criar um sinistro para este recurso, você DEVE informar a qual empresa e centro de custo ele pertence.', false).then(res => {})
            }
            else {
              this.carregando = true;
              this.apiCad.salvarLaudo(this.laudo, this.session.token).then(res => {
                this.carregando = false;
                if(res.status != 200) {
                  this.util.exibirFalhaComunicacao();
                }
                else {
                  // Atualizar o ID do laudo com o retornado pelo backend
                  const laudoId = res.data?.Id || res.data?.id;
                  
                  if (laudoId) {
                    this.laudo.id = laudoId;
                    this.uploadEvidenciasPendentes();
                  } else {
                    console.error('[LAUDO] Erro: ID do laudo não retornado pelo backend');
                    console.error('[LAUDO] Estrutura da resposta:', res.data);
                  }
                  
                  this.util.exibirMensagemToast('Laudo salvo com sucesso.', 5000);
                  // Emitir evento para o componente pai
                  this.laudoSalvo.emit(res.data);
                }
              })
            }
          })
        }
        else {
          // Recurso já tem empresa e centro de custo, salvar diretamente
          this.carregando = true;
          this.apiCad.salvarLaudo(this.laudo, this.session.token).then(res => {
            this.carregando = false;
            if(res.status != 200) {
              this.util.exibirFalhaComunicacao();
            }
            else {
              // Atualizar o ID do laudo com o retornado pelo backend
              const laudoId = res.data?.Id || res.data?.id;
              
              if (laudoId) {
                this.laudo.id = laudoId;
                this.uploadEvidenciasPendentes();
              } else {
                console.error('[LAUDO] Erro: ID do laudo não retornado pelo backend');
                console.error('[LAUDO] Estrutura da resposta:', res.data);
              }
              
              this.util.exibirMensagemToast('Laudo salvo com sucesso.', 5000);
              // Emitir evento para o componente pai
              this.laudoSalvo.emit(res.data);
            }
          })
        }
      } else {
        this.util.exibirMensagemToast('Por favor, selecione um equipamento válido para criar o laudo.', 5000);
      }
    }
    // if(this.form.valid) {
    //   this.util.aguardar(true);
    //   this.api.salvarEmpresa(this.laudo, this.session.token).then(res => {
    //     this.util.aguardar(false);
    //     if(res.status != 200) {
    //       this.util.exibirFalhaComunicacao();
    //     }
    //     else {
    //       this.util.exibirMensagemToast('Empresa salva com sucesso!', 5000);
    //       this.route.navigate(['/empresas']);
    //     }
    //   })
    // }
  }

  // 🆕 MÉTODOS PARA O NOVO PADRÃO
  getTitulo(): string {
    return this.titulo || 'Novo Sinistro';
  }

  getBotaoTexto(): string {
    return this.carregando ? 'Salvando...' : 'Salvar Laudo';
  }

  cancelar(): void {
    // Emitir evento para o componente pai fechar o modal
    this.cancelado.emit();
  }

  // Propriedade para controlar estado de loading
  private _carregando: boolean = false;
  
  get carregando(): boolean {
    return this._carregando;
  }
  
  set carregando(value: boolean) {
    this._carregando = value;
    this.util.aguardar(value);
  }

  // 📸 MÉTODOS PARA EVIDÊNCIAS FOTOGRÁFICAS
  onFileSelected(event: any): void {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.processarArquivos(files);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.processarArquivos(files);
    }
  }

  private processarArquivos(files: FileList): void {
    const arquivosPermitidos = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/bmp'];
    const tamanhoMaximo = 10 * 1024 * 1024; // 10MB

    for (let i = 0; i < files.length; i++) {
      if (this.evidencias.length >= 6) {
        this.util.exibirMensagemToast('Máximo de 6 evidências permitidas', 3000);
        break;
      }

      const arquivo = files[i];

      // Validar tipo
      if (!arquivosPermitidos.includes(arquivo.type)) {
        this.util.exibirMensagemToast(`Arquivo ${arquivo.name}: tipo não permitido`, 3000);
        continue;
      }

      // Validar tamanho
      if (arquivo.size > tamanhoMaximo) {
        this.util.exibirMensagemToast(`Arquivo ${arquivo.name}: muito grande (máx. 10MB)`, 3000);
        continue;
      }

      // Criar preview
      this.criarPreviewEvidencia(arquivo);
    }
  }

  private criarPreviewEvidencia(arquivo: File): void {
    const reader = new FileReader();
    reader.onload = (e: any) => {
      const evidencia = {
        arquivo: arquivo,
        nomeOriginal: arquivo.name,
        preview: e.target.result,
        tamanho: arquivo.size,
        tipo: arquivo.type,
        id: null, // Será preenchido após upload
        uploaded: false
      };

      this.evidencias.push(evidencia);
      
      // Fazer upload se o laudo já existe
      if (this.laudo.id && this.laudo.id > 0) {
        this.uploadEvidencia(evidencia);
      }
    };
    reader.readAsDataURL(arquivo);
  }

  private async uploadEvidencia(evidencia: any): Promise<void> {
    try {
      // Validar se temos os dados necessários
      if (!this.laudo.id || !this.laudo.usuario) {
        console.error('[EVIDÊNCIA] Dados inválidos para upload:', {
          laudoId: this.laudo.id,
          usuarioId: this.laudo.usuario
        });
        throw new Error('Laudo ID ou Usuário ID inválidos');
      }
      this.uploadProgress = 0;
      
      const formData = new FormData();
      formData.append('arquivo', evidencia.arquivo);
      formData.append('laudoId', this.laudo.id.toString());
      formData.append('usuarioId', this.laudo.usuario.toString());
      const response = await this.apiCad.uploadEvidenciaLaudo(formData) as UploadResponse;
      if (!response || typeof response !== 'object') {
        console.error('[EVIDÊNCIA] Resposta inválida da API:', response);
        throw new Error('Resposta inválida da API');
      }
      
      // Verificar se é uma resposta de erro estruturada
      if ('error' in response && response.error) {
        console.error('[EVIDÊNCIA] API retornou erro estruturado:', response.status, response.data);
        throw new Error(`API retornou erro: ${response.data?.mensagem || 'Erro desconhecido'}`);
      }
      
      // Verificar se é um erro HTTP (axios retorna erro quando status não é 2xx)
      if (response.status && response.status >= 400) {
        console.error('[EVIDÊNCIA] API retornou erro HTTP:', response.status, response.data);
        throw new Error(`API retornou erro HTTP: ${response.status}`);
      }
      
      // Verificar se é uma resposta de sucesso
      if (response.status === 200 || response.status === 201) {
        // Tentar diferentes formatos de ID (backend pode retornar 'Id' ou 'id')
        const evidenciaId = response.data?.Id || response.data?.id;
        const nomeArquivo = response.data?.NomeArquivo || response.data?.nomeArquivo;
        
        if (evidenciaId) {
          evidencia.id = evidenciaId;
          evidencia.uploaded = true;
          evidencia.nomeArquivo = nomeArquivo || evidencia.nomeOriginal;
          this.uploadProgress = 100;
          setTimeout(() => {
            this.uploadProgress = 0;
          }, 1000);
        } else {
          console.error('[EVIDÊNCIA] ID da evidência não retornado pela API');
          throw new Error('ID da evidência não retornado pela API');
        }
      } else {
        console.error('[EVIDÊNCIA] API retornou status inesperado:', response.status);
        throw new Error(`API retornou status inesperado: ${response.status}`);
      }
    } catch (error) {
      console.error('[EVIDÊNCIA] Erro no upload:', error);
      this.util.exibirMensagemToast('Erro ao enviar evidência', 3000);
      
      // Remover evidência que falhou no upload
      const index = this.evidencias.indexOf(evidencia);
      if (index > -1) {
        this.evidencias.splice(index, 1);
      }
    }
  }

  removerEvidencia(index: number): void {
    const evidencia = this.evidencias[index];
    
    if (evidencia.uploaded && evidencia.id) {
      // Remover do servidor
      this.apiCad.excluirEvidenciaLaudo(evidencia.id).then(() => {
        this.evidencias.splice(index, 1);
        this.util.exibirMensagemToast('Evidência removida', 2000);
      }).catch(() => {
        this.util.exibirMensagemToast('Erro ao remover evidência', 3000);
      });
    } else {
      // Remover apenas localmente
      this.evidencias.splice(index, 1);
    }
  }

  // Drag & Drop para reordenação
  onDragStart(event: DragEvent, index: number): void {
    this.dragIndex = index;
    event.dataTransfer?.setData('text/plain', index.toString());
    
    // Adicionar classe visual para feedback
    if (event.target) {
      (event.target as HTMLElement).classList.add('dragging');
    }
  }

  onDragEnd(event: DragEvent): void {
    this.dragIndex = -1;
    
    // Remover classe visual
    if (event.target) {
      (event.target as HTMLElement).classList.remove('dragging');
    }
  }

  onDragEnter(event: DragEvent, targetIndex: number): void {
    event.preventDefault();
    if (this.dragIndex !== -1 && this.dragIndex !== targetIndex) {
      (event.currentTarget as HTMLElement).classList.add('drag-over');
    }
  }

  onDragLeaveItem(event: DragEvent, targetIndex: number): void {
    event.preventDefault();
    (event.currentTarget as HTMLElement).classList.remove('drag-over');
  }

  onDragOverItem(event: DragEvent, targetIndex: number): void {
    event.preventDefault();
    
    if (this.dragIndex !== -1 && this.dragIndex !== targetIndex) {
      const draggedItem = this.evidencias[this.dragIndex];
      this.evidencias.splice(this.dragIndex, 1);
      this.evidencias.splice(targetIndex, 0, draggedItem);
      
      // Atualizar índices
      this.dragIndex = targetIndex;
      
      // Salvar nova ordem no servidor se necessário
      if (this.laudo.id && this.laudo.id > 0) {
        this.salvarOrdemEvidencias();
      }
    }
  }

  private async salvarOrdemEvidencias(): Promise<void> {
    try {
      const ordemIds = this.evidencias
        .filter(e => e.uploaded && e.id)
        .map(e => e.id);
      
      if (ordemIds.length > 0) {
        await this.apiCad.reordenarEvidenciasLaudo({
          laudoId: this.laudo.id,
          ordemEvidencias: ordemIds
        });
      }
    } catch (error) {
      console.error('[EVIDÊNCIA] Erro ao salvar ordem:', error);
    }
  }

  private async carregarEvidenciasExistentes(): Promise<void> {
    if (!this.laudo.id || this.laudo.id <= 0) return;

    try {
      const response = await this.apiCad.listarEvidenciasLaudo(this.laudo.id);
      if (response.status === 200 && response.data) {
        this.evidencias = response.data.map((evidencia: any) => ({
          id: evidencia.id,
          nomeOriginal: evidencia.nomeOriginal,
          nomeArquivo: evidencia.nomeArquivo,
          url: `/api/configuracoes/DownloadEvidenciaLaudo/${evidencia.id}`,
          uploaded: true,
          tamanho: evidencia.tamanho,
          tipo: evidencia.tipoArquivo
        }));
      }
    } catch (error) {
      console.error('[EVIDÊNCIA] Erro ao carregar evidências:', error);
    }
  }

  // 📸 Upload de evidências pendentes após salvar o laudo
  private async uploadEvidenciasPendentes(): Promise<void> {
    if (!this.laudo.id || this.laudo.id <= 0) {
      console.error('[EVIDÊNCIA] Laudo sem ID válido. Aguardando...');
      console.error('[EVIDÊNCIA] Estado do laudo:', this.laudo);
      // Aguardar um pouco e tentar novamente
      setTimeout(() => {
        if (this.laudo.id && this.laudo.id > 0) {
          this.uploadEvidenciasPendentes();
        } else {
          console.error('[EVIDÊNCIA] ID do laudo ainda não disponível após timeout');
        }
      }, 1000);
      return;
    }
    
    const evidenciasPendentes = this.evidencias.filter(e => !e.uploaded);
    if (evidenciasPendentes.length === 0) {
      return;
    }
    for (const evidencia of evidenciasPendentes) {
      try {
        await this.uploadEvidencia(evidencia);
      } catch (error) {
        console.error(`[EVIDÊNCIA] Erro ao enviar evidência ${evidencia.nomeOriginal}:`, error);
      }
    }
    
    this.util.exibirMensagemToast(`${evidenciasPendentes.length} evidência(s) enviada(s) com sucesso!`, 3000);
  }
}
