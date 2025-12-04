import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';
import { LaudoVisualizarComponent } from '../laudo-visualizar/laudo-visualizar.component';

@Component({
  selector: 'app-laudos',
  templateUrl: './laudos.component.html',
  styleUrls: ['./laudos.component.scss']
})
export class LaudosComponent implements OnInit {

  private session:any = {};
  public colunas = ['id', 'recurso', 'abertura', 'finalizacao', 'tecnico', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  public dadosOriginais: any[] = [];
  
  // 🎯 PROPRIEDADES DO MODAL
  public mostrarFormulario = false;
  public mostrarVisualizacao = false;
  public mostrarModalEvidencias = false;
  public mostrarModalFinalizar = false; // ✅ ADICIONADO: Controla modal de finalizar sinistro
  public laudoEditando: any = null;
  public laudoVisualizando: any = null;
  public laudoFinalizando: any = null; // ✅ ADICIONADO: Sinistro sendo finalizado
  public modoFormulario: 'criar' | 'editar' = 'criar';
  
  // Variáveis para upload de evidências
  public arquivosSelecionados: File[] = [];
  public isDragOver = false;
  public fazendoUpload = false;
  public filePreviews: Map<File, string> = new Map();
  public tituloEvidencias = 'Adicionar Evidências';

  constructor(private util: UtilService, private api: ConfiguracoesApiService, private route: Router, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    
    // Inicializar com lista vazia para evitar erros
    this.dataSource = new MatTableDataSource<any>([]);
    
    // Configuração do observable de busca com debounce
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(500),
      tap(value => {
        this.buscar(value);
      })
    );
    // Inscrição no observable
    this.resultado.subscribe();
    
    // Listener para abrir modal de evidências
    window.addEventListener('abrirModalEvidencias', (event: any) => {
      this.laudoEditando = { id: event.detail.laudoId };
      this.abrirModalEvidencias();
    });
    
    this.listar();
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  // 🔧 MÉTODO AUXILIAR PARA CONFIGURAR PAGINADOR
  private configurarPaginador() {
    if (!this.paginator || !this.dataSource) {
      return;
    }
    
    // CONFIGURAÇÃO SIMPLES E DIRETA
    this.dataSource.paginator = this.paginator;
    
    // CONFIGURAR TAMANHO INICIAL
    this.paginator.pageSize = 10;
    this.paginator.pageIndex = 0;
    
    // ADICIONAR LISTENER PARA MUDANÇAS
    this.paginator.page.subscribe(() => {
      // FORÇAR ATUALIZAÇÃO DA VIEW
      this.cdr.detectChanges();
      this.cdr.markForCheck();
    });
  }

  listar() {
    this.util.aguardar(true);
    this.api.listarLaudos("null", this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        if (res.data && res.data.length > 0) {
        }
        this.dadosOriginais = res.data || [];
        this.dataSource = new MatTableDataSource<any>(res.data || []);
        this.configurarPaginador();
      }
    }).catch(err => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    })
  }

  buscar(valor) {
    if (valor && valor.trim() !== '') {
      // Se temos dados originais, usar filtro local primeiro para resposta rápida
      if (this.dadosOriginais && this.dadosOriginais.length > 0) {
        this.aplicarFiltroLocal(valor);
      }
      
      // Fazer busca no servidor para resultados mais precisos
      this.util.aguardar(true);
      
      // Busca inteligente: tentar diferentes variações da pesquisa
      const termosBusca = this.gerarTermosBusca(valor);
      
      // Tentar primeiro com o termo completo
      this.fazerBuscaServidor(valor, termosBusca);
    }
    else {
      // Se o valor estiver vazio, recarregar a lista completa
      this.listar();
    }
  }

  // Método para gerar termos de busca inteligentes
  gerarTermosBusca(valor: string): string[] {
    const termos = [valor.toLowerCase()];
    
    // Adicionar palavras individuais
    const palavras = valor.toLowerCase().split(' ').filter(p => p.length > 2);
    termos.push(...palavras);
    
    // Adicionar variações comuns
    if (valor.toLowerCase().includes('smartphone')) {
      termos.push('celular', 'mobile', 'phone');
    }
    if (valor.toLowerCase().includes('samsung')) {
      termos.push('samsung');
    }
    if (valor.toLowerCase().includes('a33')) {
      termos.push('a33', 'a 33');
    }
    
    // Remover duplicatas
    return [...new Set(termos)];
  }

  // Método para fazer busca no servidor
  fazerBuscaServidor(termoPrincipal: string, termosAlternativos: string[]) {
    this.api.listarLaudos(termoPrincipal, this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        let resultados = res.data || [];
        
        // Se não encontrou resultados, tentar com termos alternativos
        if (resultados.length === 0 && termosAlternativos.length > 1) {
          this.tentarTermosAlternativos(termosAlternativos.slice(1));
          return;
        }
        
        this.dadosOriginais = resultados;
        this.dataSource = new MatTableDataSource(resultados);
        this.configurarPaginador();
      }
    }).catch(err => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  // Método para tentar termos alternativos
  tentarTermosAlternativos(termos: string[]) {
    if (termos.length === 0) {
      return;
    }
    
    const proximoTermo = termos[0];
    
    this.api.listarLaudos(proximoTermo, this.cliente, this.session.token).then(res => {
      if (res.status == 200 || res.status == 204) {
        const resultados = res.data || [];
        if (resultados.length > 0) {
          this.dadosOriginais = resultados;
          this.dataSource = new MatTableDataSource(resultados);
          this.configurarPaginador();
          return;
        }
      }
      
      // Tentar próximo termo
      this.tentarTermosAlternativos(termos.slice(1));
    }).catch(err => {
      // Tentar próximo termo mesmo com erro
      this.tentarTermosAlternativos(termos.slice(1));
    });
  }

  limparBusca() {
    this.consulta.setValue('');
    this.listar();
  }

  onBuscaInput(event: any) {
    // Método mantido para compatibilidade, mas não é mais necessário
    // A busca é feita automaticamente pelo observable
  }

  testarBusca() {
    // Testar com diferentes tipos de busca
    const testes = [
      'notebook', 
      '12345', 
      'João', 
      'danificado', 
      'Smartphone Samsung A33',
      'Samsung',
      'A33',
      'Smartphone'
    ];
    const testeAleatorio = testes[Math.floor(Math.random() * testes.length)];
    
    this.buscar(testeAleatorio);
  }

  verificarDados() {
    // Método mantido para compatibilidade, mas sem logs
  }

  // Método para filtro local rápido (opcional)
  aplicarFiltroLocal(valor: string) {
    if (this.dadosOriginais && this.dadosOriginais.length > 0) {
      const filtro = valor.toLowerCase();
      
      // Busca mais inteligente - dividir em palavras
      const palavras = filtro.split(' ').filter(p => p.length > 0);
      
      // Adicionar variações comuns para busca local
      const termosBusca = [...palavras];
      if (filtro.includes('smartphone')) {
        termosBusca.push('celular', 'mobile', 'phone');
      }
      if (filtro.includes('samsung')) {
        termosBusca.push('samsung');
      }
      if (filtro.includes('a33')) {
        termosBusca.push('a33', 'a 33');
      }
      
      const dadosFiltrados = this.dadosOriginais.filter(item => 
        termosBusca.some(termo => 
          item.equipamento?.toLowerCase().includes(termo) ||
          item.numeroserie?.toLowerCase().includes(termo) ||
          item.tecniconome?.toLowerCase().includes(termo) ||
          item.usuarionome?.toLowerCase().includes(termo) ||
          item.patrimonio?.toLowerCase().includes(termo) ||
          item.descricao?.toLowerCase().includes(termo)
        )
      );
      this.dataSource.data = dadosFiltrados;
      this.configurarPaginador();
    }
  }

// 🆕 MÉTODOS PARA CONTROLAR O MODAL
  novoSinistro() {
    this.laudoEditando = null;
    this.modoFormulario = 'criar';
    this.mostrarFormulario = true;
  }
  
  onSinistroSalvo(sinistro: any) {
    this.mostrarFormulario = false;
    this.laudoEditando = null;
    this.listar(); // Recarregar lista
  }

  onSinistroFinalizado(event: any) {
    this.mostrarModalFinalizar = false;
    this.laudoFinalizando = null;
    this.listar();
    this.util.exibirMensagemToast('Sinistro finalizado com sucesso!', 5000);
  }

  onCancelado() {
    this.mostrarFormulario = false;
    this.laudoEditando = null;
  }

  // 🆕 MÉTODOS PARA CONTROLAR A VISUALIZAÇÃO
  visualizar(row) {
    this.laudoVisualizando = row;
    this.mostrarVisualizacao = true;
  }

  fecharVisualizacao() {
    this.mostrarVisualizacao = false;
    this.laudoVisualizando = null;
  }

  excluir(obj) {
    if(confirm('Deseja realmente excluir o laudo ' + obj.id + '?')) {
      this.util.aguardar(true);
      this.api.encerrarLaudo(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Laudo encerrado com sucesso!', 5000);
          this.listar();
        }
      })
    }
  }

  encerrar(obj) {
    // ✅ MODIFICADO: Abre modal de finalizar sinistro em vez de navegar
    this.laudoFinalizando = obj;
    this.mostrarModalFinalizar = true;
  }

  imprimir(ld) {
    // Buscar templates disponíveis para sinistros (tipo ID=4)
    this.api.listarTemplatesPorTipo(this.session.usuario.cliente, 4, this.session.token).then(res => {
      if (res.status === 200 && res.data && res.data.length > 0) {
        // Se há templates, usar o primeiro disponível
        const template = res.data[0]; // Usar o primeiro template encontrado
        this.gerarPDFComTemplate(ld.id, template.id);
      } else {
        // Se não há templates, mostrar mensagem informativa
        this.util.exibirMensagemToast('É necessário configurar um template de sinistro antes de imprimir. Acesse Configurações > Templates.', 8000);
      }
    }).catch(err => {
      // Em caso de erro, mostrar mensagem
      this.util.exibirMensagemToast('Erro ao verificar templates. Verifique se há um template configurado.', 5000);
    });
  }
  
  // Métodos de seleção de template removidos - agora é automático
  
  gerarPDFComTemplate(laudoId: number, templateId: number | null) {
    this.util.aguardar(true);
    this.api.gerarLaudoEmPDF(laudoId, this.session.token, templateId).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        this.util.gerarDocumentoNovaGuia(res.data);
      } else {
        this.util.exibirMensagemToast('Falha de comunicação com o serviço...', 5000);
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error(`[LAUDO] Erro na API:`, err);
      this.util.exibirMensagemToast('Falha de comunicação com o serviço...', 5000);
    });
  }

  // Getter para dados paginados
  get dadosPaginados(): any[] {
    if (!this.dataSource || !this.dataSource.paginator) {
      return this.dataSource?.data || [];
    }
    
    const startIndex = this.dataSource.paginator.pageIndex * this.dataSource.paginator.pageSize;
    const endIndex = startIndex + this.dataSource.paginator.pageSize;
    return this.dataSource.data.slice(startIndex, endIndex);
  }

  // 🎯 MÉTODOS PARA O MODAL DE EVIDÊNCIAS
  abrirModalEvidencias() {
    this.mostrarModalEvidencias = true;
    this.arquivosSelecionados = [];
    this.isDragOver = false;
    
    // Definir título baseado no contexto
    if (this.laudoEditando && this.laudoEditando.id) {
      this.tituloEvidencias = `Adicionar Evidências - Sinistro #${this.laudoEditando.id}`;
    } else {
      this.tituloEvidencias = 'Adicionar Evidências';
    }
  }

  fecharModalEvidencias() {
    this.mostrarModalEvidencias = false;
    
    // Limpar todos os previews
    this.filePreviews.forEach((url, file) => {
      URL.revokeObjectURL(url);
    });
    this.filePreviews.clear();
    
    this.arquivosSelecionados = [];
    this.isDragOver = false;
  }

  // ✅ ADICIONADO: Método para fechar modal de finalizar sinistro
  fecharModalFinalizar() {
    this.mostrarModalFinalizar = false;
    this.laudoFinalizando = null;
  }

  triggerFileInput() {
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    if (fileInput) {
      fileInput.click();
    }
  }

  onFileSelected(event: any) {
    const files = Array.from(event.target.files || []) as File[];
    this.processarArquivos(files);
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
    
    const files = Array.from(event.dataTransfer?.files || []) as File[];
    this.processarArquivos(files);
  }

  processarArquivos(files: File[]) {
    const arquivosValidos = files.filter(file => {
      // Validar tipo de arquivo
      const tiposAceitos = ['image/jpeg', 'image/png', 'application/pdf'];
      if (!tiposAceitos.includes(file.type)) {
        this.util.exibirMensagemToast(`Tipo de arquivo não suportado: ${file.name}`, 3000);
        return false;
      }
      
      // Validar tamanho (5MB)
      const maxSize = 5 * 1024 * 1024; // 5MB em bytes
      if (file.size > maxSize) {
        this.util.exibirMensagemToast(`Arquivo muito grande: ${file.name} (máx. 5MB)`, 3000);
        return false;
      }
      
      return true;
    });

    // Adicionar arquivos válidos à lista
    this.arquivosSelecionados.push(...arquivosValidos);
    
    // Gerar previews para imagens
    arquivosValidos.forEach(file => {
      if (this.isImageFile(file)) {
        this.gerarPreview(file);
      }
    });
    
    if (arquivosValidos.length > 0) {
      this.util.exibirMensagemToast(`${arquivosValidos.length} arquivo(s) adicionado(s) com sucesso!`, 3000);
    }
  }

  removerArquivo(index: number) {
    const arquivo = this.arquivosSelecionados[index];
    this.arquivosSelecionados.splice(index, 1);
    
    // Limpar preview do arquivo removido
    if (this.filePreviews.has(arquivo)) {
      URL.revokeObjectURL(this.filePreviews.get(arquivo)!);
      this.filePreviews.delete(arquivo);
    }
  }

  isImageFile(file: File): boolean {
    return file.type.startsWith('image/');
  }

  gerarPreview(file: File): void {
    if (this.isImageFile(file)) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.filePreviews.set(file, e.target.result);
        this.cdr.detectChanges(); // Forçar atualização da view
      };
      reader.readAsDataURL(file);
    }
  }

  getFilePreview(file: File): string {
    return this.filePreviews.get(file) || '';
  }

  getFileIcon(fileName: string): string {
    const extension = fileName.toLowerCase().split('.').pop();
    switch (extension) {
      case 'pdf':
        return 'cil-file-pdf';
      case 'jpg':
      case 'jpeg':
      case 'png':
        return 'cil-image';
      default:
        return 'cil-file';
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  async fazerUpload() {
    if (this.arquivosSelecionados.length === 0) {
      this.util.exibirMensagemToast('Nenhum arquivo selecionado!', 3000);
      return;
    }

    this.fazendoUpload = true;
    
    try {
      let uploadsSucesso = 0;
      let uploadsErro = 0;
      
      for (const arquivo of this.arquivosSelecionados) {
        try {
          // Criar FormData para upload
          const formData = new FormData();
          formData.append('arquivo', arquivo);
          formData.append('laudoId', this.laudoEditando.id.toString());
          formData.append('usuarioId', this.session.usuario.id.toString());
          
          // Fazer upload real para a API
          const response = await this.api.uploadEvidenciaLaudo(formData);
          
          if (response.status === 200) {
            uploadsSucesso++;
          } else {
            uploadsErro++;
            console.error(`Erro no upload de ${arquivo.name}:`, response);
          }
          
        } catch (error) {
          uploadsErro++;
          console.error(`Erro no upload de ${arquivo.name}:`, error);
        }
      }
      
      // Exibir resultado
      if (uploadsSucesso > 0) {
        this.util.exibirMensagemToast(`${uploadsSucesso} evidência(s) enviada(s) com sucesso!`, 3000);
      }
      
      if (uploadsErro > 0) {
        this.util.exibirMensagemToast(`${uploadsErro} evidência(s) falharam no envio.`, 3000);
      }
      
      // Fechar modal apenas se pelo menos um upload foi bem-sucedido
      if (uploadsSucesso > 0) {
        this.fecharModalEvidencias();
        
        // Recarregar evidências se estiver na tela de visualização
        if (this.mostrarVisualizacao && this.laudoVisualizando) {
          // Disparar evento para recarregar evidências
          window.dispatchEvent(new CustomEvent('recarregarEvidencias', {
            detail: { laudoId: this.laudoEditando.id }
          }));
        }
      }
      
    } catch (error) {
      console.error('Erro geral no upload:', error);
      this.util.exibirFalhaComunicacao();
    } finally {
      this.fazendoUpload = false;
    }
  }
  
  // Propriedades de template removidas - agora é automático
}
