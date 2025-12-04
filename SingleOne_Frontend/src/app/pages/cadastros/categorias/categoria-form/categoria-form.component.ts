import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Categoria, CategoriasApiService } from '../../../../api/categorias/categorias-api.service';
@Component({
  selector: 'app-categoria-form',
  templateUrl: './categoria-form.component.html',
  styleUrls: ['./categoria-form.component.scss']
})
export class CategoriaFormComponent implements OnInit {
  @Input() categoria: Categoria | null = null;
  @Input() modo: 'criar' | 'editar' = 'criar';
  @Output() categoriaSalva = new EventEmitter<Categoria>();
  @Output() cancelado = new EventEmitter<void>();

  categoriaForm: FormGroup;
  carregando = false;

  constructor(
    private fb: FormBuilder,
    private categoriasService: CategoriasApiService
  ) {
    this.categoriaForm = this.fb.group({
      nome: ['', [Validators.required, Validators.maxLength(100)]],
      descricao: ['', [Validators.maxLength(500)]]
    });
  }

  ngOnInit(): void {
    if (this.categoria && this.modo === 'editar') {
      this.categoriaForm.patchValue({
        nome: this.categoria.nome,
        descricao: this.categoria.descricao
      });
    }
  }

  async onSubmit(): Promise<void> {
    if (this.categoriaForm.valid) {
      this.carregando = true;
      const dados = this.categoriaForm.value;
      if (this.modo === 'criar') {
        await this.criarCategoria(dados);
      } else {
        await this.atualizarCategoria(dados);
      }
    } else {
      console.error('Por favor, preencha todos os campos obrigatórios');
    }
  }

  private async criarCategoria(dados: any): Promise<void> {
    try {
      const response = await this.categoriasService.criarCategoria(dados);
      if (response.data.sucesso) {
        this.categoriaSalva.emit(response.data.dados);
      } else {
        console.error('Erro ao criar categoria:', response.data.mensagem);
      }
    } catch (error) {
      console.error('Erro ao criar categoria:', error);
    } finally {
      this.carregando = false;
    }
  }

  private async atualizarCategoria(dados: any): Promise<void> {
    if (!this.categoria) return;

    try {
      const dadosAtualizacao = {
        id: this.categoria.id,
        nome: dados.nome,
        descricao: dados.descricao,
        ativo: this.categoria.ativo  // ✅ IMPORTANTE: Manter o status atual!
      };
      const response = await this.categoriasService.atualizarCategoria(dadosAtualizacao);
      
      if (response.data.sucesso) {
        this.categoriaSalva.emit(response.data.dados);
      } else {
        console.error('Erro ao atualizar categoria:', response.data.mensagem);
      }
    } catch (error) {
      console.error('Erro ao atualizar categoria:', error);
    } finally {
      this.carregando = false;
    }
  }

  onCancelar(): void {
    this.cancelado.emit();
  }

  getTitulo(): string {
    return this.modo === 'criar' ? 'Nova Categoria' : 'Editar Categoria';
  }

  getBotaoTexto(): string {
    return this.carregando 
      ? (this.modo === 'criar' ? 'Criando...' : 'Atualizando...')
      : (this.modo === 'criar' ? 'Criar Categoria' : 'Atualizar Categoria');
  }
}
